using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Adds helpers to <see cref="ITestHelperConfiguration"/>.
    /// </summary>
    public static class TestHelperConfigurationExtensions
    {
        /// <summary>
        /// Gets a boolean configuration value.
        /// </summary>
        /// <param name="this">This configuration.</param>
        /// <param name="key">The configuration entry key.</param>
        /// <returns>The value or null if not found.</returns>
        public static bool? GetBoolean( this TestHelperConfiguration @this, NormalizedPath key )
        {
            var s = @this.Get( key );
            return s == null ? (bool?)null : StringComparer.OrdinalIgnoreCase.Equals( s, "true" );
        }

        /// <summary>
        /// Gets an integer configuration value.
        /// </summary>
        /// <param name="this">This configuration.</param>
        /// <param name="key">The configuration entry key.</param>
        /// <returns>The value or null if not found.</returns>
        public static int? GetInt32( this TestHelperConfiguration @this, NormalizedPath key )
        {
            var s = @this.Get( key );
            return s == null ? (int?)null : Int32.Parse( s );
        }

        /// <summary>
        /// Gets the configuration value from semi colon (;) separated paths as a set of paths
        /// (see <see cref="TestHelperConfigurationValue.GetValueAsPath"/>).
        /// </summary>
        /// <param name="this">This configuration.</param>
        /// <param name="key">The configuration entry key.</param>
        /// <returns>The values as paths.</returns>
        public static IEnumerable<NormalizedPath> GetMultiPaths( this TestHelperConfiguration @this, NormalizedPath key )
        {
            var p = @this.GetConfigValue( key );
            if( !p.HasValue || p.Value.Value.Length == 0 ) return Array.Empty<NormalizedPath>();
            return p.Value.Value.Split( ';' )
                                .Select( x => new TestHelperConfigurationValue( @this, p.Value.BasePath, x.Trim() ) )
                                .Where( x => x.Value.Length > 0 )
                                .Select( x => x.GetValueAsPath() )
                                .Select( x => new NormalizedPath( x ) );
        }
    }
}

