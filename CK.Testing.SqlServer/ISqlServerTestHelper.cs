namespace CK.Testing
{
    /// <summary>
    /// Mixin of <see cref="SqlServer.ISqlServerTestHelperCore"/> and <see cref="IMonitorTestHelper"/>.
    /// </summary>
    public interface ISqlServerTestHelper : IMixinTestHelper, SqlServer.ISqlServerTestHelperCore, IMonitorTestHelper
    {
    }
}
