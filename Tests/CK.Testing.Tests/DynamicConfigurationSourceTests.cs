using NUnit.Framework;
using Shouldly;
using System.Linq;

namespace CK.Testing.Tests;

[TestFixture]
public class DynamicConfigurationSourceTests
{
    [Test]
    public void simple_changes()
    {
        var c = new DynamicConfigurationSource();
        c.Count().ShouldBe( 0 );
        var t = c.GetReloadToken();
        t.HasChanged.ShouldBeFalse();

        c["Hop"] = "hip";
        t.HasChanged.ShouldBeTrue();

        t = c.GetReloadToken();
        t.HasChanged.ShouldBeFalse();

        c["Hop"] = "hip";
        t.HasChanged.ShouldBeFalse();

        c["Hop"] = "hup";
        t.HasChanged.ShouldBeTrue();
    }

    [Test]
    public void StartBatch_retains_changes()
    {
        var c = new DynamicConfigurationSource();
        var t = c.GetReloadToken();
        t.HasChanged.ShouldBeFalse();

        using( c.StartBatch() )
        {
            c["Hop1"] = "hip";
            c["Hop2"] = "hip";
            t.HasChanged.ShouldBeFalse();
            using( c.StartBatch() )
            {
                c.Remove( "Hop2" ).ShouldBeTrue();
                c.Remove( "Hop2" ).ShouldBeFalse();
            }
            t.HasChanged.ShouldBeFalse();
        }
        t.HasChanged.ShouldBeTrue();
    }
}
