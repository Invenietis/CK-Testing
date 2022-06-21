using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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

        class Context
        {
            readonly ISimpleServiceContainer _container;
            List<ITestHelperResolvedCallback>? _created;
            HashSet<Type>? _fromTypes;
            int _depth;
            Type? _initialRequestedType;
            object? _initialRequestedTypeResult;

            public Context( ISimpleServiceContainer c )
            {
                _container = c;
            }

            public int CallDepth => _depth;

            public void Start( ref Type t, ref object? result )
            {
                Debug.Assert( t != null && t != typeof( IMixinTestHelper ) && t != typeof(ITestHelperResolvedCallback) );
                Debug.Assert( _depth >= 0 );
                Debug.Assert( result == null );

                if( _depth++ == 0 )
                {
                    _initialRequestedType = t;
                    Type? first = GetResolveTarget( t );
                    if( first == null ) return;
                    if( _fromTypes == null ) _fromTypes = new HashSet<Type>();
                    Type prev = t;
                    do
                    {
                        if( !_fromTypes.Add( prev ) ) throw new Exception( $"ResolveTarget attribute: cyclic references found between types: {prev.FullName} -> {_fromTypes.Select( x => x.FullName! ).Concatenate()}" );
                        prev = first;
                        first = GetResolveTarget( first );
                    }
                    while( first != null );
                    result = _container.GetService( prev );
                    if( result != null )
                    {
                        foreach( var o in _fromTypes )
                        {
                            if( _container.GetService( prev ) == null ) _container.Add( o, result );
                        }
                        _fromTypes.Clear();
                        --_depth;
                        return;
                    }
                    t = prev;
                }
            }

            public static Type? GetResolveTarget( Type target )
            {
                var t = ((ResolveTargetAttribute?)target.GetCustomAttribute( typeof( ResolveTargetAttribute ) ))?.Target;
                if( t != null && !t.IsInterface )
                {
                    Throw.ArgumentException( $"ResolveTarget attribute on {target.FullName}: must be an interface.", nameof( target ) );
                }
                return t;
            }

            public void AddMapping( Type t, object result )
            {
                if( _initialRequestedType == t ) _initialRequestedTypeResult = result;
                _container.Add( t, result );
                _fromTypes?.Remove( t );
            }

            public object? GetAlreayResolved( Type t ) => _container.GetService( t );

            public object? Stop( Type t, object? result, bool mappingWithResolvedTarget )
            {
                Debug.Assert( t != null );
                Debug.Assert( _depth >= 1 );
                if( result != null )
                {
                    if( !mappingWithResolvedTarget ) AddMapping( t, result );
                    if( !result.GetType().Assembly.IsDynamic && result is ITestHelperResolvedCallback cb )
                    {
                        if( _created == null ) _created = new List<ITestHelperResolvedCallback>();
                        _created.Add( cb );
                    }
                    if( _depth == 1 )
                    {
                        if( _fromTypes != null ) foreach( var o in _fromTypes ) _container.Add( o, result );
                        if( _created != null && _created.Count > 0 )
                        {
                            foreach( var c in _created ) c.OnTestHelperGraphResolved( result );
                        }
                        if( mappingWithResolvedTarget ) _initialRequestedTypeResult = result;
                    }
                }
                if( --_depth == 0 )
                {
                    _fromTypes?.Clear();
                    _created?.Clear();
                    Debug.Assert( _initialRequestedTypeResult != null );
                    return _initialRequestedTypeResult;
                }
                return result;
            }

        }

        ResolverImpl( ITestHelperConfiguration config )
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
                        var ctx = new Context( _container );
                        foreach( var preLoad in _preLoadedTypes ) Resolve( ctx, preLoad );
                    }
                }
            }
            else _preLoadedTypes = Type.EmptyTypes;
        }

        public bool TransientMode { get; }

        public IReadOnlyList<Type> PreLoadedTypes => _preLoadedTypes;

        public object Resolve( Type t )
        {
            Throw.CheckNotNullArgument( t );
            using( WeakAssemblyNameResolver.TemporaryInstall() )
            {
                Context ctx; 
                if( !TransientMode ) ctx = new Context( _container );
                else
                {
                    ctx = new Context( new SimpleServiceContainer( _container ) );
                    foreach( var preLoad in _preLoadedTypes ) Resolve( ctx, preLoad );
                }
                var r = Resolve( ctx, t );
                if( r == null ) Throw.Exception( $"Unable to resolve type '{t.AssemblyQualifiedName}'." );
                return r;
            }
        }

        object? Resolve( Context ctx, Type t )
        {
            object? result = ctx.GetAlreayResolved( t );
            if( result == null && t != typeof(ITestHelperResolvedCallback) && t != typeof(IMixinTestHelper) )
            {
                ctx.Start( ref t, ref result );
                if( result != null ) return result;
                Type? mappingResolvedTarget = null;
                if( !t.IsClass || t.IsAbstract )
                {
                    Type tMapped = MapType( t, throwOnError: true )!;
                    bool isDynamicType = tMapped.Assembly.IsDynamic;
                    if( !isDynamicType
                        && ctx.CallDepth == 1
                        && (mappingResolvedTarget = Context.GetResolveTarget( tMapped )) != null )
                    {
                        result = Resolve( ctx, mappingResolvedTarget );
                    }
                    else result = Create( ctx, tMapped );
                    if( !isDynamicType && mappingResolvedTarget == null )
                    {
                        Debug.Assert( result != null );
                        ctx.AddMapping( tMapped, result );
                    }
                }
                else result = Create( ctx, t );
                return ctx.Stop( t, result, mappingResolvedTarget != null );
            }
            return result;
        }

        Type? MapType( Type t, bool throwOnError )
        {
            Debug.Assert( t != typeof( ITestHelperResolvedCallback ) && t != typeof( IMixinTestHelper ) );
            string? typeName = _config.Get( "TestHelper/" + t.FullName );
            if( typeName != null )
            {
                // Always throw when config is used.
                Type? fromConfig = SimpleTypeFinder.WeakResolver( typeName, true );
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
                Type? found = SimpleTypeFinder.WeakResolver( fullName, false );
                if( found == null && cName.EndsWith( "Core" ) )
                {
                    var nameNoCore = cName.Remove( cName.Length - 4 );
                    var ns = t.Namespace?.Split( '.' ).ToList();
                    while( ns != null && ns.Count > 0 )
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

        object Create( Context ctx, Type t )
        {
            Debug.Assert( t != null && t.IsClass && !t.IsAbstract );
            var longestCtor = t.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                                .Select( x => Tuple.Create( x, x.GetParameters() ) )
                                .OrderByDescending( x => x.Item2.Length )
                                .Select( x => new
                                {
                                    Ctor = x.Item1,
                                    Parameters = x.Item2,
                                    Values = new object?[x.Item2.Length]
                                } )
                                .FirstOrDefault();
            if( longestCtor == null )
            {
                Throw.Exception( $"Unable to find a public constructor for '{t.FullName}'." );
                return null!;
            }
            for( int i = 0; i < longestCtor.Parameters.Length; ++i )
            {
                var p = longestCtor.Parameters[i];
                bool shouldBePreloaded = p.Name == "isMissingFromPreloaded" && p.ParameterType == typeof(bool);
                if( shouldBePreloaded )
                {
                    longestCtor.Values[i] = !_preLoadedTypes.Any( preloaded => preloaded.IsAssignableFrom( t )
                                                                               || (MapType( preloaded, false )?.IsAssignableFrom( t ) ?? false) );
                }
                else
                {
                    // We generated the Type dynamically... but:
                    // https://github.com/dotnet/corefx/issues/17943
                    // longestCtor.Values[i] = Resolve( container, p.ParameterType, !p.HasDefaultValue ) ?? p.DefaultValue;
                    longestCtor.Values[i] = Resolve( ctx, p.ParameterType );
                }
            }
            return longestCtor.Ctor.Invoke( longestCtor.Values );
        }

        public static ITestHelperResolver Create( ITestHelperConfiguration? config = null )
        {
            using( WeakAssemblyNameResolver.TemporaryInstall() )
            {
                return new ResolverImpl( config ?? TestHelperConfiguration.Default );
            }
        }
    }
}
