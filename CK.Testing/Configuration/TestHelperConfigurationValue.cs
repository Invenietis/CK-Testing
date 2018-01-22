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
        /// The Value can start with '{BinFolder}', {SolutionFolder} or {RepositoryFolder}: <see cref="IBasicTestHelper.BinFolder"/>,
        /// <see cref="IBasicTestHelper.SolutionFolder"/> or <see cref="IBasicTestHelper.RepositoryFolder"/> is expanded and the suffix
        /// is resolved (suffixes with \.. are handled).
        /// Placeholders {BuildConfiguration} and {TestProjectName} can appear anywhere in the Value
        /// and are replaced with <see cref="IBasicTestHelper.BuildConfiguration"/> and <see cref="IBasicTestHelper.TestProjectName"/>.
        /// </summary>
        /// <returns>The path.</returns>
        public string GetValueAsPath()
        {
            Debug.Assert( Value != null && !BasePath.IsEmpty );
            NormalizedPath raw;
            if( Value.StartsWith( "{BinFolder}", StringComparison.OrdinalIgnoreCase ) ) raw = BasicTestHelper._binFolder;
            else if( Value.StartsWith( "{SolutionFolder}", StringComparison.OrdinalIgnoreCase ) ) raw = BasicTestHelper._solutionFolder;
            else if( Value.StartsWith( "{RepositoryFolder}", StringComparison.OrdinalIgnoreCase ) ) raw = BasicTestHelper._repositoryFolder;
            if( raw.IsEmpty )
            {
                if( Path.IsPathRooted( Value ) ) raw = Path.GetFullPath( Value );
                else raw = BasePath.Combine( Value ).ResolveDots();
            }
            else raw = raw.ResolveDots();
            return raw.ToString().Replace( "{BuildConfiguration}", BasicTestHelper._buildConfiguration )
                                 .Replace( "{TestProjectName}", BasicTestHelper._testProjectName );
        }

    }

}
