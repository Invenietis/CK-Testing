using NUnit.Framework;

namespace CK.Testing.NUnit.Tests
{
    [TestFixture]
    public class Class1
    {
        static Class1()
        {
            CK.NUnitHooks.Touch();
        }

        [Test]
        public void opening_closing_grouep()
        {
            Assert.That( true );
        }
    }
}
