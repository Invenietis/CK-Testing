using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CK.Core;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="IBasicTestHelper"/>.
    /// </summary>
    public class BasicTestHelper : StaticBasicTestHelper, IBasicTestHelper
    {
        static BasicTestHelper()
        {
            if( _initializationError != null ) _initializationError.Throw();
        }

        event EventHandler<CleanupFolderEventArgs>? _onCleanupFolder;

        string IBasicTestHelper.BuildConfiguration => _buildConfiguration;

        string IBasicTestHelper.TestProjectName => _testProjectName;

        bool IBasicTestHelper.IsTestHost => _isTestHost;

        NormalizedPath IBasicTestHelper.RepositoryFolder => _repositoryFolder;

        NormalizedPath IBasicTestHelper.SolutionFolder => _solutionFolder;

        NormalizedPath IBasicTestHelper.LogFolder => _logFolder;

        NormalizedPath IBasicTestHelper.TestProjectFolder => _testProjectFolder;

        NormalizedPath IBasicTestHelper.BinFolder => _binFolder;

        bool IBasicTestHelper.IsExplicitAllowed => !_isTestHost || ExplicitTestManager.IsExplicitAllowed;

        NormalizedPath IBasicTestHelper.CleanupFolder( NormalizedPath folder, bool ensureFolderAvailable, int maxRetryCount )
        {
            if( folder.IsEmptyPath ) throw new ArgumentOutOfRangeException( nameof( folder ) );
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
        /// Gets the <see cref="IBasicTestHelper"/> default implementation.
        /// </summary>
        public static IBasicTestHelper TestHelper => TestHelperResolver.Default.Resolve<IBasicTestHelper>();
    }
}
