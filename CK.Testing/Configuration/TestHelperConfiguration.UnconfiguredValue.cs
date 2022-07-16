using CK.Core;
using System;

namespace CK.Testing
{

    public sealed partial class TestHelperConfiguration
    {

        sealed class UnconfiguredValue : IValue
        {
            readonly Func<string>? _currentValue;
            string? _defaultValue;

            public bool IsEditable => _currentValue != null;

            public string? ConfiguredValue => null;

            public NormalizedPath Key { get; }

            public string? CurrentValue => _currentValue != null ? _currentValue() : _defaultValue;

            public string Description { get; }

            public NormalizedPath ObsoleteKeyUsed => default;

            public NormalizedPath FileBasePath => StaticBasicTestHelper._binFolder;

            // Will always throw with the "ConfiguredValue != null" message.
            public void SetNormalizedConfiguredValue( string normalizedValue ) => Throw.CheckState( ConfiguredValue != null );


            public UnconfiguredValue( NormalizedPath key, string description, Func<string>? currentValue )
            {
                Key = key;
                _currentValue = currentValue;
                Description = description;
            }

            public void SetDefaultValue( string? defaultValue )
            {
                Throw.CheckState( IsEditable is false );
                _defaultValue = defaultValue;
            }
        }
    }
}
