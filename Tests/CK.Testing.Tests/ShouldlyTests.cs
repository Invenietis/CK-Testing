using CK.Core;
using NUnit.Framework;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace CK.Testing.Tests;

[TestFixture]
public class ShouldlyTests
{
    [Test]
    public void Shouldly_non_generic_ShouldThrow_doesnt_honor_LSP()
    {
        Action bug = () => throw new ArgumentNullException();

        // This is our ShouldThrow (based on Delegate and in the global namespace).
        bug.ShouldThrow( typeof( Exception ) );

        // Same test using Shouldly's non generic ShouldThrow.
        Action withShouldly = () => Shouldly.ShouldThrowExtensions.ShouldThrow( bug, typeof( Exception ) );
        // It throws an assertion exception.
        withShouldly.ShouldThrow<ShouldAssertException>();
    }

    [Test]
    public void ShouldThrow_on_Delegate_correctly_handles_multicast_Delegate()
    {
        Action bug = () => throw new ArgumentNullException();
        Action noBug = () => { };

        Action? combined = noBug;
        combined += bug;

        combined.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void ShouldThrow_on_Func_ValueType_Delegate()
    {
        Func<int> bug = () => throw new ArgumentNullException();
        Func<int> noBug = () => 42;

        Func<int>? combined = noBug;
        combined += bug;

        combined.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void ShouldThrow_on_Func_ReferenceType_Delegate()
    {
        Func<string> bug = () => throw new ArgumentNullException();
        Func<string> noBug = () => "Hello";

        Func<string>? combined = noBug;
        combined += bug;

        combined.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void ShouldThrow_on_Func_Object_Delegate()
    {
        Func<object> bug = () => throw new ArgumentNullException();
        Func<object> noBug = () => "Hello";

        Func<object>? combined = noBug;
        combined += bug;

        combined.ShouldThrow<ArgumentNullException>();
    }

    [Test]
    public void ShouldThrow_on_Delegate_checks_that_Delegate_has_no_parameter()
    {
        Action<int> expectParameter = SomeFunc;

        Action sut = () => expectParameter.ShouldThrow<ArgumentNullException>();

        sut.ShouldThrow<ArgumentException>().Message.ShouldBe( """
            ShouldThrow can only be called on a delegate without parameters.
            Found method ShouldlyTests.SomeFunc( Int32 a )
            """ );
    }

    static void SomeFunc( int a ) => throw new ArgumentNullException();

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


    #region Genuine Shouldy tests (no override needed).
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
    #endregion


}
