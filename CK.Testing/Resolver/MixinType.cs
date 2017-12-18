using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Linq;
using CK.Reflection;
using System.Diagnostics;

namespace CK.Testing
{
    static class MixinType
    {
        const string _nsAndAssemblyName = "CK.Testing.Dyn";
        static int _typeID;
        static readonly ModuleBuilder _moduleBuilder;
        static readonly MethodInfo _delegateCombine;
        static readonly MethodInfo _delegateGetInvocationList;
        static readonly MethodInfo _delegateGetMethod;
        static readonly MethodInfo _delegateRemove;
        static readonly object _lock;

        static MixinType()
        {
            // Creates a new Assembly for running only (not saved).
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly( new AssemblyName( _nsAndAssemblyName ), AssemblyBuilderAccess.Run );
            // Creates a new Module
            _moduleBuilder = assemblyBuilder.DefineDynamicModule( "ProxiesModule" );

            _delegateGetInvocationList = typeof( Delegate ).GetMethod( "GetInvocationList", Type.EmptyTypes );
            _delegateGetMethod = typeof( Delegate ).GetMethod( "get_Method", Type.EmptyTypes );

            Type[] paramTwoDelegates = new Type[] { typeof( Delegate ), typeof( Delegate ) };
            _delegateCombine = typeof( Delegate ).GetMethod( "Combine", paramTwoDelegates );
            _delegateRemove = typeof( Delegate ).GetMethod( "Remove", paramTwoDelegates );
            _lock = new object();
        }

        internal static Type Create( Type tMixin )
        {
            lock( _lock )
            {
                string typeName = $"{_nsAndAssemblyName}.Gen{++_typeID}";

                var baseInterfaces = tMixin.GetInterfaces();
                var allInterfaces = new Type[baseInterfaces.Length + 1];
                allInterfaces[0] = tMixin;
                baseInterfaces.CopyTo( allInterfaces, 1 );

                // Defines a public sealed class that implements typeInterface only.
                TypeBuilder tB = _moduleBuilder.DefineType(
                        typeName,
                        TypeAttributes.Class | TypeAttributes.Sealed,
                        null,
                        allInterfaces );

                var interfaces = allInterfaces.Where( i => i.GetMembers().Length > 0 ).ToArray();
                var fields = interfaces
                        .Select( ( i, num ) => tB.DefineField( $"_impl{num}", i, FieldAttributes.Private | FieldAttributes.InitOnly ) )
                        .ToArray();

                ConstructorBuilder ctor = tB.DefineConstructor(
                        MethodAttributes.Public,
                        CallingConventions.Standard,
                        interfaces );
                // ctor
                {
                    var g = ctor.GetILGenerator();
                    for( int i = 0; i < fields.Length; ++i )
                    {
                        g.LdArg( 0 );
                        g.LdArg( i + 1 );
                        g.Emit( OpCodes.Stfld, fields[i] );
                    }
                    g.Emit( OpCodes.Ret );
                }
                for( int i = 0; i < interfaces.Length; ++i )
                {
                    foreach( MethodInfo m in interfaces[i].GetMethods() )
                    {
                        GenerateMethod( tB, m, fields[i] );
                    }
                }
                return tB.CreateTypeInfo().AsType();
            }
        }

        static void GenerateMethod( TypeBuilder tB, MethodInfo m, FieldBuilder impl )
        {
            Type[] parameters;
            MethodBuilder mB = CreateInterfaceMethodBuilder( tB, m, out parameters );
            var g = mB.GetILGenerator();
            // Pushes the impl field on the stack.
            g.Emit( OpCodes.Ldarg_0 );
            g.Emit( OpCodes.Ldfld, impl );
            // Pushes all the parameters on the stack.
            g.RepushActualParameters( false, parameters.Length );
            g.EmitCall( OpCodes.Callvirt, m, null );
            g.Emit( OpCodes.Ret );
        }

        static MethodBuilder CreateInterfaceMethodBuilder( TypeBuilder typeBuilder, MethodInfo m, out Type[] parameters )
        {
            // Initializes the signature with only its name, attributes and calling conventions first.
            MethodBuilder mB = typeBuilder.DefineMethod(
                m.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.Final,
                CallingConventions.HasThis );

            parameters = ReflectionHelper.CreateParametersType( m.GetParameters() );
            // If it is a Generic method definition (since we are working with an interface, 
            // it can not be a Generic closed nor opened method).
            if( m.IsGenericMethodDefinition )
            {
                Type[] genArgs = m.GetGenericArguments();
                Debug.Assert( genArgs.Length > 0 );
                string[] names = new string[genArgs.Length];
                for( int i = 0; i < names.Length; ++i ) names[i] = genArgs[i].Name;
                // Defines generic parameters.
                GenericTypeParameterBuilder[] genTypes = mB.DefineGenericParameters( names );
                for( int i = 0; i < names.Length; ++i )
                {
                    Type source = genArgs[i];
                    GenericTypeParameterBuilder target = genTypes[i];
                    target.SetGenericParameterAttributes( source.GenericParameterAttributes );
                    Type[] sourceConstraints = source.GetGenericParameterConstraints();
                    List<Type> interfaces = new List<Type>();
                    foreach( Type c in sourceConstraints )
                    {
                        if( c.IsClass ) target.SetBaseTypeConstraint( c );
                        else interfaces.Add( c );
                    }
                    target.SetInterfaceConstraints( interfaces.ToArray() );
                }
            }
            // Now that generic parameters have been defined, configures the signature.
            mB.SetReturnType( m.ReturnType );
            mB.SetParameters( parameters );
            // Set DebuggerStepThroughAttribute.
            ConstructorInfo ctor = typeof( DebuggerStepThroughAttribute ).GetConstructor( Type.EmptyTypes );
            CustomAttributeBuilder attr = new CustomAttributeBuilder( ctor, new object[0] );
            mB.SetCustomAttribute( attr );
            return mB;
        }


    }
}
