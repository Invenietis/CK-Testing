using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace CK.Testing.SqlServer
{
    /// <summary>
    /// Support sql database related helpers.
    /// Operations exposed here are dangerous. The only check is that the database name can not be
    /// system databases like 'master', 'tempdb' or 'model'.
    /// </summary>
    public interface ISqlServerTestHelperCore
    {
        /// <summary>
        /// Gets the connection string to the master database from "SqlServer/MasterConnectionString" configuration.
        /// Defaults to "Server=.;Database=master;Integrated Security=true".
        /// </summary>
        string MasterConnectionString { get; }

        /// <summary>
        /// Gets the default test database informations.
        /// </summary>
        ISqlServerDatabaseOptions DefaultDatabaseOptions { get; }

        /// <summary>
        /// Gets the connection string based on <see cref="MasterConnectionString"/> to the given database.
        /// </summary>
        /// <param name="databaseName">Database name. Defaults to default <see cref="DefaultDatabaseOptions"/>.</param>
        /// <returns>The connection string to the database.</returns>
        string GetConnectionString( string databaseName = null );

        /// <summary>
        /// Gets the database options. Null if the database does not exist.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The options or null if it does not exist.</returns>
        SqlServerDatabaseOptions GetDatabaseOptions( string databaseName );

        /// <summary>
        /// Checks that the database exists and has the correct options.
        /// Drops and recreates it if needed.
        /// </summary>
        /// <param name="o">Database options to use. Defaults to <see cref="DefaultDatabaseOptions"/>.</param>
        /// <param name="reset">True to drop and recreate the database.</param>
        void EnsureDatabase( ISqlServerDatabaseOptions o = null, bool reset = false );

        /// <summary>
        /// Drops the database.
        /// </summary>
        /// <param name="databaseName">Database name to drop. Defaults to default <see cref="DefaultDatabaseOptions"/>.</param>
        void DropDatabase( string databaseName = null );

        /// <summary>
        /// Fires whenever a database is created, reset or droppped.
        /// </summary>
        event EventHandler<SqlServerDatabaseEventArgs> OnDatabaseCreatedOrDropped;

        /// <summary>
        /// Creates an opened connection to a database.
        /// It must be disposed by the caller.
        /// </summary>
        /// <param name="databaseName">Database name to target. Defaults to default <see cref="DefaultDatabaseOptions"/>.</param>
        /// <returns>An opened connection.</returns>
        SqlConnection CreateOpenedConnection( string databaseName = null );

        /// <summary>
        /// Creates an opened connection to a database.
        /// It must be disposed by the caller.
        /// </summary>
        /// <returns>An opened connection.</returns>
        Task<SqlConnection> CreateOpenedConnectionAsync( string databaseName = null );

        /// <summary>
        /// Executes scripts that may contain 'GO' separators (that must be alone in their line).
        /// </summary>
        /// <param name="scripts">Scripts to execute.</param>
        /// <param name="databaseName">Database name to target. Defaults to default <see cref="DefaultDatabaseOptions"/>.</param>
        /// <remarks>
        /// The 'GO' may be lowercase but must always be alone on its line.
        /// </remarks>
        /// <returns>True on success, false if an error occurred.</returns>
        bool ExecuteScripts( IEnumerable<string> scripts, string databaseName = null );

        /// <summary>
        /// Executes scripts that may contain 'GO' separators (that must be alone in their line).
        /// </summary>
        /// <param name="scripts">Scripts to execute.</param>
        /// <param name="databaseName">Database name to target. Defaults to default <see cref="DefaultDatabaseOptions"/>.</param>
        /// <remarks>
        /// The 'GO' may be lowercase but must always be alone on its line.
        /// </remarks>
        /// <returns>True on success, false if an error occurred.</returns>
        bool ExecuteScripts( string scripts, string databaseName = null );

    }
}
