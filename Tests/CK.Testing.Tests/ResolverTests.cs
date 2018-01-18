using FluentAssertions;
using NUnit.Framework;
using System;

namespace CK.Testing.Tests
{
    // A is like Monitor.
    public interface IACore
    {
        IBasicTestHelper AToBasicRef { get; }
        int CallACount { get; }
        void DoA();
        event EventHandler ADone;
    }

    public interface IA : IMixinTestHelper, ITestHelper, IBasicTestHelper, IACore
    {
    }

    public class A : IACore
    {
        readonly IBasicTestHelper _basic;
        int _callCount;


        event EventHandler _aDone;

        internal A( IBasicTestHelper basic )
        {
            _basic = basic;
        }

        int IACore.CallACount => _callCount;

        IBasicTestHelper IACore.AToBasicRef => _basic;

        void IACore.DoA()
        {
            _basic.BuildConfiguration.Should().Match( s => s == "Debug" || s == "Release" );
            _basic.TestProjectName.Should().Be( "CK.Testing.Tests" );
            ++_callCount;
            _aDone?.Invoke( this, EventArgs.Empty );
        }

        event EventHandler IACore.ADone
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
        readonly ITestHelperConfiguration _config;
        readonly IA _a;
        readonly IBasicTestHelper _basic;

        internal B( ITestHelperConfiguration config, IA a, IBasicTestHelper basic )
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

        public F( ITestHelperConfiguration config, ID d, IE e, IC c )
        {
            _d = d;
            _e = e;
            _c = c;
        }

        void IFCore.DoF()
        {
        }
    }

    // B is like SqlTransform.
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
        public void resolving_one_simple_mixin_as_singleton()
        {
            int eventCount = 0;
            var a = TestHelperResolver.Default.Resolve<IA>();
            a.ADone += ( e, arg ) => ++eventCount;

            int originalCount = a.CallACount;
            a.DoA();
            a.CallACount.Should().Be( originalCount + 1 );

            var a2 = TestHelperResolver.Default.Resolve<IA>();
            a2.Should().BeSameAs( a );
        }

        [Test]
        public void when_the_Core_implementation_is_not_found_an_exception_is_raised()
        {
            TestHelperResolver.Default.Invoking( sut => sut.Resolve<INotImpl>() )
                                        .ShouldThrow<Exception>()
                                        .Where( e => e.Message.StartsWith( "Unable to locate an implementation for " ) );
        }

        [TestCase( true )]
        [TestCase( false )]
        public void accessing_mixins_as_singleton( bool revert )
        {
            var r = TestHelperResolver.Create();

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
            a.AToBasicRef.Should().BeSameAs( basic );
            b.BToARef.Should().BeSameAs( a );
            c.CToARef.Should().BeSameAs( a );
            c.CToBasicRef.Should().BeSameAs( basic );
        }

    }
}
