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
        internal static readonly NormalizedPath _binFolder;
        internal static readonly string _buildConfiguration;
        internal static readonly NormalizedPath _testProjectFolder;
        internal static readonly string _testProjectName;
        internal static readonly NormalizedPath _repositoryFolder;
        internal static readonly NormalizedPath _solutionFolder;
        internal static readonly NormalizedPath _logFolder;
        internal static readonly bool _isTestHost;
        internal static readonly HashSet<string> _onlyOnce;

        static BasicTestHelper()
        {
            _onlyOnce = new HashSet<string>();
            string p = AppContext.BaseDirectory;
            _binFolder = p;
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
                throw new InvalidOperationException( $"Initialization error: Unable to find parent folder named '{_allowedConfigurations.Concatenate( "' or '" )}' above '{_binFolder}'." );
            }
            p = Path.GetDirectoryName( buildConfDir );
            if( Path.GetFileName( p ) != "bin" )
            {
                throw new InvalidOperationException( $"Initialization error: Folder '{_buildConfiguration}' MUST be in 'bin' folder (above '{_binFolder}')." );
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
                        throw new InvalidOperationException( $"Initialization error: Current test project assembly is '{assemblyName}' but folder is '{_testProjectName}' (above '{_buildConfiguration}' in '{_binFolder}')." );
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
            if( !hasGit ) throw new InvalidOperationException( $"Initialization error: The project must be in a git repository (above '{_binFolder}')." );
            _repositoryFolder = p;
            if( testsFolder == null )
            {
                throw new InvalidOperationException( $"Initialization error: A parent 'Tests' folder must exist above '{_testProjectFolder}'." );
            }
            _solutionFolder = Path.GetDirectoryName( testsFolder );
            _logFolder = Path.Combine( _testProjectFolder, "Logs" );
        }

        event EventHandler<CleanupFolderEventArgs> _onCleanupFolder;

        string IBasicTestHelper.BuildConfiguration => _buildConfiguration;

        string IBasicTestHelper.TestProjectName => _testProjectName;

        bool IBasicTestHelper.IsTestHost => _isTestHost;

        NormalizedPath IBasicTestHelper.RepositoryFolder => _repositoryFolder;

        NormalizedPath IBasicTestHelper.SolutionFolder => _solutionFolder;

        NormalizedPath IBasicTestHelper.LogFolder => _logFolder;

        NormalizedPath IBasicTestHelper.TestProjectFolder => _testProjectFolder;

        NormalizedPath IBasicTestHelper.BinFolder => _binFolder;

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

        void IBasicTestHelper.OnlyOnce( Action a, string s, int l )
        {
            var key = s + l.ToString();
            bool shouldRun;
            lock( _onlyOnce )
            {
                shouldRun = _onlyOnce.Add( key );
            }
            if( shouldRun ) a();
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
