using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{
    /// <summary>
    /// Encapsulates a configuration value and the base path where it has been defined.
    /// </summary>
    public struct TestHelperConfigurationValue
    {
        /// <summary>
        /// The base path of this confivguration value.
        /// Defaults to <see cref="IBasicTestHelper.TestProjectFolder"/> (for environment setting for instance).
        /// </summary>
        public readonly NormalizedPath BasePath;

        /// <summary>
        /// The value: never null but may be empty.
        /// </summary>
        public readonly string Value;

        internal TestHelperConfigurationValue( NormalizedPath basePath, string value )
        {
            BasePath = basePath;
            Value = value;
        }

        /// <summary>
        /// Gets the <see cref="Value"/> as an absolute path or relative to <see cref="BasePath"/>.
        /// If the value is not a valid path, result is what it is, without any warranty.
        /// </summary>
        /// <returns>The path.</returns>
        public string GetValueAsPath()
        {
            Debug.Assert( Value != null && !BasePath.IsEmpty );
            var p = Path.IsPathRooted( Value )
                    ? Path.GetFullPath( Value )
                    : BasePath.Combine( Value ).ResolveDots().ToString();
            return p.Replace( "{BuildConfiguration}", BasicTestHelper._buildConfiguration )
                    .Replace( "{TestProjectName}", BasicTestHelper._testProjectName );
        }
    }

}
