using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System.Text.Json;

namespace CK.Testing.Tests;

[TestFixture]
public class JsonIdempotenceTests
{
    [Test]
    public void rewriting_shorter_does_not_show_the_trailing()
    {
        ITestHelperResolver resolver = TestHelperResolver.Create( new TestHelperConfiguration() );
        var h = resolver.Resolve<IBasicTestHelper>();
        h.JsonIdempotenceCheck( "long initial write.", Writer, Reader ).Should().Be( "long initial write." );

        string? text1 = null, text2 = null;
        FluentActions.Invoking( () => h.JsonIdempotenceCheck( "long initial write.", Writer, BuggyReader, jsonText1: t => text1 = t, jsonText2: t => text2 = t ) )
            .Should().Throw<CKException>()
            .WithMessage( """
                Json idempotence failure between first write:
                {"P":"long initial write."}

                And second write of the read back string instance:
                {"P":"long in"}

                """ );
        text1.Should().Be( """{"P":"long initial write."}""" );
        text2.Should().Be( """{"P":"long in"}""" );
    }

    static void Writer( Utf8JsonWriter writer, string s )
    {
        writer.WriteStartObject();
        writer.WriteString( "P", s );
        writer.WriteEndObject();
    }

    static string Reader( ref Utf8JsonReader r, IUtf8JsonReaderContext context )
    {
        r.Read();
        r.GetString().Should().Be( "P" );
        r.Read();
        var s = r.GetString();
        r.Read();
        r.Read();
        return s!;
    }

    static string BuggyReader( ref Utf8JsonReader r, IUtf8JsonReaderContext context )
    {
        return Reader( ref r, context ).Substring( 0, 7 );
    }
}
