using CK.Core;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace CK.Testing.Tests;

[TestFixture]
public class ShouldlyTests
{
    static async Task<int> VTypeAsync( bool error )
    {
        await Task.Delay( 0 );
        if( error ) throw new CKException( "Intentional error." );
        return 3712;
    }

    static async Task<string> RTypeAsync( bool error )
    {
        await Task.Delay( 0 );
        if( error ) throw new CKException( "Intentional error." );
        return "3712";
    }

    [Test]
    public async Task ShouldNotThrowAsync()
    {
        await Util.Awaitable( () => Task.Delay( 15 ) ).ShouldNotThrowAsync();
        await Util.Awaitable( () => RTypeAsync( false ) ).ShouldNotThrowAsync();
        await Util.Awaitable( async () => await VTypeAsync( false ) ).ShouldNotThrowAsync();
    }

    [Test]
    public async Task ShouldThrowAsync()
    {
        (await Util.Awaitable( () => RTypeAsync( true ) ).ShouldThrowAsync<Exception>())
            .Message.ShouldBe( "Intentional error." );
        (await Util.Awaitable( async () => await VTypeAsync( true ) ).ShouldThrowAsync<Exception>())
            .Message.ShouldBe( "Intentional error." );
    }

    [Test]
    public void ShouldAll_display()
    {
        int[] values = [0, 1, 2];
        Util.Invokable( () => values.ShouldAll( i => i.ShouldBePositive( "Subordinated message." ) ) )
            .ShouldThrow<ShouldAssertException>()
            .Message.ShouldBe( """
            ShouldAll failed for item n°0.
              | Util.Invokable( values.ShouldAll( i => i
              |     should be positive but
              | 0
              |     is negative
              | 
              | Additional Info:
              |     Subordinated message.
            """ );
    }

}
