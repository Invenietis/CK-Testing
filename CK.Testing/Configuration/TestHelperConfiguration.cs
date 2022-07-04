using CK.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CK.Testing
{

    /// <summary>
    /// Simple configuration that reads its content from all "*.TestHelper.config" (in lexicographical 
    /// order) and then "TestHelper.config" files in folders from <see cref="IBasicTestHelper.SolutionFolder"/> 
    /// down to the current execution path.
    /// Once all these files are applied, environment variables that start with "TestHelper::" prefix are applied.
    /// </summary>
    public sealed class TestHelperConfiguration
    {
        readonly Dictionary<NormalizedPath, TestHelperConfigurationValue> _config;
        readonly SimpleServiceContainer _container;
        internal IBasicTestHelper? _basic;

        /// <summary>
        /// Initializes a new <see cref="TestHelperConfiguration"/>.
        /// </summary>
        public TestHelperConfiguration()
        {
            _config = new Dictionary<NormalizedPath, TestHelperConfigurationValue>();
            _container = new SimpleServiceContainer();
            ApplyFilesConfig( BasicTestHelper._binFolder );
            SimpleReadFromEnvironment();
        }

        /// <summary>
        /// Gets the configuration value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <returns>The configured value.</returns>
        public TestHelperConfigurationValue? GetConfigValue( NormalizedPath key )
        {
            while( key.HasParts )
            {
                if( _config.TryGetValue( key, out var result ) ) return result;
                if( key.Parts.Count == 1 ) break;
                key = key.RemovePart( key.Parts.Count - 2 );
            }
            return null;
        }

        /// <summary>
        /// Gets the configuration string value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <param name="defaultValue">The default value when not found.</param>
        /// <returns>The configured value or the default value.</returns>
        [return: NotNullIfNotNull( "defaultValue" )]
        public string? Get( NormalizedPath key, string? defaultValue = null ) => GetConfigValue( key )?.Value ?? defaultValue;

        /// <summary>
        /// Gets the configuration value associated to a key as a file or folder path
        /// (see <see cref="TestHelperConfigurationValue.GetValueAsPath"/>).
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <returns>The configured path or null.</returns>
        public NormalizedPath? GetPath( NormalizedPath key )
        {
            var v = GetConfigValue( key );
            return v.HasValue ? new NormalizedPath( v.Value.GetValueAsPath() ) : (NormalizedPath?)null;
        }

        /// <summary>
        /// Gets all the configuration values defined.
        /// </summary>
        public IEnumerable<KeyValuePair<NormalizedPath, TestHelperConfigurationValue>> ConfigurationValues => _config;

        void SetEntry( string key, NormalizedPath basePath, string value )
        {
            NormalizedPath k = NormalizeKey( key );
            if( value == null ) _config.Remove( k );
            else
            {
                if( basePath.IsEmptyPath ) basePath = BasicTestHelper._testProjectFolder;
                _config[k] = new TestHelperConfigurationValue( this, basePath, value );
            }
        }

        static NormalizedPath NormalizeKey( string key )
        {
            return key.Replace( "::", NormalizedPath.DirectorySeparatorString )
                      .Replace( "__", NormalizedPath.DirectorySeparatorString );
        }

        void ApplyFilesConfig( NormalizedPath folder )
        {
            if( folder.Parts.Count > BasicTestHelper._solutionFolder.Parts.Count )
            {
                ApplyFilesConfig( folder.RemoveLastPart() );
            }
            foreach( var f in Directory.EnumerateFiles( folder, "*.TestHelper.config" ).OrderBy( n => n ) )
            {
                SimpleReadFromAppSetting( f );
            }
            var file = folder.AppendPart( "TestHelper.config" );
            if( File.Exists( file ) )
            {
                SimpleReadFromAppSetting( file );
            }
        }

        void SimpleReadFromAppSetting( NormalizedPath appConfigFile )
        {
            var basePath = appConfigFile.RemoveLastPart();
            XDocument doc = XDocument.Load( appConfigFile );
            Debug.Assert( doc.Root != null );
            foreach( var e in doc.Root.Elements( "appSettings" ).Elements() )
            {
                if( e.Name.LocalName == "add" )
                {
                    SetEntry( e.AttributeRequired( "key" ).Value, basePath, e.AttributeRequired( "value" ).Value );
                }
                else if( e.Name.LocalName == "remove" )
                {
                    var k = NormalizeKey( e.AttributeRequired( "key" ).Value );
                    while( k.HasParts )
                    {
                        _config.Remove( k );
                        k = k.RemoveFirstPart();
                    }
                }
                else if( e.Name.LocalName == "clear" )
                {
                    _config.Clear();
                }
                else throw new Exception( $"Only add, remove and clear child elements of appSettings are supported in {appConfigFile}." );
            }
        }

        void SimpleReadFromEnvironment()
        {
            const string prefix1 = "TestHelper::";
            const string prefix2 = "TestHelper__";
            Debug.Assert( prefix1.Length == 12 && prefix2.Length == 12 );
            var env = Environment.GetEnvironmentVariables()
                        .Cast<DictionaryEntry>()
                        .Select( e => new KeyValuePair<string,string>( (string)e.Key, (string)e.Value! ) )
                        .Where( t => t.Key.StartsWith( prefix1, StringComparison.OrdinalIgnoreCase )
                                     || t.Key.StartsWith( prefix2, StringComparison.OrdinalIgnoreCase ) )
                        .Select( t => new KeyValuePair<string, string>( t.Key.Substring( 12 ), t.Value ) );

            foreach( var kv in env ) SetEntry( kv.Key, BasicTestHelper._testProjectFolder, kv.Value );
        }

        /// <summary>
        /// Gets a singleton that exposes the global configuration.
        /// </summary>
        public static TestHelperConfiguration Default { get; } = new TestHelperConfiguration();
    }
}
