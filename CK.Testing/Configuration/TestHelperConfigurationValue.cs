using CK.Core;
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
        /// The base path of this configuration value.
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
        /// <para>
        /// Placeholders {BuildConfiguration} and {TestProjectName} can appear anywhere in the Value
        /// and are replaced with <see cref="IBasicTestHelper.BuildConfiguration"/> and <see cref="IBasicTestHelper.TestProjectName"/>.
        /// </para>
        /// <para>
        /// The Value can start with {BinFolder}, {SolutionFolder} or {RepositoryFolder}: <see cref="IBasicTestHelper.BinFolder"/>,
        /// <see cref="IBasicTestHelper.SolutionFolder"/> or <see cref="IBasicTestHelper.RepositoryFolder"/> is expanded.
        /// If the Value does not start with one of this 3 paths and no { appear, the <see cref="BasePath"/> is prepended.
        /// </para>
        /// <para>
        /// If there is no '{' (ie. there is no unresolved placeholder), all '/../' are automatically resolved.
        /// If a '{'  remains in the path, the dots are not resolved: this is up to the code that will use the path to resolve the placeholders
        /// and the dots (see <see cref="NormalizedPath.ResolveDots(int, bool)"/>).
        /// </para>
        /// Note that if the value is not a valid path, result is what it is, without any warranty.
        /// </summary>
        /// <returns>The path.</returns>
        public string GetValueAsPath()
        {
            Debug.Assert( Value != null && !BasePath.IsEmptyPath );

            string v = Value.Replace( "{BuildConfiguration}", BasicTestHelper._buildConfiguration )
                            .Replace( "{TestProjectName}", BasicTestHelper._testProjectName );

            string SubPathNoRoot( string theV, int prefixLen )
            {
                if( theV.Length > prefixLen
                    && ( theV[prefixLen] == System.IO.Path.DirectorySeparatorChar
                         || theV[prefixLen] == System.IO.Path.AltDirectorySeparatorChar) )
                {
                    ++prefixLen;
                }
                return v.Substring( prefixLen );
            }

            Debug.Assert( "{BinFolder}".Length == 11 );
            Debug.Assert( "{SolutionFolder}".Length == 16 );
            Debug.Assert( "{RepositoryFolder}".Length == 18 );
            NormalizedPath raw;
            if( v.StartsWith( "{BinFolder}" ) ) raw = BasicTestHelper._binFolder.Combine( SubPathNoRoot( v, 11 ) );
            else if( v.StartsWith( "{SolutionFolder}" ) ) raw = BasicTestHelper._solutionFolder.Combine( SubPathNoRoot( v, 16 ) );
            else if( v.StartsWith( "{RepositoryFolder}" ) ) raw = BasicTestHelper._repositoryFolder.Combine( SubPathNoRoot( v, 18 ) );
            else
            {
                if( Path.IsPathRooted( v ) ) return Path.GetFullPath( v );
                if( v.Contains( '{' ) ) return v;
                raw = BasePath.Combine( v );
            }
            return raw.Path.IndexOf( '{' ) < 0 ? raw.ResolveDots() : raw;
        }

    }

}
