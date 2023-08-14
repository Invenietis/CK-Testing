using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading;
using CK.Core;
using Microsoft.IO;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="IBasicTestHelper"/>.
    /// </summary>
    public sealed class BasicTestHelper : StaticBasicTestHelper, IBasicTestHelper
    {
        readonly NormalizedPath _closestSUTProjectFolder;

        static BasicTestHelper()
        {
            EnsureInitialized();
        }

        internal BasicTestHelper( TestHelperConfiguration config )
        {
            var (c,p) = config.DeclarePath( "TestHelper/ClosestSUTProjectFolder",
                                            description: $"The closest directory named '{_testProjectFolder.LastPart}.SUT' or '{_testProjectFolder.LastPart}' in the Solution. This test project if not found.",
                                            editableValue: null );
            if( p is not null )
            {
                _closestSUTProjectFolder = p.Value;
            }
            else
            {
                _closestSUTProjectFolder = GetClosestSUTProjectCandidatePaths( _solutionFolder, _testProjectFolder )
                                            .FirstOrDefault( p => Directory.Exists( p ) );
                if( _closestSUTProjectFolder.IsEmptyPath )
                {
                    _closestSUTProjectFolder = _testProjectFolder;
                }
                c.SetDefaultValue( _closestSUTProjectFolder );
            }
            if( config._basic != null )
            {
                Throw.InvalidOperationException( "Resolver context error: the configuration object is already bound to a BasicTestHelper. Provides a new TestHelperConfiguration to the resolver or reuse already resolved helpers." );
            }
            config._basic = this;
        }

        event EventHandler<CleanupFolderEventArgs>? _onCleanupFolder;

        string IBasicTestHelper.BuildConfiguration => _buildConfiguration;

        NormalizedPath IBasicTestHelper.SolutionFolder => _solutionFolder;

        NormalizedPath IBasicTestHelper.ClosestSUTProjectFolder => _closestSUTProjectFolder;

        NormalizedPath IBasicTestHelper.LogFolder => _logFolder;

        NormalizedPath IBasicTestHelper.TestProjectFolder => _testProjectFolder;

        NormalizedPath IBasicTestHelper.BinFolder => _binFolder;

        NormalizedPath IBasicTestHelper.PathToBin => _pathToBin;

        NormalizedPath IBasicTestHelper.CleanupFolder( NormalizedPath folder, bool ensureFolderAvailable, int maxRetryCount )
        {
            Throw.CheckArgument( !folder.IsEmptyPath );
            int tryCount = 0;
            for(; ; )
            {
                try
                {
                    if( Directory.Exists( folder ) ) Directory.Delete( folder, true );
                    if( ensureFolderAvailable )
                    {
                        Directory.CreateDirectory( folder );
                        File.WriteAllText( Path.Combine( folder, "TestWrite.txt" ), "Test write works." );
                        File.Delete( Path.Combine( folder, "TestWrite.txt" ) );
                    }
                    _onCleanupFolder?.Invoke( this, new CleanupFolderEventArgs( folder, ensureFolderAvailable ) );
                    return folder;
                }
                catch( Exception )
                {
                    if( ++tryCount > maxRetryCount ) throw;
                    Thread.Sleep( 100 );
                }
            }
        }

        event EventHandler<CleanupFolderEventArgs>? IBasicTestHelper.OnCleanupFolder
        {
            add => _onCleanupFolder += value;
            remove => _onCleanupFolder -= value;
        }

        void IBasicTestHelper.OnlyOnce( Action a, string? s, int l )
        {
            var key = s + l.ToString();
            bool shouldRun;
            lock( _onlyOnce )
            {
                shouldRun = _onlyOnce.Add( key );
            }
            if( shouldRun ) a();
        }

        T IBasicTestHelper.JsonIdempotenceCheck<T>( T o,
                                                    Action<Utf8JsonWriter, T> write,
                                                    Utf8JsonReaderDelegate<T> read,
                                                    Action<string>? jsonText )
        {
            using( var m = (RecyclableMemoryStream)Util.RecyclableStreamManager.GetStream() )
            using( Utf8JsonWriter w = new Utf8JsonWriter( (IBufferWriter<byte>)m ) )
            {
                write( w, o );
                w.Flush();
                string? text1 = Encoding.UTF8.GetString( m.GetReadOnlySequence() );
                jsonText?.Invoke( text1 );
                var reader = new Utf8JsonReader( m.GetReadOnlySequence() );
                var oBack = read( ref reader );
                if( oBack == null )
                {
                    Throw.CKException( $"A null has been read back from '{text1}' for a non null instance of '{typeof( T ).ToCSharpName()}'." );
                }
                string? text2 = null;
                m.Position = 0;
                using( var w2 = new Utf8JsonWriter( (IBufferWriter<byte>)m ) )
                {
                    write( w2, oBack );
                    w2.Flush();
                    text2 = Encoding.UTF8.GetString( m.GetReadOnlySequence() );
                }
                if( text1 != text2 )
                {
                    Throw.CKException( $"""
                            Json idempotence failure between first write:
                            {text1}

                            And second write of the read back {typeof( T ).ToCSharpName()} instance:
                            {text2}

                            """ );
                }
                return oBack;
            }

            /// <summary>
            /// Enumerates the <see cref="IBasicTestHelper.ClosestSUTProjectFolder"/> candidate paths, starting with the best one.
            /// This is public to ease tests and because it may be useful.
            /// </summary>
            /// <param name="solutionFolder">The root folder: nothing happen above this one.</param>
            /// <param name="testProjectFolder">The test project that must be in <paramref name="solutionFolder"/> and contains at least one "Tests" part.</param>
            /// <returns>The closest SUT path in order of preference.</returns>
            public static IEnumerable<NormalizedPath> GetClosestSUTProjectCandidatePaths( NormalizedPath solutionFolder, NormalizedPath testProjectFolder )
        {
            Throw.CheckArgument( testProjectFolder.StartsWith( solutionFolder ) );

            string? targetName = null;
            if( testProjectFolder.LastPart.EndsWith( ".Tests" ) ) targetName = testProjectFolder.LastPart.Substring( 0, testProjectFolder.LastPart.Length - 6 );
            else if( testProjectFolder.LastPart.EndsWith( "Tests" ) ) targetName = testProjectFolder.LastPart.Substring( 0, testProjectFolder.LastPart.Length - 5 );
            if( !String.IsNullOrEmpty( targetName ) )
            {
                var cache = new List<NormalizedPath>();
                // The .SUT always has the priority, wherever it is.
                var sutTargetName = targetName + ".SUT";
                foreach( var p in GetClosestCandidates( solutionFolder.Parts.Count, testProjectFolder.RemoveLastPart(), sutTargetName ) )
                {
                    cache.Add( p );
                    yield return p;
                }
                // Then we use the cache to avoid recomputing the combinations.
                // Rationale: their should be much less SUT than regular assemblies, the first round will often not succeeds,
                // we'll often have to replay the list...
                // Note: the list's length depends on the number of parts (the depth of the testProjectFolder).
                foreach( var c in cache )
                {
                    yield return c.RemoveLastPart().AppendPart( targetName );
                }
            }
        }

        /// <summary>
        /// Enumerates a set of lookup paths from a folder starting with the best one (implements <see cref="IBasicTestHelper.ClosestSUTProjectFolder"/>).
        /// This is public to ease tests and because it may be useful.
        /// </summary>
        /// <param name="rootCount">The root length: nothing will return above this one.</param>
        /// <param name="startFolder">The starting folder.</param>
        /// <param name="targetName">The leaf directory name to lookup.</param>
        /// <param name="skipDirectParentFolder">False to allow candidates to be parent folders of <paramref name="startFolder"/>.</param>
        /// <returns>The closest paths in order of preference.</returns>
        public static IEnumerable<NormalizedPath> GetClosestCandidates( int rootCount,
                                                                        NormalizedPath startFolder,
                                                                        string targetName,
                                                                        bool skipDirectParentFolder = true )
        {
            var head = startFolder;
            var subPaths = new List<NormalizedPath>();

            static IEnumerable<NormalizedPath> WithSubPaths( NormalizedPath startFolder,
                                                             bool skipDirectParentFolder,
                                                             List<NormalizedPath> subPaths,
                                                             ref NormalizedPath head )
            {
                static IEnumerable<NormalizedPath> GenerateWithSubPaths( NormalizedPath startFolder,
                                                                         bool skipDirectParentFolder,
                                                                         List< NormalizedPath> subPaths,
                                                                         NormalizedPath head,
                                                                         string lastPart )
                {
                    foreach( var subPath in subPaths )
                    {
                        var p = head.Combine( subPath );
                        if( !skipDirectParentFolder || !startFolder.StartsWith( p ) )
                        yield return p;
                    }
                    int c = subPaths.Count;
                    NormalizedPath h = new NormalizedPath( lastPart );
                    for( int i = 0; i < c; i++ )
                    {
                        subPaths.Add( h.Combine( subPaths[i] ) );
                    }
                }

                var lastPart = head.LastPart;
                head = head.RemoveLastPart();
                return GenerateWithSubPaths( startFolder, skipDirectParentFolder, subPaths, head, lastPart );
            }

            subPaths.Add( targetName );
            yield return head.AppendPart( targetName );
            while( head.Parts.Count >= rootCount )
            {
                foreach( var s in WithSubPaths( startFolder, skipDirectParentFolder, subPaths, ref head ) ) yield return s;
            }
        }

        /// <summary>
        /// Gets the <see cref="IBasicTestHelper"/> default implementation.
        /// </summary>
        public static IBasicTestHelper TestHelper => TestHelperResolver.Default.Resolve<IBasicTestHelper>();

    }
}
