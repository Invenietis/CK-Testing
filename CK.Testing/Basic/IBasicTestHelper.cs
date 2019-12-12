using CK.Core;
using CK.Text;
using System;
using System.Runtime.CompilerServices;

namespace CK.Testing
{
    /// <summary>
    /// Provides basic tests information.
    /// </summary>
    public interface IBasicTestHelper 
    {
        /// <summary>
        /// Gets the build configuration ("Debug" or "Release").
        /// </summary>
        string BuildConfiguration { get;}

        /// <summary>
        /// Gets whether the VS tests adapter is actually running this test.
        /// </summary>
        bool IsTestHost { get; }

        /// <summary>
        /// Workaround the VS tests limitation: this should be "assumed" at the start of every "Explicit" test method.
        /// This is always true if <see cref="IsTestHost"/> is false.
        /// </summary>
        bool IsExplicitAllowed { get; }

        /// <summary>
        /// Gets the name of the running test project that must be the name of the <see cref="System.Reflection.Assembly.GetEntryAssembly()"/>
        /// (except if this is the assembly "testhost" that is running) otherwise an exception is thrown.
        /// </summary>
        string TestProjectName { get; }

        /// <summary>
        /// Gets the path to the root folder: where the .git folder is.
        /// </summary>
        NormalizedPath RepositoryFolder { get; }

        /// <summary>
        /// Gets the solution folder. It is the parent directory of the 'Tests/' folder (that must exist).
        /// </summary>
        NormalizedPath SolutionFolder { get; }

        /// <summary>
        /// Gets the path to the test project folder (where the .csproj is).
        /// This is usually where folders specific to the test should be created and managed (like a
        /// "TestScripts" folder).
        /// The <see cref="LogFolder"/> is located inside this one.
        /// </summary>
        NormalizedPath TestProjectFolder { get; }

        /// <summary>
        /// Gets the path to the log folder. It is the 'Logs' folder in the <see cref="TestProjectFolder"/>. 
        /// </summary>
        NormalizedPath LogFolder { get; }

        /// <summary>
        /// Gets the bin folder where the tests are beeing executed.
        /// This normally is the same as <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        NormalizedPath BinFolder { get; }

        /// <summary>
        /// Clears a folder from all its existing content or ensures it exists
        /// and that a file can be written in it, or simple destroys it.
        /// </summary>
        /// <param name="folder">The path to the folder.</param>
        /// <param name="ensureFolderAvailable">
        /// By default, ensures that the the folder exists and clears is content.
        /// When false, the folder and its content is removed.
        /// </param>
        /// <param name="maxRetryCount">Maximal number of retries on failure.</param>
        void CleanupFolder( string folder, bool ensureFolderAvailable = true, int maxRetryCount = 5 );

        /// <summary>
        /// Raised whenever a folder has been cleaned up.
        /// </summary>
        event EventHandler<CleanupFolderEventArgs> OnCleanupFolder;

        /// <summary>
        /// Executes an action once and only the first time it is called during the application lifetime.
        /// The action is identified by the calling site.
        /// </summary>
        /// <param name="a">Action to execute.</param>
        /// <param name="s">Path of the source file, automatically sets by the compiler.</param>
        /// <param name="l">Line number in the source file, automatically sets by the compiler.</param>
        void OnlyOnce( Action a, [CallerFilePath]string s = null, [CallerLineNumber] int l = 0 );
    }
}
