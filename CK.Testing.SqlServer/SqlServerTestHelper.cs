using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CK.Core;
using CK.Testing.SqlServer;
using CK.Text;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="ISqlServerTestHelperCore"/>.
    /// </summary>
    public class SqlServerTestHelper : ISqlServerTestHelperCore
    {
        readonly ITestHelperConfiguration _config;
        readonly IMonitorTestHelper _monitor;
        static Version _serverVersion;
        static int _maxCompatibilityLevel;
        static ISqlServerDatabaseOptions _defaultDatabaseOptions;
        static SqlConnectionStringBuilder _masterConnectionString;
        event EventHandler<SqlServerDatabaseEventArgs> _onEvent;

        public SqlServerTestHelper( ITestHelperConfiguration config, IMonitorTestHelper monitor )
        {
            _config = config;
            _monitor = monitor;
        }

        string ISqlServerTestHelperCore.MasterConnectionString => EnsureMasterConnection().InitialCatalog;

        ISqlServerDatabaseOptions ISqlServerTestHelperCore.DefaultDatabaseOptions => DoGetDefaultDatabaseOptions();

        SqlServerDatabaseOptions ISqlServerTestHelperCore.GetDatabaseOptions( string databaseName ) => DoGetDatabaseOptions( databaseName );

        string ISqlServerTestHelperCore.GetConnectionString( string databaseName ) => DoGetConnectionString( databaseName );

        bool ISqlServerTestHelperCore.ExecuteScripts( IEnumerable<string> scripts, string databaseName ) => DoExecuteScripts( scripts, databaseName );

        bool ISqlServerTestHelperCore.ExecuteScripts( string scripts, string databaseName ) => DoExecuteScripts( new[] { scripts }, databaseName );

        void ISqlServerTestHelperCore.EnsureDatabase( ISqlServerDatabaseOptions o, bool reset )
        {
            if( o == null ) o = DoGetDefaultDatabaseOptions();
            using( _monitor.Monitor.OpenInfo( $"Ensuring database '{o.ToString()}'." ) )
            {
                try
                {
                    int normalizedLevel = o.CompatibilityLevel;
                    if( normalizedLevel == _maxCompatibilityLevel ) normalizedLevel = 0;
                    var current = DoGetDatabaseOptions( o.DatabaseName );
                    if( current != null )
                    {
                        if( !reset )
                        {
                            reset = current.Collation != o.Collation || current.CompatibilityLevel != normalizedLevel;
                        }
                        if( reset ) DoDrop( current.DatabaseName );
                    }
                    string create = $@"create database {o.DatabaseName} collate {o.Collation};";
                    if( normalizedLevel != 0 )
                    {
                        create += Environment.NewLine + "go" + Environment.NewLine;
                        create += $"alter database {o.DatabaseName} set compatibility_level = {normalizedLevel}";
                    }
                    using( var oCon = new SqlConnection( EnsureMasterConnection().ToString() ) )
                    using( var cmd = new SqlCommand( create, oCon ) )
                    {
                        oCon.Open();
                        cmd.ExecuteNonQuery();
                    }
                    _onEvent?.Invoke( this, new SqlServerDatabaseEventArgs( DoGetDatabaseOptions( o.DatabaseName ), false ) );
                }
                catch( Exception ex )
                {
                    _monitor.Monitor.Error( ex );
                    throw;
                }
            }
        }

        SqlConnection ISqlServerTestHelperCore.CreateOpenedConnection( string databaseName ) => DoCreateOpenedConnection( databaseName );

        Task<SqlConnection> ISqlServerTestHelperCore.CreateOpenedConnectionAsync(string databaseName) => DoCreateOpenedConnectionAsync( databaseName );

        void ISqlServerTestHelperCore.DropDatabase( string databaseName )
        {
            var o = DoGetDatabaseOptions( databaseName );
            if( o != null )
            {
                DoDrop( databaseName );
                _onEvent?.Invoke( this, new SqlServerDatabaseEventArgs( o, true ) );
            }
        }

        event EventHandler<SqlServerDatabaseEventArgs> ISqlServerTestHelperCore.OnDatabaseCreatedOrDropped
        {
            add => _onEvent += value;
            remove => _onEvent -= value;
        }

        SqlConnection DoCreateOpenedConnection( string databaseName )
        {
            var c = DoGetConnectionString( databaseName );
            var oCon = new SqlConnection( c );
            oCon.Open();
            return oCon;
        }

        async Task<SqlConnection> DoCreateOpenedConnectionAsync( string databaseName )
        {
            var c = DoGetConnectionString( databaseName );
            var oCon = new SqlConnection( c );
            await oCon.OpenAsync();
            return oCon;
        }

        SqlServerDatabaseOptions DoGetDatabaseOptions( string dbName )
        {
            const string info = "select compatibility_level, collation_name from sys.databases where name=@N;"; 
            using( var oCon = new SqlConnection( EnsureMasterConnection().ToString() ) )
            using( var cmd = new SqlCommand( info, oCon ) )
            {
                oCon.Open();
                _monitor.OnlyOnce( () =>
                {
                    using( var cmdV = new SqlCommand( "select SERVERPROPERTY('ProductVersion')", oCon ) )
                    {
                        _serverVersion = Version.Parse( (string)cmdV.ExecuteScalar() );
                        _maxCompatibilityLevel = _serverVersion.Major * 10;
                        _monitor.Monitor.Info( $"Sql Server Version: {_serverVersion}, MaxCompatibilityLevel: {_maxCompatibilityLevel}." );
                    }
                } );
                cmd.Parameters.AddWithValue( "@N", dbName );
                using( var r = cmd.ExecuteReader() )
                {
                    if( !r.Read() ) return null;
                    int level = r.GetByte( 0 );
                    if( level == _maxCompatibilityLevel ) level = 0;
                    return new SqlServerDatabaseOptions()
                    {
                        DatabaseName = dbName,
                        CompatibilityLevel = level,
                        Collation = r.GetString( 1 )
                    };
                }
            }
        }

        ISqlServerDatabaseOptions DoGetDefaultDatabaseOptions()
        {
            if( _defaultDatabaseOptions == null )
            {
                _monitor.OnlyOnce( () =>
                {
                    using( var oCon = new SqlConnection( EnsureMasterConnection().ToString() ) )
                    using( var cmd = new SqlCommand( "select SERVERPROPERTY('ProductVersion')", oCon ) )
                    {
                        oCon.Open();
                        _serverVersion = Version.Parse( (string)cmd.ExecuteScalar() );
                        _maxCompatibilityLevel = _serverVersion.Major * 10;
                    }
                    _defaultDatabaseOptions = new SqlServerDatabaseOptions()
                    {
                        DatabaseName = _config.Get( "SqlServer/DatabaseName" ) ?? _monitor.TestProjectName.Replace( '.', '_' ),
                        Collation = _config.Get( "SqlServer/Collation" ) ?? "Latin1_General_100_BIN2",
                        CompatibilityLevel = _config.GetInt32( "SqlServer/CompatibilityLevel" ) ?? _maxCompatibilityLevel
                    };
                } );
            }
            return _defaultDatabaseOptions;
        }

        string DoDrop( string dbName )
        {
            using( _monitor.Monitor.OpenInfo( $"Dropping database '{dbName}'." ) )
            {
                try
                {
                    string delBackup = $"exec msdb.dbo.sp_delete_database_backuphistory @database_name = N'{dbName}';";
                    string c = DoGetConnectionString( dbName );
                    using( var oCon = new SqlConnection( c ) )
                    using( var cmd = new SqlCommand( delBackup, oCon ) )
                    {
                        oCon.Open();
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"use master; drop database {dbName};";
                        cmd.ExecuteNonQuery();
                    }
                    return c;
                }
                catch( Exception ex )
                {
                    _monitor.Monitor.Error( ex );
                    throw;
                }
            }
        }

        #region Execute scripts
        static readonly Regex _rGo = new Regex( @"^\s*GO(?:\s|$)+", RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled );

        bool DoExecuteScripts( IEnumerable<string> scripts, string databaseName )
        {
            using( var oCon = DoCreateOpenedConnection( databaseName ) )
            using( _monitor.Monitor.OpenInfo( $"Executing scripts on '{oCon.Database}'." ) )
            using( var cmd = new SqlCommand() )
            {
                try
                {
                    cmd.Connection = oCon;
                    foreach( var g in scripts )
                    {
                        if( !String.IsNullOrWhiteSpace( g ) )
                        {
                            foreach( var s in SplitGoSeparator( g ) )
                            {
                                _monitor.Monitor.Debug( s );
                                cmd.CommandText = s;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    return true;
                }
                catch( Exception ex )
                {
                    _monitor.Monitor.Error( ex );
                    return false;
                }
            }
        }

        static IEnumerable<string> SplitGoSeparator( string script )
        {
            if( !string.IsNullOrWhiteSpace( script ) )
            {
                int curBeg = 0;
                for( Match goDelim = _rGo.Match( script ); goDelim.Success; goDelim = goDelim.NextMatch() )
                {
                    int lenScript = goDelim.Index - curBeg;
                    if( lenScript > 0 )
                    {
                        yield return script.Substring( curBeg, lenScript );
                    }
                    curBeg = goDelim.Index + goDelim.Length;
                }
                if( script.Length > curBeg )
                {
                    yield return script.Substring( curBeg ).TrimEnd();
                }
            }
        }

        #endregion

        string DoGetConnectionString( string dbName )
        {
            if( dbName == null ) dbName = DoGetDefaultDatabaseOptions().DatabaseName;
            var c = EnsureMasterConnection();
            string savedMaster = c.InitialCatalog;
            c.InitialCatalog = dbName;
            string result = c.ToString();
            c.InitialCatalog = savedMaster;
            return result;
        }

        SqlConnectionStringBuilder EnsureMasterConnection()
        {
            if( _masterConnectionString == null )
            {
                _monitor.OnlyOnce( () =>
                {
                    string c = _config.Get( "SqlServer/MasterConnectionString" );
                    if( c == null )
                    {
                        c = "Server=.;Database=master;Integrated Security=SSPI";
                        _monitor.Monitor.Info( $"Using default connection string: {c}" );
                    }
                    _masterConnectionString = new SqlConnectionStringBuilder( c );
                } );
            }
            return _masterConnectionString;
        }

        /// <summary>
        /// Gets the <see cref="ISqlServerTestHelper"/> default implementation.
        /// </summary>
        public static ISqlServerTestHelper TestHelper => TestHelperResolver.Default.Resolve<ISqlServerTestHelper>();

    }
}
