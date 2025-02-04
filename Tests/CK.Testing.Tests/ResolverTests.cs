using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace CK.Testing.Tests;


// A is like Monitor.
public interface IACore : ITestHelperResolvedCallback
{
    bool ResolvedCallbackCalled { get; }
    object ResolvedObject { get; }
    IBasicTestHelper AToBasicRef { get; }
    int CallACount { get; }
    void DoA();
    event EventHandler ADone;
}

public interface IA : IMixinTestHelper, IBasicTestHelper, IACore
{
}

public class A : IACore
{
    readonly IBasicTestHelper _basic;
    int _callCount;
    bool _cbCalled;
    object? _resolvedObject;

    event EventHandler? _aDone;

    internal A( IBasicTestHelper basic )
    {
        _basic = basic;
    }

    int IACore.CallACount => _callCount;

    IBasicTestHelper IACore.AToBasicRef => _basic;

    public bool ResolvedCallbackCalled => _cbCalled;

    public object ResolvedObject => _resolvedObject!;

    void IACore.DoA()
    {
        _basic.BuildConfiguration.Should().Match( s => s == "Debug" || s == "Release" );
        _basic.TestProjectName.Should().Be( "CK.Testing.Tests" );
        ++_callCount;
        _aDone?.Invoke( this, EventArgs.Empty );
    }

    void ITestHelperResolvedCallback.OnTestHelperGraphResolved( object resolvedObject )
    {
        _cbCalled.Should().BeFalse();
        _cbCalled = true;
        _resolvedObject = resolvedObject;
    }

    event EventHandler? IACore.ADone
    {
        add => _aDone += value;
        remove => _aDone -= value;
    }

}

// B is like SqlTransform.
public interface IBCore
{
    IA BToARef { get; }
    IBasicTestHelper BToBasicRef { get; }
    void DoB();
}

public interface IB : IMixinTestHelper, IA, IBCore
{
}

public class B : IBCore
{
    readonly TestHelperConfiguration _config;
    readonly IA _a;
    readonly IBasicTestHelper _basic;

    internal B( TestHelperConfiguration config, IA a, IBasicTestHelper basic )
    {
        _config = config;
        _a = a;
        _basic = basic;
    }

    IA IBCore.BToARef => _a;

    IBasicTestHelper IBCore.BToBasicRef => _basic;

    void IBCore.DoB() { }
}

// C is like StObjMap
public interface ICCore
{
    IA CToARef { get; }
    IBasicTestHelper CToBasicRef { get; }
    void DoC();
}

public interface IC : IMixinTestHelper, IA, ICCore
{
}

public class C : ICCore
{
    readonly IBasicTestHelper _basic;
    readonly IA _a;

    public C( IA a, IBasicTestHelper basic )
    {
        _a = a;
        _basic = basic;
    }

    IA ICCore.CToARef => _a;

    IBasicTestHelper ICCore.CToBasicRef => _basic;

    void ICCore.DoC()
    {
    }
}

// D is like CKSetup
public interface IDCore
{
    void DoD();
}

public interface ID : IMixinTestHelper, IA, IDCore
{
}

public class D : IDCore
{
    readonly IA _a;

    public D( IA a )
    {
        _a = a;
    }

    void IDCore.DoD()
    {
    }
}

// E is like SqlServer
public interface IECore
{
    void EoE();
}

public interface IE : IMixinTestHelper, IA, IECore
{
}

public class E : IECore
{
    readonly IA _a;

    public E( IA a )
    {
        _a = a;
    }

    void IECore.EoE()
    {
    }
}

// F is like DBSetup.
public interface IFCore
{
    void DoF();
}

public interface IF : IMixinTestHelper, IE, ID, IC
{
}

class F : IFCore
{
    readonly ID _d;
    readonly IE _e;
    readonly IC _c;

    public F( TestHelperConfiguration config, ID d, IE e, IC c )
    {
        _d = d;
        _e = e;
        _c = c;
    }

    void IFCore.DoF()
    {
    }
}

public interface INotImplCore
{
    void Do();
}

public interface INotImpl : IMixinTestHelper, IA, INotImplCore
{
}


[TestFixture]
public class ResolverTests
{
    [Test]
    public void configuration_values_Declared_and_Useless()
    {
        var config = new TestHelperConfiguration();

        string editableValue = "I can change!";

        var pathsAreOptionals = config.Declare( "Anything/nawak/Deep/Ambiguous", "D0", null );
        pathsAreOptionals.ConfiguredValue.Should().Be( "I'm at the root!" );

        var usedAndConfigured = config.DeclareMultiPaths( "Test/MultiPaths", "D1", null ).Value.ToList();
        // A Value not editable AND without default should not exist but this is not checked (its CurrentValue is simply null).
        var unconfiguredWithDefault = config.Declare( "Test/UnconfWithDefault", "the default value", "D2", null );
        var unconfiguredEditable = config.Declare( "Test/UnconfWithEditableValue", "D3", () => editableValue );
        //
        var renamed = config.Declare( "Thing/NewName", "Deprecated name", null, "OldThing/OldName" );
        renamed.Key.Path.Should().Be( "Thing/NewName" );
        renamed.ConfiguredValue.Should().Be( "I'm using a deprecated name." );
        renamed.ObsoleteKeyUsed.Should().Be( "OldName" );


        config.UselessValues.Should().HaveCount( 1 ).And.Contain( x => x.UnusedKey.Path == "Test/UnusedKey" && x.ConfiguredValue == "unused" );
        config.DeclaredValues.Should().HaveCount( 5 );

        // MultiStrings are trimmed but duplicates ';;;;' appear in the ConfiguredValue so that user can see them
        // (the default value is set (if not editable) to the evaluated paths (not tested here).
        config.DeclaredValues.Should().Contain( x => x.Key.Path == "Test/MultiPaths"
                                                     && x.Description == "D1"
                                                     && x.ConfiguredValue == @"{SolutionFolder}..;{TestProjectFolder}/../XXXXX/{BuildConfiguration};;X/../{TestProjectName};{ClosestSUTProjectFolder}/{BuildConfiguration}-{TestProjectName}-{SolutionName}/{PathToBin};../Y;{X}\{BuildConfiguration}\..\Y;;;;;;" );

        config.DeclaredValues.Should().Contain( x => x.Key.Path == "Test/UnconfWithDefault"
                                                     && x.Description == "D2"
                                                     && x.ConfiguredValue == null
                                                     && x.CurrentValue == "the default value" );

        config.DeclaredValues.Should().Contain( x => x.Key.Path == "Test/UnconfWithEditableValue"
                                                     && x.Description == "D3"
                                                     && x.ConfiguredValue == null
                                                     && x.CurrentValue == "I can change!" );

        config.DeclaredValues.Should().Contain( renamed );
        config.DeclaredValues.Should().Contain( pathsAreOptionals );
    }

    [TestCase( "ConfigurationResolvedFirst" )]
    [TestCase( "BasicTestHelperResolvedFirst" )]
    public void configuration_value_as_paths( string mode )
    {

        IBasicTestHelper b;
        TestHelperConfiguration config;

        {
            var resolver = TestHelperResolver.Create( new TestHelperConfiguration() );
            if( mode == "BasicTestHelperResolvedFirst" )
            {
                b = resolver.Resolve<IBasicTestHelper>();
                config = resolver.Resolve<TestHelperConfiguration>();
            }
            else
            {
                config = resolver.Resolve<TestHelperConfiguration>();
                b = resolver.Resolve<IBasicTestHelper>();
            }
        }

        b.SolutionName.Should().Be( "CK-Testing" );
        b.TestProjectName.Should().Be( "CK.Testing.Tests" );
        b.ClosestSUTProjectFolder.Path.Should().EndWith( "CK-Testing/CK.Testing" );

        var paths = config.DeclareMultiPaths( "Test/MultiPaths", "The description of MultiPaths.", null ).Value.ToList();
        paths.Should().HaveCount( 6 );

        paths[0].Should().Be( new NormalizedPath( Path.GetDirectoryName( b.SolutionFolder ) ), "{SolutionFolder}.." );

        paths[1].Should().Be( b.TestProjectFolder.RemoveLastPart().AppendPart( "XXXXX" ).AppendPart( b.BuildConfiguration ), "{TestProjectFolder}/../XXXXX/{BuildConfiguration}" );

        paths[2].Should().Be( b.TestProjectFolder.AppendPart( b.TestProjectName ), "X/../{TestProjectName}" );

        var expectedPlacehoders = $"{b.BuildConfiguration}-{b.TestProjectName}-{b.SolutionName}";
        paths[3].Should().Be( b.ClosestSUTProjectFolder.AppendPart( expectedPlacehoders ).Combine( b.PathToBin ),
            "{ClosestSUTProjectFolder}/{BuildConfiguration}-{TestProjectName}-{SolutionName}/{PathToBin}" );

        paths[4].Should().Be( b.TestProjectFolder.RemoveLastPart().AppendPart( "Y" ), "../Y" );

        // No .. resolved when { appears.
        paths[5].Should().Be( $"{{X}}/{b.BuildConfiguration}/../Y", "Since a { exists, the .. are not resolved. {X}/{BuildConfiguration}/../Y" );
        paths[5].ResolveDots().Should().Be( $"{{X}}/Y" );
    }

    [Test]
    public void resolving_one_simple_mixin_as_singleton()
    {
        ITestHelperResolver resolver = TestHelperResolver.Create( new TestHelperConfiguration() );
        int eventCount = 0;
        var a = resolver.Resolve<IA>();
        a.ADone += ( e, arg ) => ++eventCount;
        a.ResolvedCallbackCalled.Should().BeTrue();
        a.ResolvedObject.Should().BeAssignableTo<IA>( "The resolved object here is the Mixin." );

        int originalCount = a.CallACount;
        a.DoA();
        a.CallACount.Should().Be( originalCount + 1 );

        var a2 = resolver.Resolve<IACore>();
        a2.Should().NotBeSameAs( a, "Accessing IACore returns the A implementation object." );
        a2.DoA();
        a.CallACount.Should().Be( originalCount + 2 );
    }

    [Test]
    public void resolving_one_core_as_singleton()
    {
        ITestHelperResolver resolver = TestHelperResolver.Create( new TestHelperConfiguration() );
        int eventCount = 0;
        var a = resolver.Resolve<IACore>();
        a.ADone += ( e, arg ) => ++eventCount;
        a.ResolvedCallbackCalled.Should().BeTrue();
        a.ResolvedObject.Should().BeAssignableTo<A>( "The resolved object here is NOT a Mixin, It is just the A implementation." );

        int originalCount = a.CallACount;
        a.DoA();
        a.CallACount.Should().Be( originalCount + 1 );

        var a2 = resolver.Resolve<IA>();
        a2.Should().NotBeSameAs( a, "This is the Mixin built on A." );

        a2.DoA();
        a.CallACount.Should().Be( originalCount + 2 );
    }

    [Test]
    public void when_the_Core_implementation_is_not_found_an_exception_is_raised()
    {
        TestHelperResolver.Default.Invoking( sut => sut.Resolve<INotImpl>() )
                                    .Should().Throw<Exception>()
                                    .Where( e => e.Message.StartsWith( "Unable to locate an implementation for " ) );
    }

    [TestCase( true )]
    [TestCase( false )]
    public void accessing_mixins_as_singleton( bool revert )
    {
        var r = TestHelperResolver.Create( new TestHelperConfiguration() );

        IBasicTestHelper basic;
        IA a;
        IB b;
        IC c;
        ID d;
        IE e;
        IF f;
        if( revert )
        {
            f = r.Resolve<IF>();
            e = r.Resolve<IE>();
            d = r.Resolve<ID>();
            c = r.Resolve<IC>();
            b = r.Resolve<IB>();
            a = r.Resolve<IA>();
            basic = r.Resolve<IBasicTestHelper>();
        }
        else
        {
            basic = r.Resolve<IBasicTestHelper>();
            a = r.Resolve<IA>();
            b = r.Resolve<IB>();
            c = r.Resolve<IC>();
            d = r.Resolve<ID>();
            e = r.Resolve<IE>();
            f = r.Resolve<IF>();
        }
        a.ResolvedCallbackCalled.Should().BeTrue();
        a.AToBasicRef.Should().BeSameAs( basic );
        b.BToARef.Should().BeSameAs( a );
        c.CToARef.Should().BeSameAs( a );
        c.CToBasicRef.Should().BeSameAs( basic );
    }

}
