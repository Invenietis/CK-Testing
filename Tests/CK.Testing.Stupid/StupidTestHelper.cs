using CK.Core;
using CK.Testing.Stupid;
using System;
using System.Linq;

namespace CK.Testing;

/// <summary>
/// StupidTestHelper is here to show the mixin implementation.
/// This is the actual implementation. Everything is implemented explicitly so that the API exposes
/// only the <see cref="TestHelper"/> magic static property.
/// </summary>
public class StupidTestHelper : IStupidTestHelperCore
{
    readonly ISqlServerTestHelper _sql;
    string? _lastDatabaseCreatedOrDroppedName;
    EventHandler? _onStupidMethodCalled;
    int _countCall;

    /// <summary>
    /// Other test helpers will be resolved and injected.
    /// This implementation uses them but focuses on the implementation of its own interface (here the <see cref="IStupidTestHelperCore"/>).
    /// </summary>
    /// <param name="sql">This helper is based on the (real) Sql test helper.</param>
    internal StupidTestHelper( ISqlServerTestHelper sql )
    {
        _sql = sql;
        _sql.OnDatabaseCreatedOrDropped += ( source, e ) =>
        {
            _lastDatabaseCreatedOrDroppedName = e.DatabaseOptions.DatabaseName;
            DoStupidMethod();
        };
    }

    /// <summary>
    /// We hide the property thanks to an explicit implementation: a TestHelper should expose only
    /// its <see cref="TestHelper"/> magic static property.
    /// </summary>
    string? IStupidTestHelperCore.LastDatabaseCreatedOrDroppedName => _lastDatabaseCreatedOrDroppedName;

    int IStupidTestHelperCore.CountOfStupidMethodCalls => _countCall;

    event EventHandler IStupidTestHelperCore.OnStupidMethodCalled
    {
        add => _onStupidMethodCalled += value;
        remove => _onStupidMethodCalled -= value;
    }

    /// <summary>
    /// It is easier (in my experience) to relay the call to a private method rather than playing with casts of the
    /// explicit interfaces.
    /// </summary>
    void IStupidTestHelperCore.StupidMethod() => DoStupidMethod();

    void DoStupidMethod()
    {
        ++_countCall;
        _onStupidMethodCalled?.Invoke( this, EventArgs.Empty );
    }

    /// <summary>
    /// This is where the final magic occurs: the TestHelper exposed here combines all the <see cref="IMixinTestHelper"/> interfaces
    /// implementation into one facade object.
    /// </summary>
    public static IStupidTestHelper TestHelper => TestHelperResolver.Default.Resolve<IStupidTestHelper>();

}
