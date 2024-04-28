using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CK.Testing
{
    /// <summary>
    /// Implements a dynamic <see cref="IConfigurationSource"/>: when <see cref="IConfigurationBuilder.Add(IConfigurationSource)"/> to a
    /// builder, any subsequent change of these items (through the indexer <see cref="this[string]"/>) or <see cref="Remove(string)"/>
    /// triggers a configuration reload.
    /// Use <see cref="StartBatch()"/> to apply multiple updates at once.
    /// </summary>
    public class DynamicConfigurationSource : ConfigurationProvider, IConfigurationSource, IEnumerable<KeyValuePair<string, string>>
    {
        readonly IDisposable _batch;
        int _batchCOunt;
        bool _changed;

        class D : IDisposable
        {
            readonly DynamicConfigurationSource _h;

            public D( DynamicConfigurationSource h ) => _h = h;

            public void Dispose()
            {
                if( --_h._batchCOunt == 0 && _h._changed ) _h.OnChanged();
            }
        }

        /// <summary>
        /// Initializes an empty configuration source.
        /// </summary>
        public DynamicConfigurationSource()
        {
            _batch = new D( this );
        }

        /// <summary>
        /// Gets or sets a configuration entry. A typical key is "Monitoring:GrandOutput:Handlers:Console:BackgroundColor".
        /// The value can be null, that is not the same as removing the key: a previously registered configuration provider may
        /// provide a value when the key is removed.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <returns>The value that can be null if the key doesn't exist or is associated to null.</returns>
        public string? this[string key]
        {
            get
            {
                Data.TryGetValue( key, out var v );
                return v;
            }
            set
            {
                Data.TryGetValue( key, out var v );
                if( v != value )
                {
                    Data[key] = value;
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Removes a key f it exists.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>True when removed, false if the key was not found.</returns>
        public bool Remove( string key )
        {
            if( Data.Remove( key ) )
            {
                OnChanged();
                return true;
            }
            return false;
        }

        void OnChanged()
        {
            if( _batchCOunt > 0 ) _changed = true;
            else
            {
                _changed = false;
                OnReload();
            }
        }

        /// <summary>
        /// Creates a <see cref="IDisposable"/> that will suspend any configuration changes until its disposal.
        /// Can be called multiple times (an internal reference counter is managed).
        /// </summary>
        /// <returns>The disposable.</returns>
        public IDisposable StartBatch()
        {
            ++_batchCOunt;
            return _batch;
        }

        /// <summary>
        /// Builds the associated builder: actually this is its own builder.
        /// </summary>
        /// <param name="builder">This (is its own builder).</param>
        /// <returns>This object.</returns>
        public IConfigurationProvider Build( IConfigurationBuilder builder ) => this;

        /// <summary>
        /// Returns all the keys and values configurations.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Data.GetEnumerator();
    }
}
