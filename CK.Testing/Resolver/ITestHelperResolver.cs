using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Core resolver for test helper objects.
    /// Any <see cref="IMixinTestHelper"/> are dynamically concretized.
    /// </summary>
    public interface ITestHelperResolver
    {
        /// <summary>
        /// Resolves an instance of .
        /// This method throw exceptions on failure and this is intended: test framework must be fully operational
        /// and any error are considered developer errors.
        /// </summary>
        /// <param name="t">The type to resolve.</param>
        /// <returns>The resolved instance.</returns>
        object Resolve( Type t );
    }

}
