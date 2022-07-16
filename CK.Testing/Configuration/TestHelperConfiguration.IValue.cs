using CK.Core;
using System;

namespace CK.Testing
{

    public sealed partial class TestHelperConfiguration
    {
        /// <summary>
        /// Describes a configuration value that may not be
        /// configured (<see cref="ConfiguredValue"/> is null).
        /// </summary>
        public interface IValue
        {
            /// <summary>
            /// Gets whether the configuration value acts as a default value (true) that can be changed later
            /// (in such case, <see cref="CurrentValue"/> may differ from <see cref="ConfiguredValue"/>) or
            /// can only be set by the configuration (false).
            /// </summary>
            bool IsEditable { get; }

            /// <summary>
            /// Gets the configured value.
            /// Null if this value has no explicit configuration.
            /// </summary>
            string? ConfiguredValue { get; }


            /// <summary>
            /// Gets the key of this configuration value.
            /// </summary>
            NormalizedPath Key { get; }

            /// <summary>
            /// Gets the current value of this configuration value.
            /// When <see cref="IsEditable"/> is false, this is the default value that must have been
            /// set by the TestHelper by calling <see cref="SetDefaultValue(string?)"/>.
            /// </summary>
            string? CurrentValue { get; }

            /// <summary>
            /// Gets the description of this configuration value.
            /// </summary>
            string Description { get; }

            /// <summary>
            /// Gets the obsolete key that has been found in configuration.
            /// If this is non empty, the configuration MUST be changed to use <see cref="Key"/>.
            /// </summary>
            NormalizedPath ObsoleteKeyUsed { get; }

            /// <summary>
            /// Sets a default value if this configuration value is not editable.
            /// If <see cref="IsEditable"/> is true, this throws an <see cref="InvalidOperationException"/>.
            /// </summary>
            /// <param name="defaultValue">The default value that the helper will use.</param>
            void SetDefaultValue( string? defaultValue );

            /// <summary>
            /// Enables the <see cref="ConfiguredValue"/> to be set to a normalized value.
            /// <para>
            /// <see cref="TestHelperConfiguration.DeclareMultiStrings(NormalizedPath, string, Func{string}?, char, string[])"/> for instance
            /// uses this to pack the trimmed strings so that the list of strings fits on one line.
            /// </para>
            /// <para>
            /// If this value is not configured (<see cref="ConfiguredValue"/> is null), this throws an <see cref="InvalidOperationException"/>.
            /// </para>
            /// </summary>
            /// <param name="normalizedValue">The configure value.</param>
            void SetNormalizedConfiguredValue( string normalizedValue );

            /// <summary>
            /// Gets the base path of this configuration value.
            /// Defaults to <see cref="IBasicTestHelper.TestProjectFolder"/> when this value is not configured or set by
            /// environment variables (the environment variables that start with <c>"TestHelper__"</c> prefix).
            /// </summary>
            NormalizedPath FileBasePath { get; }
        }
    }
}
