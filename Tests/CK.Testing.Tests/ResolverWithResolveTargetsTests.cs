using Shouldly;
using NUnit.Framework;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace CK.Testing.Tests;

// RACore => IRACore => IRA

[ResolveTarget( typeof( IRA ) )]
public interface IRACore
{
    object ResolvedForIRACore { get; }
}

public interface IRA : IMixinTestHelper, IBasicTestHelper, IRACore
{
}

[ResolveTarget( typeof( IRACore ) )]
public class RACore : IRACore, ITestHelperResolvedCallback
{
    public object ResolvedForIRACore { get; private set; }

    void ITestHelperResolvedCallback.OnTestHelperGraphResolved( object resolvedObject )
    {
        ResolvedForIRACore = resolvedObject;
    }
}

// IRBCore => IRB, RBCore => IRB

[ResolveTarget( typeof( IRB ) )]
public interface IRBCore
{
    object ResolvedForIRBCore { get; }
}

public interface IRB : IMixinTestHelper, IRA, IRBCore
{
}

[ResolveTarget( typeof( IRB ) )]
public class RBCore : IRBCore, ITestHelperResolvedCallback
{
    readonly IRACore _a;

    public RBCore( IRACore a )
    {
        _a = a;
    }

    public object ResolvedForIRBCore { get; private set; }

    public void OnTestHelperGraphResolved( object resolvedObject )
    {
        ResolvedForIRBCore = resolvedObject;
    }
}

// IRCCore => IRC

public interface IRCCore
{
    object ResolvedForIRCCore { get; }
}

public interface IRC : IMixinTestHelper, IRB, IRCCore
{
}

[ResolveTarget( typeof( IRC ) )]
public class RCCore : IRCCore, ITestHelperResolvedCallback
{
    readonly IRACore _a;

    public RCCore( IRACore a )
    {
        _a = a;
    }

    public object ResolvedForIRCCore { get; private set; }

    public void OnTestHelperGraphResolved( object resolvedObject )
    {
        ResolvedForIRCCore = resolvedObject;
    }
}

[TestFixture]
public class ResolverWithResolveTargetsTests
{
    [Test]
    public void resolving_with_targets_does_not_change_behavior_for_concrete_classes()
    {
        var r = TestHelperResolver.Create( new TestHelperConfiguration() );
        var c = r.Resolve<RACore>();
        c.ShouldBeOfType<RACore>( "Obvious since we used the generic cast accessor." );
        c.ResolvedForIRACore.ShouldBeAssignableTo<IRA>( "The IRA mixin has been injected." );

        var mixinRequest = r.Resolve<IRA>();
        mixinRequest.ShouldBeSameAs( c.ResolvedForIRACore );
    }

    [Test]
    public void resolving_with_targets_with_inner_mixin()
    {
        var r = TestHelperResolver.Create( new TestHelperConfiguration() );
        var c = r.Resolve<RBCore>();
        c.ResolvedForIRBCore.ShouldBeAssignableTo<IRB>();

        var ira = r.Resolve<IRA>();
        ira.ResolvedForIRACore.ShouldBeSameAs( c.ResolvedForIRBCore );
    }

    [Test]
    public void resolving_with_targets_where_only_class_uses_ResolveTarget()
    {
        var r = TestHelperResolver.Create( new TestHelperConfiguration() );
        var c = r.Resolve<IRCCore>();
        c.ShouldBeAssignableTo<IRC>();
        c.ResolvedForIRCCore.ShouldBeSameAs( c );
    }

}
