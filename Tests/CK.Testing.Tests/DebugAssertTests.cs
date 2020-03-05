using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing.Tests
{
    [TestFixture]
    public class DebugAssertTests
    {
        [Test]
        public void Debug_Fail_does_not_crash_the_test_runner_process_but_throws_a_DebugAssertionException()
        {
            // Even with a Module initializer (see the ModuleInit.Fody package for instance), when this
            // test is run with absolutely no interaction with the CK.Testing objects... The initializer is
            // not yet executed because the assembly itself is not yet loaded/initialized...
            // The module initializer is not a solution: we need to trigger "something"!
            // (And this is why we don't use ModuleInit.Fody...)
            //
            // In "normal" use a TestHelper has been required, this initialization has already been done.
            //
            StaticBasicTestHelper.Touch();
            try
            {
                System.Diagnostics.Debug.Assert( 1 == 0, "This should lead to a simple exception, not the death of the process..." );
                Assert.Fail( "This is NEVER reached!" );
            }
            catch( DebugAssertionException ex )
            {
                // Everything is fine!
                ex.Should().NotBeNull();
            }
        }
    }
}
