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
    public static class TestHelperResolver
    {
        static object _lock = new object();
        static ITestHelperResolver _resolver;

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
