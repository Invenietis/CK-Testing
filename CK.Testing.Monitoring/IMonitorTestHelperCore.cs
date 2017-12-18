using CK.Core;
using System;

namespace CK.Testing
{
    /// <summary>
    /// Provides a monitor and console control.
    /// </summary>
    public interface IMonitorTestHelperCore
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
        /// Configurable by "Monitor/GlobalCKMonFiles" = "true", otherwise defaults to false.
        /// </summary>
        bool GlobalCKMonFiles { get; }

        /// <summary>
        /// Gets whether all activities will be logged to <see cref="IBasicTestHelper.LogFolder"/>/Text folders.
        /// Configurable by "Monitor/GlobalTextFiles" = "true", otherwise defaults to false.
        /// </summary>
        bool GlobalTextFiles { get; }
    }
}
