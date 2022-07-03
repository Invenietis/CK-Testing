using CK.Core;
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
        /// Gets the name of the running test project (the last part of <see cref="TestProjectFolder"/>).
        /// </summary>
        string TestProjectName => TestProjectFolder.LastPart;

        /// <summary>
        /// Gets the name of the Solution (the last part of <see cref="SolutionFolder"/>).
        /// </summary>
        string SolutionName => SolutionFolder.LastPart;

        /// <summary>
        /// Gets the solution folder: where the .git folder is.
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
        /// Tries to locate the SUT (System Under Test) project based on the <see cref="TestProjectName"/> (if it ends with ".Tests"):
        /// it is the first directory that exists in a directory above with a name without the ".Tests" suffix.
        /// <para>
        /// If no such project is found, this fallbacks to <see cref="TestProjectFolder"/>.
        /// </para>
        /// </summary>
        NormalizedPath ClosestSUTProjectFolder { get; }

        /// <summary>
        /// Gets the path to the log folder. It is the 'Logs' folder in the <see cref="TestProjectFolder"/>. 
        /// </summary>
        NormalizedPath LogFolder { get; }

        /// <summary>
        /// Gets the bin folder where the tests are being executed.
        /// This normally is the same as <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        NormalizedPath BinFolder { get; }

        /// <summary>
        /// Gets the sub path from <see cref="TestProjectFolder"/> to <see cref="BinFolder"/>.
        /// This captures the "bin/<see cref="BuildConfiguration"/>}/(target framework folder)"/>.
        /// </summary>
        NormalizedPath PathToBin { get; }

        /// <summary>
        /// Clears a folder from all its existing content or ensures it exists
        /// and that a file can be written in it, or simple destroys it.
        /// This method raises the <see cref="OnCleanupFolder"/> event.
        /// </summary>
        /// <param name="folder">The path to the folder.</param>
        /// <param name="ensureFolderAvailable">
        /// By default, ensures that the folder exists and clears is content.
        /// When false, the folder and its content is removed.
        /// </param>
        /// <param name="maxRetryCount">Maximal number of retries on failure.</param>
        /// <returns>The <paramref name="folder"/>.</returns>
        NormalizedPath CleanupFolder( NormalizedPath folder, bool ensureFolderAvailable = true, int maxRetryCount = 5 );

        /// <summary>
        /// Raised whenever a folder has been cleaned up.
        /// </summary>
        event EventHandler<CleanupFolderEventArgs>? OnCleanupFolder;

        /// <summary>
        /// Executes an action once and only the first time it is called during the application lifetime.
        /// The action is identified by the calling site.
        /// </summary>
        /// <param name="a">Action to execute.</param>
        /// <param name="s">Path of the source file, automatically sets by the compiler.</param>
        /// <param name="l">Line number in the source file, automatically sets by the compiler.</param>
        void OnlyOnce( Action a, [CallerFilePath]string? s = null, [CallerLineNumber] int l = 0 );
    }
}
