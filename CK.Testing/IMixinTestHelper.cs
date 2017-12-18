using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Marker interface for mixin test helpers.
    /// Interfaces that extends this interface can not be explicitely implemented.
    /// </summary>
    public interface IMixinTestHelper : ITestHelper
    {
    }
}
