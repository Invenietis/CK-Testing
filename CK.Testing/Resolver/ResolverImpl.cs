using CK.Core;
using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace CK.Testing
{
    class ResolverImpl : ITestHelperResolver
    {
        readonly ITestHelperConfiguration _config;
        readonly SimpleServiceContainer _container;
        readonly IReadOnlyList<Type> _preLoadedTypes;

        public ResolverImpl( ITestHelperConfiguration config )
        {
            _container = new SimpleServiceContainer();
            _container.Add( config );
            _config = config;
            TransientMode = config.GetBoolean( "TestHelper/TransientMode" ) ?? false;
            string[] assemblies = config.Get( "TestHelper/PreLoadedAssemblies", String.Empty ).Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            if( assemblies.Length > 0 )
            {
                using( WeakAssemblyNameResolver.TemporaryInstall() )
                {
                    var types = new List<Type>();
                    foreach( var n in assemblies )
                    {
                        var a = Assembly.Load( n );
                        types.AddRange( a.GetExportedTypes().Where( t => t.IsInterface && typeof( IMixinTestHelper ).IsAssignableFrom( t ) ) );
                    }
                    _preLoadedTypes = types;
                    if( !TransientMode )
                    {
                        foreach( var preLoad in _preLoadedTypes ) Resolve( _container, preLoad, true );
                    }
                }
            }
            else _preLoadedTypes = Type.EmptyTypes;
        }

        public bool TransientMode { get; }

        public IReadOnlyList<Type> PreLoadedTypes => _preLoadedTypes;

        public object Resolve( Type t )
        {
            using( WeakAssemblyNameResolver.TemporaryInstall() )
            {
                SimpleServiceContainer container;
                if( TransientMode )
                {
                    container = new SimpleServiceContainer( _container );
                    foreach( var preLoad in _preLoadedTypes ) Resolve( container, preLoad, true );
                }
                else container = _container;
                return Resolve( container, t, true );
            }
        }

        object Resolve( ISimpleServiceContainer container, Type t, bool throwOnError )
        {
            object result = container.GetService( t );
            if( result == null && t != typeof(ITestHelper) && t != typeof(IMixinTestHelper) )
            {
                if( !t.IsClass || t.IsAbstract )
                {
                    Type tMapped = MapType( t, throwOnError );
                    result = Create( container, tMapped, throwOnError );
                    if( result != null && !tMapped.Assembly.IsDynamic ) container.Add( tMapped, result );
                }
                else result = Create( container, t, throwOnError );
                if( result != null ) container.Add( t, result );
            }
            return result;
        }

        Type MapType( Type t, bool throwOnError )
        {
            Debug.Assert( t != typeof( ITestHelper ) && t != typeof( IMixinTestHelper ) );
            string typeName = _config.Get( "TestHelper/" + t.FullName );
            if( typeName != null )
            {
                // Always throw when config is used.
                Type fromConfig = SimpleTypeFinder.WeakResolver( typeName, true );
                if( typeof(IMixinTestHelper).IsAssignableFrom(fromConfig))
                {
                    throw new Exception( $"Mapped type '{fromConfig.FullName}' is a Mixin. It can not be explicitely implemented." );
                }
                return fromConfig;
            }
            if( t.IsInterface && t.Name[0] == 'I' )
            {
                var cName = t.Name.Substring( 1 );
                string fullName = $"{t.Namespace}.{cName}, {t.Assembly.FullName}";
                Type found = SimpleTypeFinder.WeakResolver( fullName, false );
                if( found == null && cName.EndsWith( "Core" ) )
                {
                    var nameNoCore = cName.Remove( cName.Length - 4 );
                    var ns = t.Namespace.Split( '.' ).ToList();
                    while( ns.Count > 0 )
                    {
                        fullName = $"{String.Join(".", ns)}.{nameNoCore}, {t.Assembly.FullName}";
                        found = SimpleTypeFinder.WeakResolver( fullName, false );
                        if( found != null ) break;
                        ns.RemoveAt( ns.Count - 1 );
                    }
                }
                if( found != null && t.IsAssignableFrom( found ) )
                {
                    return found;
                }
                if( typeof( IMixinTestHelper ).IsAssignableFrom( t ) )
                {
                    if( t.GetMembers().Length > 0 )
                    {
                        throw new Exception( $"Interface '{t.FullName}' is a Mixin. It can not have members of its own." );
                    }
                    return MixinType.Create( t );
                }
            }
            if( !throwOnError ) return null;
            throw new Exception( $"Unable to locate an implementation for {t.AssemblyQualifiedName}." );
        }

        object Create( ISimpleServiceContainer container, Type t, bool throwOnError )
        {
            Debug.Assert( t != null && t.IsClass && !t.IsAbstract );
            var longestCtor = t.GetConstructors( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic )
                                .Select( x => Tuple.Create( x, x.GetParameters() ) )
                                .OrderByDescending( x => x.Item2.Length )
                                .Select( x => new
                                {
                                    Ctor = x.Item1,
                                    Parameters = x.Item2,
                                    Values = new object[x.Item2.Length]
                                } )
                                .FirstOrDefault();
            if( longestCtor == null )
            {
                if( throwOnError ) throw new Exception( $"Unable to find a public constructor for '{t.FullName}'." );
                return null;
            }
            for( int i = 0; i < longestCtor.Parameters.Length; ++i )
            {
                var p = longestCtor.Parameters[i];
                longestCtor.Values[i] = Resolve( container, p.ParameterType, !p.HasDefaultValue ) ?? p.DefaultValue;
            }
            return longestCtor.Ctor.Invoke( longestCtor.Values );
        }

        public static ITestHelperResolver Create() => new ResolverImpl( TestHelperConfiguration.Default );
    }
}
