using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{
    public sealed partial class TestHelperConfiguration
    {
        /// <summary>
        /// Configured value (read from the configuration).
        /// May be unused.
        /// </summary>
        sealed class Value : IUnusedValue, IValue
        {
            Func<string>? _currentValue;
            string? _description;
            string _configuredValue;
            string? _defaultValue;

            public NormalizedPath FileBasePath { get; }

            internal bool IsUsed => _description != null;

            public string ConfiguredValue => _configuredValue;

            public void SetNormalizedConfiguredValue( string normalizedValue ) => _configuredValue = normalizedValue;

            string IValue.Description => _description!;

            public NormalizedPath Key { get; private set; }

            NormalizedPath IUnusedValue.UnusedKey => Key;

            public NormalizedPath ObsoleteKeyUsed { get; private set; }

            public string? CurrentValue => _currentValue?.Invoke() ?? _defaultValue;

            public bool IsEditable => _currentValue != null;

            public void SetDefaultValue( string? defaultValue ) => _defaultValue = defaultValue;

            internal Value( NormalizedPath basePath, NormalizedPath key, string value )
            {
                Key = key;
                FileBasePath = basePath;
                _configuredValue = value;
            }

            internal void SetUsageInfoOnlyOnce( NormalizedPath key, string description, Func<string>? currentValue, NormalizedPath lookupKey )
            {
                if( _description != null ) ThrowAlreadyReadConfiguration( key );
                Key = key;
                _description = description;
                _currentValue = currentValue;
                if( lookupKey != key ) ObsoleteKeyUsed = lookupKey;
            }

            internal static void ThrowAlreadyReadConfiguration( NormalizedPath key )
            {
                Throw.InvalidOperationException( $"The configuration '{key}' has already been initialized. A configuration value must be initialized once and only once." );
            }

        }
    }
}
