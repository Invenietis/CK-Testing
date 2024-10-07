using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CK.Testing;

/// <summary>
/// Entry point: this exposes the <see cref="Default"/> resolver.
/// </summary>
public static class TestHelperResolver
{
    static readonly object _lock = new object();
    static ITestHelperResolver? _resolver;

    /// <summary>
    /// Gets the default <see cref="ITestHelperResolver"/> to use.
    /// This resolver is bound to <see cref="TestHelperConfiguration.Default"/>.
    /// </summary>
    public static ITestHelperResolver Default
    {
        get
        {
            if( _resolver == null )
            {
                lock( _lock )
                {
                    if( _resolver == null )
                    {
                        _resolver = ResolverImpl.Create();
                    }
                }
            }
            return _resolver;
        }
    }

    /// <summary>
    /// Creates a new <see cref="ITestHelperResolver"/> bound to a specific configuration.
    /// </summary>
    /// <param name="config">An optional configuration: when null the <see cref="TestHelperConfiguration.Default"/> is used.</param>
    /// <returns>A new resolver.</returns>
    public static ITestHelperResolver Create( TestHelperConfiguration? config = null ) => ResolverImpl.Create( config );
}
