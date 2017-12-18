using CK.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    public static class TestHelperResolverExtensions
    {
        /// <summary>
        /// Resolves an instance, either a singleton or a transient one depending on <see cref="ITestHelperResolver.TransientMode"/>.
        /// This method throw exceptions on failure and this is intended: test framework must be fully operational
        /// and any error are considered developper errors.
        /// </summary>
        /// <param name="t">The type to resolve.</param>
        /// <returns>The resolved instance.</returns>
        public static T Resolve<T>( this ITestHelperResolver @this ) => (T)@this.Resolve( typeof( T ) );
    }
}
