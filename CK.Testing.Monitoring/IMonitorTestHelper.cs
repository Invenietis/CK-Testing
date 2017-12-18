using CK.Core;
using System;

namespace CK.Testing
{
    /// <summary>
    /// Mixin of <see cref="IMonitorTestHelperCore"/> and <see cref="IBasicTestHelper"/>.
    /// </summary>
    public interface IMonitorTestHelper : IMixinTestHelper, IMonitorTestHelperCore, IBasicTestHelper
    {
    }
}
