using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using CK.Core;

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
            if( _initializationError != null ) _initializationError.Throw();
        }

        internal BasicTestHelper( TestHelperConfiguration config )
        {
            var p = config.GetPath( "TestHelper/ClosestSUTProjectFolder" );
            if( p is not null )
            {
                _closestSUTProjectFolder = p.Value;
            }
            else
            {
                _closestSUTProjectFolder = GetClosestSUTProjectCandidatePaths( _solutionFolder, _testProjectFolder )
                                            .FirstOrDefault( p => System.IO.Directory.Exists( p ) );
                if( _closestSUTProjectFolder.IsEmptyPath )
                {
                    _closestSUTProjectFolder = _testProjectFolder;
                }
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
            Throw.CheckArgument( testProjectFolder.Parts.Contains( "Tests" ) );

            string? targetName = null;
            if( testProjectFolder.LastPart.EndsWith( ".Tests" ) ) targetName = testProjectFolder.LastPart.Substring( 0, testProjectFolder.LastPart.Length - 6 );
            else if( testProjectFolder.LastPart.EndsWith( "Tests" ) ) targetName = testProjectFolder.LastPart.Substring( 0, testProjectFolder.LastPart.Length - 5 );
            if( !String.IsNullOrEmpty( targetName ) )
            {
                var testsSubPaths = ExpandTestsSubPaths( solutionFolder.Parts.Count, testProjectFolder );
                return testProjectFolder.PathsToFirstPart( testsSubPaths, new[] { targetName } )
                                        .Where( p => !testProjectFolder.StartsWith( p ) );
            }
            return Array.Empty<NormalizedPath>();

            static IEnumerable<NormalizedPath> ExpandTestsSubPaths( int solutionFolderPartsCount, NormalizedPath testProjectFolder )
            {
                var testProjectParentFolder = testProjectFolder.RemoveLastPart();
                int iTests = solutionFolderPartsCount;
                for(; ; )
                {
                    while( iTests < testProjectFolder.Parts.Count && testProjectFolder.Parts[iTests] != "Tests" ) iTests++;
                    if( iTests == testProjectFolder.Parts.Count ) break;
                    var fromTestsToProject = testProjectParentFolder.RemoveParts( 0, ++iTests );
                    foreach( var p in fromTestsToProject.Parents ) yield return p;
                }
                yield return default;
            }
        }

        /// <summary>
        /// Gets the <see cref="IBasicTestHelper"/> default implementation.
        /// </summary>
        public static IBasicTestHelper TestHelper => TestHelperResolver.Default.Resolve<IBasicTestHelper>();

    }
}
