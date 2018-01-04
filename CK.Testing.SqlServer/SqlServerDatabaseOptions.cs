using CK.Core;
using CK.Testing.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{
    /// <summary>
    /// Encapsulates database options.
    /// </summary>
    public class SqlServerDatabaseOptions : ISqlServerDatabaseOptions
    {
        /// <summary>
        /// Initializes a new <see cref="SqlServerDatabaseOptions"/>.
        /// </summary>
        public SqlServerDatabaseOptions()
        {
            Collation = "Latin1_General_100_BIN2";
        }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database collation.
        /// Defaults to 'Latin1_General_100_BIN2'.
        /// Use 'Random' while creating a database to use another random collation.
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// Gets or sets the compatibility level.
        /// Defaults to 0: the maximal compatibility level is used. 
        /// </summary>
        public int CompatibilityLevel { get; set; }

        /// <summary>
        /// Overridden to return the Database name, collation and compatibility level.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString()
        {
            if( CompatibilityLevel != 0 )
            {
                return $"Database:{DatabaseName}, Collation:{Collation}, CompatibilityLevel:{CompatibilityLevel}";
            }
            return $"Database:{DatabaseName}, Collation:{Collation}";
        }
    }
}
