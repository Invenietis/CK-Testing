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
        /// Gets whether the test helper services must be resolved as transient ones.
        /// Defaults to false: the services are by default singletons.
        /// To activate transient mode, the configuration "TestHelper/TransientMode" must be "true".
        /// </summary>
        bool TransientMode { get; }

        /// <summary>
        /// Gets the <see cref="IMixinTestHelper"/> that are pre-loaded from "TestHelper/PreLoadedAssemblies"
        /// configuration: this is an optional comma separated list of assembly names for which all IMixinTestHelper
        /// implementations available will be resolved, even if it's only a "more basic one" that is actually resolved.
        /// </summary>
        IReadOnlyList<Type> PreLoadedTypes { get; }

        /// <summary>
        /// Resolves an instance, either a singleton or a transient one depending on <see cref="TransientMode"/>.
        /// This method throw exceptions on failure and this is intended: test framework must be fully operational
        /// and any error are considered developper errors.
        /// </summary>
        /// <param name="t">The type to resolve.</param>
        /// <returns>The resolved instance.</returns>
        object Resolve( Type t );
    }

}
