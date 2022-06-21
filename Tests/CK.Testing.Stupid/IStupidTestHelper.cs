using CK.Testing.Stupid;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// StupidTestHelper is here to show the mixin implementation.
    /// This IStupidTestHelper is the final, automatically implemented facade.
    /// It doesn't define anything: it combines all its interfaces' implementation.
    /// What this StupidTestHelper brings is defined in <see cref="IStupidTestHelperCore"/> and the
    /// implementation is in <see cref="StupidTestHelper"/>.
    /// </summary>
    public interface IStupidTestHelper : ISqlServerTestHelper, IStupidTestHelperCore
    {
    }
}
