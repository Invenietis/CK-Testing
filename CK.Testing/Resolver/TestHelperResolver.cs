using CK.Core;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CK.Testing
{
    /// <summary>
    /// Entry point: this exposes the <see cref="Default"/> resolver.
    /// </summary>
    public static class TestHelperResolver
    {
        static object _lock = new object();
        static ITestHelperResolver _resolver;

        /// <summary>
        /// Gets the <see cref="ITestHelperResolver"/> to use.
        /// </summary>
        public static ITestHelperResolver Default
        {
            get
            {
                if( _resolver == null )
                {
                    lock( _lock )
                    {
                        using( WeakAssemblyNameResolver.TemporaryInstall() )
                        {
                            _resolver = ResolverImpl.Create();
                        }
                    }
                }
                return _resolver;
            }
        }
    }
}
