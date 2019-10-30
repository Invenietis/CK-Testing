using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing.Tests
{

    public interface IShouldBePreloadedCore
    {
    }

    public interface IShouldBePreloaded : IMixinTestHelper, IBasicTestHelper, IShouldBePreloadedCore
    {
    }

    public class ShouldBePreloaded : IShouldBePreloadedCore
    {
        readonly IBasicTestHelper _basic;

        internal ShouldBePreloaded( IBasicTestHelper basic, bool isMissingFromPreloaded )
        {
            _basic = basic;
            if( isMissingFromPreloaded )
            {
                throw new Exception( $"Assembly {GetType().Assembly.GetName().Name} should appear in 'TestHelper/PreLoadedAssemblies' configuration." );
            }
        }
    }

    [TestFixture]
    public class ShouldBePreloadedTests
    {
        [Test]
        public void isMissingFromPreloaded_constructor_parameter_does_the_job()
        {
            
            var r = TestHelperResolver.Create();
            r.Invoking( sut => sut.Resolve( typeof( ShouldBePreloaded ) ) ).Should()
                                    .Throw<TargetInvocationException>()
                                    .WithInnerException<Exception>()
                                    .WithMessage( "*appear in 'TestHelper/PreLoadedAssemblies'*" );
        }
    }
}
