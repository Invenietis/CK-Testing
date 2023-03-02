using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace CK.Testing.Tests
{
    [TestFixture]
    public class DynamicConfigurationSourceTests
    {
        [Test]
        public void simple_changes()
        {
            var c = new DynamicConfigurationSource();
            c.Count().Should().Be( 0 );
            var t = c.GetReloadToken();
            t.HasChanged.Should().BeFalse();

            c["Hop"] = "hip";
            t.HasChanged.Should().BeTrue();

            t = c.GetReloadToken();
            t.HasChanged.Should().BeFalse();

            c["Hop"] = "hip";
            t.HasChanged.Should().BeFalse();

            c["Hop"] = "hup";
            t.HasChanged.Should().BeTrue();
        }

        [Test]
        public void StartBatch_retains_changes()
        {
            var c = new DynamicConfigurationSource();
            var t = c.GetReloadToken();
            t.HasChanged.Should().BeFalse();

            using( c.StartBatch() )
            {
                c["Hop1"] = "hip";
                c["Hop2"] = "hip";
                t.HasChanged.Should().BeFalse();
                using( c.StartBatch() )
                {
                    c.Remove( "Hop2" ).Should().BeTrue();
                    c.Remove( "Hop2" ).Should().BeFalse();
                }
                t.HasChanged.Should().BeFalse();
            }
            t.HasChanged.Should().BeTrue();
        }
    }
}
