using CK.Core;
using System;

namespace CK.Testing.Monitoring
{
    /// <summary>
    /// Provides a monitor and console control.
    /// </summary>
    public interface IMonitorTestHelperCore : ITestHelperResolvedCallback
    {
        /// <summary>
        /// Gets the monitor.
        /// </summary>
        IActivityMonitor Monitor { get; }

        /// <summary>
        /// Gets or sets whether <see cref="Monitor"/> will log into the console.
        /// Initially configurable by "Monitor/LogToConsole" = "true", otherwise defaults to false.
        /// </summary>
        bool LogToConsole { get; set; }

        /// <summary>
        /// Gets whether all activities will be logged to <see cref="IBasicTestHelper.LogFolder"/>/CKMon folders.
        /// Configurable by "Monitor/LogToBinFile" = "true" (or "Monitor/LogToBinFiles"), otherwise defaults to false.
        /// </summary>
        bool LogToBinFile { get; }

        /// <summary>
        /// Gets whether all activities will be logged to <see cref="IBasicTestHelper.LogFolder"/>/Text folders.
        /// Configurable by "Monitor/LogToTextFile" = "true" (or "Monitor/LogToTextFiles"), otherwise defaults to false.
        /// </summary>
        bool LogToTextFile { get; }

        /// <summary>
        /// Ensures that the console monitor is on (i.e. <see cref="LogToConsole"/> is true) until the
        /// returned IDisposable is disposed.
        /// </summary>
        /// <returns>The disposable.</returns>
        IDisposable TemporaryEnsureConsoleMonitor();

        /// <summary>
        /// Runs code inside a standard "weak assembly resolver" and dumps the eventual conflicts.
        /// </summary>
        /// <param name="action">The action. Must not be null.</param>
        void WithWeakAssemblyResolver( Action action );

        /// <summary>
        /// Runs code inside a standard "weak assembly resolver" and dumps the eventual conflicts.
        /// This can be used to provide a async lambda (a Task will be returned that must be awaited).
        /// </summary>
        /// <param name="action">The action. Must not be null.</param>
        /// <returns>The result.</returns>
        T WithWeakAssemblyResolver<T>( Func<T> action );
    }
}
