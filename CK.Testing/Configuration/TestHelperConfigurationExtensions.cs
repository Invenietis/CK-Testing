using CK.Text;
using System;
using System.Collections.Generic;
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
        public static bool? GetBoolean( this ITestHelperConfiguration @this, NormalizedPath key )
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
        public static int? GetInt32( this ITestHelperConfiguration @this, NormalizedPath key )
        {
            var s = @this.Get( key );
            return s == null ? (int?)null : Int32.Parse( s );
        }
    }
}
