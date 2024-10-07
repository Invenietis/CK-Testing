using System;

namespace CK.Testing.Stupid;

/// <summary>
/// StupidTestHelper is here to show the mixin implementation.
/// This IStupidTestHelperCore defines the actual (stupid) things that is added to the TestHelper.
/// The actual implementation is in <see cref="StupidTestHelper"/>.
/// </summary>
public interface IStupidTestHelperCore
{
    /// <summary>
    /// Gets the last database name that has been dropped or created by <see cref="SqlServer.ISqlServerTestHelperCore"/>.
    /// This helper subscribes to the <see cref="SqlServer.ISqlServerTestHelperCore.OnDatabaseCreatedOrDropped"/>
    /// and when this event fires, captures the database name (and also calls <see cref="StupidMethod"/>).
    /// </summary>
    string? LastDatabaseCreatedOrDroppedName { get; }

    /// <summary>
    /// Gets the total number of calls to <see cref="StupidMethod"/>.
    /// </summary>
    int CountOfStupidMethodCalls { get; }

    /// <summary>
    /// Does nothing more than incrementing <see cref="CountOfStupidMethodCalls"/> and raising <see cref="OnStupidMethodCalled"/>.
    /// </summary>
    void StupidMethod();

    /// <summary>
    /// Fires on each call to <see cref="StupidMethod"/>.
    /// </summary>
    event EventHandler OnStupidMethodCalled;
}
