using CK.Core;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CK.Testing.SqlServerTestHelper;

namespace SqlHelperTests
{
    [TestFixture]
    public class DBLayerTests
    {
        static int _consoleToggleCount = 0;

        [Explicit]
        [Test]
        public void toggle_console_output()
        {
            TestHelper.Monitor.Info( $"Before Toggle n°{++_consoleToggleCount}" );
            TestHelper.LogToConsole = !TestHelper.LogToConsole;
            TestHelper.Monitor.Info( $"After Toggle n°{_consoleToggleCount}" );
        }

        [Explicit]
        [TestCase( "closeExistingConnections" )]
        [TestCase( "" )]
        public void drop_database( string mode )
        {
            TestHelper.DropDatabase( closeExistingConnections: mode == "closeExistingConnections" );
        }

        [TestCase( "reset" )]
        [TestCase( "" )]
        [Explicit]
        public void ensure_database( string reset )
        {
            TestHelper.EnsureDatabase( reset: reset == "reset" );
        }


        [Test]
        public void dropping_database_multiple_times()
        {
            TestHelper.EnsureDatabase( reset: false );
            TestHelper.DropDatabase();
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests = null;
            TestHelper.DropDatabase();
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
        }

        [Test]
        public void Execute_create_script_on_Database_and_Drop()
        {
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests = null;
            TestHelper.EnsureDatabase( reset: true );
            TestHelper.ExecuteScripts( File.ReadAllText( TestHelper.TestProjectFolder.AppendPart( "Model.Sql" ) ) );
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests = null;
            TestHelper.DropDatabase();
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
            TestHelper.DropDatabase();
            CK.Testing.StupidTestHelper.StaticLastDatabaseCreatedOrDroppedNameToTestPreLoadedAssembliesFromSqlHelperTests.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
        }

        [Test]
        public void connection_string()
        {
            TestHelper.Monitor.Info( $"Current User: {Environment.UserDomainName}/{Environment.UserName}" );
            var c = TestHelper.MasterConnectionString;
            c.Should().Contain( "master" ).And.Contain( "Integrated Security" );
            var c2 = TestHelper.GetConnectionString( "Toto" );
            c2.Should().Contain( "Toto" ).And.Contain( "Integrated Security" );
            c = TestHelper.MasterConnectionString;
            c.Should().Contain( "master" ).And.Contain( "Integrated Security" );
        }

        /// <summary>
        /// Calls <see cref="CK.Testing.SqlServer.BackupManager.CreateBackup(string?)(string)"/> on the
        /// default database (<see cref="CK.Testing.SqlServer.ISqlServerTestHelperCore.DefaultDatabaseOptions"/>).
        /// </summary>
        [Test]
        [Explicit]
        public void backup_create()
        {
            Assert.That( TestHelper.Backup.CreateBackup() != null, "Backup should be possible." );
        }

        /// <summary>
        /// Calls <see cref="CK.Testing.SqlServer.BackupManager.CreateBackup(string?)"/> on the
        /// default database (<see cref="CK.Testing.SqlServer.ISqlServerTestHelperCore.DefaultDatabaseOptions"/>).
        /// </summary>
        [TestCase( "0 - Most recent one." )]
        [TestCase( "1" )]
        [TestCase( "2" )]
        [TestCase( "3" )]
        [TestCase( "4" )]
        [TestCase( "5" )]
        [TestCase( "X - Oldest one." )]
        [Explicit]
        public void backup_restore( string what )
        {
            if( !int.TryParse( what, out var index ) )
            {
                index = what[0] == 'X' ? Int32.MaxValue : 0;
            }
            Assert.That( TestHelper.Backup.RestoreBackup( null, index ) != null, "Restoring should be possible." );
        }

        /// <summary>
        /// Dumps all the available backup files in <see cref="CK.Testing.SqlServer.BackupManager.BackupFolder"/>
        /// as information into the <see cref="CK.Testing.Monitoring.IMonitorTestHelperCore.Monitor"/>.
        /// </summary>
        [Test]
        [Explicit]
        public void backup_list()
        {
            var all = TestHelper.Backup.GetAllBackups();
            using( TestHelper.Monitor.OpenInfo( $"There is {all.Count} backups available in '{TestHelper.Backup.BackupFolder}'." ) )
            {
                TestHelper.Monitor.Info( all.Select( a => $"n° {a.Index} - {a.FileName}" ).Concatenate( Environment.NewLine ) );
            }
        }
    }
}
