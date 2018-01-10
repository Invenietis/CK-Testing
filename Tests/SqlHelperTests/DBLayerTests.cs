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
        [Test]
        public void Execute_create_script_on_Database_and_Drop()
        {
            TestHelper.EnsureDatabase( reset: true );
            TestHelper.ExecuteScripts( File.ReadAllText( TestHelper.TestProjectFolder.AppendPart( "Model.Sql" ) ) );
            CK.Testing.StupidTestHelper.LastDatabaseCreatedOrDroppedName.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
            CK.Testing.StupidTestHelper.LastDatabaseCreatedOrDroppedName = null;
            TestHelper.DropDatabase();
            CK.Testing.StupidTestHelper.LastDatabaseCreatedOrDroppedName.Should().Be( TestHelper.DefaultDatabaseOptions.DatabaseName );
        }

        [Test]
        public void connection_string()
        {
            var c = TestHelper.MasterConnectionString;
            c.Should().Contain( "master" ).And.Contain( "Integrated Security" );
            var c2 = TestHelper.GetConnectionString( "Toto" );
            c2.Should().Contain( "Toto" ).And.Contain( "Integrated Security" );
            c = TestHelper.MasterConnectionString;
            c.Should().Contain( "master" ).And.Contain( "Integrated Security" );
        }
    }
}
