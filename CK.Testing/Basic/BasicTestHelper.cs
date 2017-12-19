using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using CK.Core;
using CK.Text;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="IBasicTestHelper"/>.
    /// </summary>
    public class BasicTestHelper : IBasicTestHelper
    {
        static readonly string[] _allowedConfigurations = new[] { "Debug", "Release" };
        NormalizedPath _binFolder;
        string _buildConfiguration;
        NormalizedPath _testProjectFolder;
        string _testProjectName;
        NormalizedPath _repositoryFolder;
        NormalizedPath _solutionFolder;
        NormalizedPath _logFolder;
        event EventHandler<CleanupFolderEventArgs> _onCleanupFolder;
        bool _isTestHost;

        string IBasicTestHelper.BuildConfiguration => Initalize( ref _buildConfiguration );

        string IBasicTestHelper.TestProjectName => Initalize( ref _testProjectName );

        bool IBasicTestHelper.IsTestHost => Initalize( ref _isTestHost );

        NormalizedPath IBasicTestHelper.RepositoryFolder => Initalize( ref _repositoryFolder );

        NormalizedPath IBasicTestHelper.SolutionFolder => Initalize( ref _solutionFolder );

        NormalizedPath IBasicTestHelper.LogFolder => Initalize( ref _logFolder );

        NormalizedPath IBasicTestHelper.TestProjectFolder => Initalize( ref _testProjectFolder );

        NormalizedPath IBasicTestHelper.BinFolder => Initalize( ref _binFolder );

        void IBasicTestHelper.CleanupFolder( string folder, int maxRetryCount )
        {
            int tryCount = 0;
            for(; ; )
            {
                try
                {
                    if( Directory.Exists( folder ) ) Directory.Delete( folder, true );
                    Directory.CreateDirectory( folder );
                    File.WriteAllText( Path.Combine( folder, "TestWrite.txt" ), "Test write works." );
                    File.Delete( Path.Combine( folder, "TestWrite.txt" ) );
                    _onCleanupFolder?.Invoke( this, new CleanupFolderEventArgs( folder ) );
                    return;
                }
                catch( Exception )
                {
                    if( ++tryCount > maxRetryCount ) throw;
                    Thread.Sleep( 100 );
                }
            }
        }

        event EventHandler<CleanupFolderEventArgs> IBasicTestHelper.OnCleanupFolder
        {
            add => _onCleanupFolder += value;
            remove => _onCleanupFolder -= value;
        }

        static HashSet<string> _onlyOnce = new HashSet<string>();

        void IBasicTestHelper.OnlyOnce( Action a, string s, int l )
        {
            var key = s + l.ToString();
            lock( _onlyOnce )
            {
                if( _onlyOnce.Add( key ) ) a();
            }
        }

        T Initalize<T>( ref T varString )
        {
            if( _buildConfiguration != null ) return varString;
            string p = _binFolder = AppContext.BaseDirectory;
            string buildConfDir = null;
            foreach( var config in _allowedConfigurations )
            {
                buildConfDir = FindAbove( p, config );
                if( buildConfDir != null )
                {
                    _buildConfiguration = config;
                    break;
                }
            }
            if( _buildConfiguration == null )
            {
                throw new InvalidOperationException( $"Unable to find parent folder named '{_allowedConfigurations.Concatenate("' or '")}' above '{_binFolder}'." );
            }
            p = Path.GetDirectoryName( buildConfDir );
            if( Path.GetFileName( p ) != "bin" )
            {
                throw new InvalidOperationException( $"Folder '{_buildConfiguration}' MUST be in 'bin' folder (above '{_binFolder}')." );
            }
            _testProjectFolder = p = Path.GetDirectoryName( p );
            _testProjectName = Path.GetFileName( p );
            Assembly entry = Assembly.GetEntryAssembly();
            if( entry != null )
            {
                string assemblyName = entry.GetName().Name;
                if( _testProjectName != assemblyName )
                {
                    if( assemblyName == "testhost" )
                    {
                        _isTestHost = true;
                    }
                    else
                    {
                        throw new InvalidOperationException( $"Current test project assembly is '{assemblyName}' but folder is '{_testProjectName}' (above '{_buildConfiguration}' in '{_binFolder}')." );
                    }
                }
            }
            p = Path.GetDirectoryName( p );

            string testsFolder = null;
            bool hasGit = false;
            while( p != null && !(hasGit = Directory.Exists( Path.Combine( p, ".git" ) )) )
            {
                if( Path.GetFileName( p ) == "Tests" ) testsFolder = p;
                p = Path.GetDirectoryName( p );
            }
            if( !hasGit ) throw new InvalidOperationException( $"The project must be in a git repository (above '{_binFolder}')." );
            _repositoryFolder = p;
            if( testsFolder == null )
            {
                throw new InvalidOperationException( $"A parent 'Tests' folder must exist above '{_testProjectFolder}'." );
            }
            _solutionFolder = Path.GetDirectoryName( testsFolder );
            _logFolder = Path.Combine( _testProjectFolder, "Logs" );
            return varString;
        }

        static string FindAbove( string path, string folderName )
        {
            while( path != null && Path.GetFileName( path ) != folderName )
            {
                path = Path.GetDirectoryName( path );
            }
            return path;
        }

        /// <summary>
        /// Gets the <see cref="IBasicTestHelper"/> default implementation.
        /// </summary>
        public static IBasicTestHelper TestHelper => TestHelperResolver.Default.Resolve<IBasicTestHelper>();
    }
}
