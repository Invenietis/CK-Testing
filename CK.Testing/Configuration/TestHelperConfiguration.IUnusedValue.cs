using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
