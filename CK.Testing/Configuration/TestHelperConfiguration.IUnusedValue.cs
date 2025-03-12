using CK.Core;

namespace CK.Testing;


public sealed partial class TestHelperConfiguration
{
    /// <summary>
    /// Describes a configuration value that exists but is not used by any TestHelper.
    /// </summary>
    public interface IUnusedValue
    {
        /// <summary>
        /// Gets the configuration key.
        /// </summary>
        NormalizedPath UnusedKey { get; }

        /// <summary>
        /// Gets the configured value.
        /// </summary>
        string ConfiguredValue { get; }
    }
}
