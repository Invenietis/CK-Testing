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
        public void Reset_Database()
        {
            TestHelper.EnsureDatabase( reset: true );
            TestHelper.ExecuteScripts( File.ReadAllText( TestHelper.TestProjectFolder.AppendPart( "Model.Sql" ) ) );
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
