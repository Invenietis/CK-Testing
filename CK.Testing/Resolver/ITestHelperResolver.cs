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
        /// Gets the <see cref="IMixinTestHelper"/> that are pre-loaded from "TestHelper/PreLoadedAssemblies"
        /// configuration: this is an optional semicolon ';' separated list of assembly names for which all IMixinTestHelper
        /// implementations available will be resolved, even if it's only a "more basic one" that is actually resolved.
        /// </summary>
        IReadOnlyList<Type> PreLoadedTypes { get; }

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
