using CK.Core;
using CK.Testing.Stupid;
using System;
using System.Linq;

namespace CK.Testing
{
    /// <summary>
    /// StupidTestHelper is here to show the mixin implementation.
    /// This is the actual implementation. Everything is implemented explicitly so that the API exposes
    /// only the <see cref="TestHelper"/> magic static property.
    /// </summary>
    public class StupidTestHelper : IStupidTestHelperCore
    {
        readonly ISqlServerTestHelper _sql;
        string _lastDatabaseCreatedOrDroppedName;
        int _countCall;
        EventHandler _onStupidMethodCalled;

        /// <summary>
        /// Other test helpers will be resolved and injected.
        /// This implementation uses them but focuses on the implementation of its own interface (here the <see cref="IStupidTestHelperCore"/>).
        /// </summary>
        /// <param name="sql">This helper is based on the (real) Sql test helper.</param>
        /// <param name="isMissingFromPreloaded">
        /// Optional parameter that when present states that the TestHelper's assembly should be preloaded.
        /// This can be used when the TestHelper is a "plugin" that reacts to events emitted by
        /// other TestHelpers: preloading this TestHelper means that its behavior will always be here, even if the
        /// TestHelper is not explicitly resolved (or the first to be resolved).
        /// </param>
        internal StupidTestHelper( ISqlServerTestHelper sql, bool isMissingFromPreloaded )
        {
            _sql = sql;
            if( isMissingFromPreloaded )
            {
                // This is not too hard... Just a warning (cound be a throw!).
                _sql.Monitor.Warn( $"Assembly {GetType().Assembly.GetName().Name} should appear in 'TestHelper/PreLoadedAssemblies' configuration." );
            }
            _sql.OnDatabaseCreatedOrDropped += ( source, e ) =>
            {
                _lastDatabaseCreatedOrDroppedName = e.DatabaseOptions.DatabaseName;
                StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests = _lastDatabaseCreatedOrDroppedName;
                DoStupidMethod();
            };
        }

        /// <summary>
        /// We hide the property thanks to an explicit implementation: a TestHelper should expose only
        /// its <see cref="TestHelper"/> magic static property.
        /// </summary>
        string IStupidTestHelperCore.LastDatabaseCreatedOrDroppedName => _lastDatabaseCreatedOrDroppedName;

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
        /// (See <see cref="StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests"/> for tests.)
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static IStupidTestHelper TestHelper => TestHelperResolver.Default.Resolve<IStupidTestHelper>();

        /// <summary>
        /// This TestHelper must not be resolved explicitly by SqlHelper.Tests!
        /// <para>
        /// The SqlHelper.Tests tests the "pre loading of assemblies" feature: the SqlServerTests project's file TestHelper.config
        /// defines this stupid dll in TestHelper/PreLoadedAssemblies so that it is pre loaded.
        /// </para>
        /// </summary>
        static public string StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests { get; set; }

    }
}
