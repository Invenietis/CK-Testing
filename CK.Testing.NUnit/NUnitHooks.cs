using CK.Core;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK
{
    public class NUnitHooks
    {
        [SetUp]
        public void OnSetup()
        {
            TestHelper.Monitor.Info( $"{TestContext.CurrentContext.Test.Name}" );
        }

        public static void Touch()
        {
        }

        [TearDown]
        public void OnTearDown()
        {
            TestHelper.Monitor.Info( $"{TestContext.CurrentContext.Test.Name}" );
        }
    }
}
