namespace CK.Testing;

/// <summary>
/// Adds methods to <see cref="ITestHelperResolver"/>.
/// </summary>
public static class TestHelperResolverExtensions
{
    /// <summary>
    /// Resolves an instance.
    /// This method throw exceptions on failure and this is intended: test framework must be fully operational
    /// and any error are considered developer errors.
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="this">This resolver.</param>
    /// <returns>The resolved instance.</returns>
    public static T Resolve<T>( this ITestHelperResolver @this ) => (T)@this.Resolve( typeof( T ) );
}
