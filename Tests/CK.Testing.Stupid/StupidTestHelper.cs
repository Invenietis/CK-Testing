using CK.Testing.Stupid;
using System;

namespace CK.Testing
{
    public class StupidTestHelper : IStupidTestHelperCore
    {
        readonly ISqlServerTestHelper _sql;

        public static string LastDatabaseCreatedOrDroppedName { get; private set; }

        internal StupidTestHelper( ISqlServerTestHelper sql )
        {
            _sql = sql;
            _sql.OnDatabaseCreatedOrDropped += SqlOnDatabaseCreatedOrDropped;
        }

        private void SqlOnDatabaseCreatedOrDropped( object sender, SqlServer.SqlServerDatabaseEventArgs e )
        {
            LastDatabaseCreatedOrDroppedName = e.DatabaseOptions.DatabaseName;
        }

        /// <summary>
        /// This helper must not be resolved explicitely.
        /// The SqlServerTests App.config defines this dll in TestHelper/Assemblies so that it is pre loaded.
        /// </summary>
        // public static IStupidTestHelper TestHelper => TestHelperResolver.Default.Resolve<IStupidTestHelper>();

    }
}
