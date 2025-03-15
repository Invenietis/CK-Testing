using CK.Core;
using Shouldly;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// This class is exceptionnaly defined in the global namespace. As such, Roslyn selects its methonds
/// other any definition in explicit namespaces.
/// <para>
/// This is bad and must remain exceptional. But, here, it enables to use Shouldly and "override" some
/// of its definitions.
/// </para>
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
[DebuggerStepThrough]
[ShouldlyMethods]
[EditorBrowsable( EditorBrowsableState.Never )]
public static class CKShouldlyGlobalOverrideExtensions
{
    /// <summary>
    /// Fix https://github.com/shouldly/shouldly/issues/934.
    /// <para>
    /// Note that this change the semantics and this is intended: Shouldy's
    /// non generic ShouldThrow doesn't honor the Lyskov Substitution Principle (as opposed to the generic one),
    /// the <paramref name="exceptionType"/> must be the exact type of the exception (like our
    /// <see cref="CKShouldlyExtensions.ShouldThrowExactly(Delegate, Type, string?)"/> does).
    /// </para>
    /// </summary>
    /// <remarks>
    /// This Delegate based method is chosen because it is in the global::CKShouldlyGlobalOverrideExtensions.
    /// </remarks>
    /// <param name="actual">The action code that should throw.</param>
    /// <param name="exceptionType">The expected exception type.</param>
    /// <param name="customMessage">Optional message.</param>
    /// <returns>The exception instance.</returns>
    public static Exception ShouldThrow( this Delegate actual, Type exceptionType, string? customMessage = null )
    {
        return CKShouldlyExtensions.ThrowInternal( actual, customMessage, exceptionType, exactType: false );
    }

    #region Override of IEnumerable to return the enumerable (when it makes sense).

    public static IEnumerable<T> ShouldContain<T>( this IEnumerable<T> actual, T expected, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, expected, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldContain<T>( this IEnumerable<T> actual, T expected, IEqualityComparer<T> comparer, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, expected, comparer, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldNotContain<T>( this IEnumerable<T> actual, T expected, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldNotContain( actual, expected, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldNotContain<T>( this IEnumerable<T> actual, T expected, IEqualityComparer<T> comparer, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldNotContain( actual, expected, comparer, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldContain<T>( this IEnumerable<T> actual, Expression<Func<T, bool>> elementPredicate, int expectedCount, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, elementPredicate, expectedCount, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldContain<T>( this IEnumerable<T> actual, Expression<Func<T, bool>> elementPredicate, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, elementPredicate, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldNotContain<T>( this IEnumerable<T> actual, Expression<Func<T, bool>> elementPredicate, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldNotContain( actual, elementPredicate, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldAllBe<T>( this IEnumerable<T> actual, Expression<Func<T, bool>> elementPredicate, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldAllBe( actual, elementPredicate, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldNotBeEmpty<T>( [NotNull] this IEnumerable<T>? actual, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldNotBeEmpty( actual, customMessage );
        return actual;
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    public static IEnumerable<float> ShouldContain( this IEnumerable<float> actual, float expected, double tolerance, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, expected, tolerance, customMessage );
        return actual;
    }

    public static IEnumerable<double> ShouldContain( this IEnumerable<double> actual, double expected, double tolerance, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldContain( actual, expected, tolerance, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeSubsetOf<T>( this IEnumerable<T> actual, IEnumerable<T> expected, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeSubsetOf( actual, expected, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeSubsetOf<T>( this IEnumerable<T> actual, IEnumerable<T> expected, IEqualityComparer<T> comparer, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeSubsetOf( actual, expected, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeUnique<T>( this IEnumerable<T> actual, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeUnique( actual, customMessage );
        return actual;
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    public static IEnumerable<T> ShouldBeUnique<T>( this IEnumerable<T> actual, IEqualityComparer<T> comparer )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeUnique( actual, comparer );
        return actual;
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    public static IEnumerable<T> ShouldBeUnique<T>( this IEnumerable<T> actual, IEqualityComparer<T> comparer, string? customMessage )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeUnique( actual, comparer, customMessage );
        return actual;
    }

    public static IEnumerable<string> ShouldBe( this IEnumerable<string> actual, IEnumerable<string> expected, Case caseSensitivity, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBe( actual, expected, caseSensitivity, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeInOrder<T>( this IEnumerable<T> actual, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeInOrder( actual, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeInOrder<T>( this IEnumerable<T> actual, SortDirection expectedSortDirection, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeInOrder( actual, expectedSortDirection, customMessage );
        return actual;
    }

    public static IEnumerable<T> ShouldBeInOrder<T>( this IEnumerable<T> actual, SortDirection expectedSortDirection, IComparer<T>? customComparer, string? customMessage = null )
    {
        Shouldly.ShouldBeEnumerableTestExtensions.ShouldBeInOrder( actual, expectedSortDirection, customComparer, customMessage );
        return actual;
    }
    #endregion

    #region Override of ShouldBe to return the subject.

    public static T? ShouldBe<T>( [NotNullIfNotNull( nameof( expected ) )] this T? actual,
                                    [NotNullIfNotNull( nameof( actual ) )] T? expected,
                                    string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, customMessage );
        return actual;
    }

    public static T? ShouldBe<T>( [NotNullIfNotNull( nameof( expected ) )] this T? actual,
                                  [NotNullIfNotNull( nameof( actual ) )] T? expected,
                                  IEqualityComparer<T> comparer,
                                  string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, comparer, customMessage );
        return actual;
    }

    public static T? ShouldNotBe<T>( this T? actual, T? expected, string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldNotBe( actual, expected, customMessage );
        return actual;
    }

    public static T? ShouldNotBe<T>( this T? actual, T? expected, IEqualityComparer<T> comparer, string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldNotBe( actual, expected, comparer, customMessage );
        return actual;
    }

    public static IEnumerable<T>? ShouldBe<T>( [NotNullIfNotNull( nameof( expected ) )] this IEnumerable<T>? actual,
                                                [NotNullIfNotNull( nameof( actual ) )] IEnumerable<T>? expected,
                                                bool ignoreOrder = false )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, ignoreOrder );
        return actual;
    }

    public static IEnumerable<T>? ShouldBe<T>(
        [NotNullIfNotNull( nameof( expected ) )] this IEnumerable<T>? actual,
        [NotNullIfNotNull( nameof( actual ) )] IEnumerable<T>? expected,
        bool ignoreOrder,
        string? customMessage )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, ignoreOrder, customMessage );
        return actual;
    }

    public static IEnumerable<T>? ShouldBe<T>(
        [NotNullIfNotNull( nameof( expected ) )] this IEnumerable<T>? actual,
        [NotNullIfNotNull( nameof( actual ) )] IEnumerable<T>? expected,
        IEqualityComparer<T> comparer,
        bool ignoreOrder = false,
        string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, comparer, ignoreOrder, customMessage );
        return actual;
    }

    public static IEnumerable<decimal>? ShouldBe( this IEnumerable<decimal> actual, IEnumerable<decimal> expected, decimal tolerance, string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBe( actual, expected, tolerance, customMessage );
        return actual;
    }

    public static object? ShouldBeSameAs(
        [NotNullIfNotNull( nameof( expected ) )] this object? actual,
        [NotNullIfNotNull( nameof( actual ) )] object? expected,
        string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldBeSameAs( actual, expected, customMessage );
        return actual;
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    public static object? ShouldNotBeSameAs( this object? actual, object? expected, string? customMessage = null )
    {
        Shouldly.ShouldBeTestExtensions.ShouldNotBeSameAs( actual, expected, customMessage );
        return actual;
    }

    #endregion
}

#pragma warning restore CA1050 // Declare types in namespaces

namespace Shouldly
{

    /// <summary>
    /// Extends Shouldly with useful helpers.
    /// </summary>
    [DebuggerStepThrough]
    [ShouldlyMethods]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static class CKShouldlyExtensions
    {
        /// <summary>
        /// Fix https://github.com/shouldly/shouldly/issues/934.
        /// </summary>
        /// <remarks>
        /// This Delegate based method is chosen over the original Should's extension method
        /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
        /// </remarks>
        /// <typeparam name="TException">The expected exception type.</typeparam>
        /// <param name="actual">The action code that should throw.</param>
        /// <param name="customMessage">Optional message.</param>
        /// <returns>The exception instance.</returns>
        public static TException ShouldThrow<TException>( this Delegate actual, string? customMessage = null )
            where TException : Exception
        {
            return (TException)ThrowInternal( actual, customMessage, typeof( TException ), exactType: false );
        }

        /// <summary>
        /// Fix https://github.com/shouldly/shouldly/issues/934.
        /// <para>
        /// Note that this change the semantics and this is intended: Shouldy's
        /// non generic ShouldThrow doesn't honor the Lyskov Substitution Principle (as opposed to the generic one),
        /// the <paramref name="exceptionType"/> must be the exact type of the exception (like our
        /// <see cref="ShouldThrowExactly(Delegate, Type, string?)"/> does).
        /// </para>
        /// </summary>
        /// <remarks>
        /// This Delegate based method is chosen over the original Should's extension method
        /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
        /// </remarks>
        /// <param name="actual">The action code that should throw.</param>
        /// <param name="exceptionType">The expected exception type.</param>
        /// <param name="customMessage">Optional message.</param>
        /// <returns>The exception instance.</returns>
        public static Exception ShouldThrow( this Delegate actual, Type exceptionType, string? customMessage = null )
        {
            return ThrowInternal( actual, customMessage, exceptionType, exactType: false );
        }

        /// <summary>
        /// Fix https://github.com/shouldly/shouldly/issues/934.
        /// </summary>
        /// <remarks>
        /// This Delegate based method is chosen over the original Should's extension method
        /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
        /// </remarks>
        /// <param name="actual">The action code that should throw.</param>
        /// <param name="exceptionType">The exact expected exception type.</param>
        /// <param name="customMessage">Optional message.</param>
        /// <returns>The exception instance.</returns>
        public static TException ShouldThrowExactly<TException>( this Delegate actual, string? customMessage = null )
            where TException : Exception
        {
            return (TException)ThrowInternal( actual, customMessage, typeof( TException ), exactType: true );
        }

        /// <summary>
        /// Fix https://github.com/shouldly/shouldly/issues/934.
        /// </summary>
        /// <remarks>
        /// This Delegate based method is chosen over the original Should's extension method
        /// that takes a <c>Func&lt;object?&gt</c> parameter (that is the cause of the issue).  
        /// </remarks>
        /// <param name="actual">The action code that should throw.</param>
        /// <param name="exceptionType">The exact expected exception type.</param>
        /// <param name="customMessage">Optional message.</param>
        /// <returns>The exception instance.</returns>
        public static Exception ShouldThrowExactly( this Delegate actual, Type exceptionType, string? customMessage = null )
        {
            return ThrowInternal( actual, customMessage, exceptionType, exactType: true );
        }

        internal static Exception ThrowInternal( Delegate actual,
                                                 string? customMessage,
                                                 Type expectedExceptionType,
                                                 bool exactType,
                                                 [CallerMemberName] string? shouldlyMethod = null )
        {
            // Handle composite delegates: consider the invocation list if any.
            var multi = actual.GetInvocationList();
            if( multi.Length == 0 )
            {
                foreach( var d in multi )
                {
                    CheckNoParameters( d );
                }
            }
            else
            {
                CheckNoParameters( actual );
            }

            static void CheckNoParameters( Delegate d )
            {
                var parameters = d.Method.GetParameters();
                if( parameters.Length > 0 )
                {
                    var parametersDesc = parameters.Select( p => $"{p.ParameterType.Name} {p.Name}" );
                    throw new ArgumentException( $"""
                        ShouldThrow can only be called on a delegate without parameters.
                        Found method {d.Method.DeclaringType?.Name}.{d.Method.Name}( {string.Join( ", ", parametersDesc )} )
                        """ );
                }
            }
            try
            {
                if( multi.Length == 0 )
                {
                    Execute( actual );
                }
                else
                {
                    foreach( var d in multi )
                    {
                        Execute( d );
                    }
                }

                static void Execute( Delegate d ) => d.Method.Invoke( d.Target, BindingFlags.DoNotWrapExceptions, null, null, null );
            }
            catch( Exception ex )
            {
                if( ex.GetType() == expectedExceptionType
                    || (!exactType && expectedExceptionType.IsAssignableFrom( ex.GetType() )) )
                {
                    return ex;
                }
                throw new ShouldAssertException( new ShouldlyThrowMessage( expectedExceptionType, ex.GetType(), customMessage, shouldlyMethod! ).ToString(), ex );
            }
            throw new ShouldAssertException( new ShouldlyThrowMessage( expectedExceptionType, customMessage: customMessage, shouldlyMethod! ).ToString() );
        }

        /// <summary>
        /// Predicate match.
        /// This is CK specific.
        /// </summary>
        /// <typeparam name="T">This type.</typeparam>
        /// <param name="actual">This instance.</param>
        /// <param name="predicate">The predicate that muts be satisfied.</param>
        /// <param name="customMessage">Optional message.</param>
        /// <returns>This instance.</returns>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static T ShouldMatch<T>( this T actual, Expression<Func<T, bool>> predicate, string? customMessage = null )
        {
            Throw.CheckNotNullArgument( predicate );
            var condition = predicate.Compile();
            if( !condition( actual ) )
                throw new ShouldAssertException( new ExpectedActualShouldlyMessage( predicate.Body, actual, customMessage ).ToString() );
            return actual;
        }

        /// <summary>
        /// Apply an action to each item that should be one or more Shouldly expectation.
        /// This is CK specific.
        /// </summary>
        /// <typeparam name="T">Th type of the enumerable.</typeparam>
        /// <param name="actual">This enumerable.</param>
        /// <param name="action">The action to apply.</param>
        /// <returns>This enumerable.</returns>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static IEnumerable<T> ShouldAll<T>( this IEnumerable<T> actual, Action<T> action )
        {
            Throw.CheckNotNullArgument( action );
            int idx = 0;
            try
            {
                foreach( var e in actual )
                {
                    action( e );
                    ++idx;
                }
            }
            catch( ShouldAssertException aEx )
            {
                var prefix = Environment.NewLine + "  | ";
                var offsetMessage = string.Join( prefix, aEx.Message.Split( Environment.NewLine ) );
                throw new ShouldAssertException( $"ShouldAll failed for item n°{idx}.{prefix}{offsetMessage}" );
            }
            return actual;
        }

        /// <summary>
        /// Explicit override to allow implict cast from string.
        /// </summary>
        /// <param name="actual">This normalized path.</param>
        /// <param name="expected">The expected path.</param>
        /// <param name="customMessage">Optional message.</param>
        public static NormalizedPath ShouldBe( this NormalizedPath actual, NormalizedPath expected, string? customMessage = null )
        {
            actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
            return actual;
        }

        public static short ShouldBe( this short actual, short expected, string? customMessage = null )
        {
            actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
            return actual;
        }

        public static ushort ShouldBe( this ushort actual, ushort expected, string? customMessage = null )
        {
            actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
            return actual;
        }

        public static sbyte ShouldBe( this sbyte actual, sbyte expected, string? customMessage = null )
        {
            actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
            return actual;
        }

        public static byte ShouldBe( this byte actual, byte expected, string? customMessage = null )
        {
            actual.AssertAwesomely( actual => actual == expected, actual, expected, customMessage );
            return actual;
        }

    }
}
