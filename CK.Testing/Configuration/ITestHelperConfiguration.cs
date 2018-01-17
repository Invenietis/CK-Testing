using CK.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Simple access to all configurations.
    /// </summary>
    public interface ITestHelperConfiguration
    {
        /// <summary>
        /// Gets the configuration string value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <param name="defaultValue">The default value when not found.</param>
        /// <returns>The configured value or the default value.</returns>
        string Get( NormalizedPath key, string defaultValue = null );

        /// <summary>
        /// Gets the configuration value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <returns>The configured value.</returns>
        TestHelperConfigurationValue? GetConfigValue( NormalizedPath key );

        /// <summary>
        /// Gets all the configuration values defined.
        /// </summary>
        IEnumerable<TestHelperConfigurationValue> ConfigurationValues { get; }

    }
}
