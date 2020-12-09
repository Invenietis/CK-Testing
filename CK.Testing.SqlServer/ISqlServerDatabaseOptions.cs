using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing.SqlServer
{
    /// <summary>
    /// Read only aspect of <see cref="SqlServerDatabaseOptions"/>.
    /// Used to expose the configured information.
    /// </summary>
    public interface ISqlServerDatabaseOptions
    {
        /// <summary>
        /// Gets the database name from "SqlServer/DatabaseName" configuration entry.
        /// Defaults to the test project name (<see cref="IBasicTestHelper.TestProjectName"/>)
        /// where '.' are replaced with '_'.
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Gets the database collation from "SqlServer/Collation" configuration entry.
        /// Defaults to 'Latin1_General_100_BIN2'.
        /// Use the string 'Random' while creating a database to use another random collation.
        /// </summary>
        string Collation { get; }

        /// <summary>
        /// Gets the database Compatibility level from "SqlServer/Collation" configuration entry.
        /// Defaults to the major of the Sql Server product version multiplied by 10 that seems to do the job
        /// (it is 130 for Sql Server 2016 which product version is 13.0...).
        /// </summary>
        int CompatibilityLevel { get; set; }
    }
}
