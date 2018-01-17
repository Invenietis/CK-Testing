using CK.Core;
using CK.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CK.Testing
{

    /// <summary>
    /// Simple configuration that reads its content from the first "TestHelper.config" "Test.config" or "App.Config"
    /// in current execution path and parent paths and from environment variables that start
    /// with "TestHelper::" prefix.
    /// </summary>
    public class TestHelperConfiguration : ITestHelperConfiguration
    {
        readonly Dictionary<NormalizedPath, TestHelperConfigurationValue> _config;
        readonly SimpleServiceContainer _container;

        /// <summary>
        /// Initializes a new <see cref="TestHelperConfiguration"/>.
        /// </summary>
        public TestHelperConfiguration()
        {
            _config = new Dictionary<NormalizedPath, TestHelperConfigurationValue>();
            _container = new SimpleServiceContainer();
            _container.Add<ITestHelperConfiguration>( this );
            ApplyConfig( BasicTestHelper._binFolder );
            SimpleReadFromEnvironment();
        }

        /// <summary>
        /// Gets the configuration value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <returns>The configured value.</returns>
        public TestHelperConfigurationValue? GetConfigValue( NormalizedPath key )
        {
            while( !key.IsEmpty )
            {
                if( _config.TryGetValue( key, out var result ) ) return result;
                if( key.Parts.Count == 1 ) break;
                key = key.RemovePart( key.Parts.Count - 2 );
            }
            return null;
        }

        /// <summary>
        /// Gets the configuration value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <param name="defaultValue">The default value when not found.</param>
        /// <returns>The configured value or the default value.</returns>
        public string Get( NormalizedPath key, string defaultValue = null ) => GetConfigValue( key )?.Value ?? defaultValue;

        /// <summary>
        /// Gets the configuration value associated to a key as a file or folder path
        /// (see <see cref="TestHelperConfigurationValue.GetValueAsPath"/>).
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <returns>The configured path or null.</returns>
        public NormalizedPath? GetPath( NormalizedPath key )
        {
            var v = GetConfigValue( key );
            return v.HasValue ? v.Value.GetValueAsPath() : null;
        }

        /// <summary>
        /// Gets all the configuration values defined.
        /// </summary>
        public IEnumerable<TestHelperConfigurationValue> ConfigurationValues => _config.Values;

        void SetEntry( string key, NormalizedPath basePath, string value )
        {
            NormalizedPath k = key.Replace( "::", FileUtil.DirectorySeparatorString );
            if( value == null ) _config.Remove( k );
            else
            {
                if( basePath.IsEmpty ) basePath = BasicTestHelper._testProjectFolder;
                _config[k] = new TestHelperConfigurationValue( basePath, value );
            }
        }

        void ApplyConfig( NormalizedPath folder )
        {
            if( folder.Parts.Count > BasicTestHelper._solutionFolder.Parts.Count )
            {
                ApplyConfig( folder.RemoveLastPart() );
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
            foreach( var e in doc.Root.Descendants( "appSettings" ).Elements( "add" ) )
            {
                SetEntry( e.AttributeRequired( "key" ).Value, basePath, e.AttributeRequired( "value" ).Value );
            }
        }

        void SimpleReadFromEnvironment()
        {
            const string prefix = "TestHelper::";
            Debug.Assert( prefix.Length == 12 );
            var env = Environment.GetEnvironmentVariables()
                        .Cast<DictionaryEntry>()
                        .Select( e => Tuple.Create( (string)e.Key, (string)e.Value ) )
                        .Where( t => t.Item1.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) )
                        .Select( t => Tuple.Create( t.Item1.Substring( 12 ), t.Item2 ) );

            foreach( var kv in env ) SetEntry( kv.Item1, BasicTestHelper._testProjectFolder, kv.Item2 );
        }

        /// <summary>
        /// Gets a singleton that exposes the global configuration.
        /// </summary>
        public static ITestHelperConfiguration Default { get; } = new TestHelperConfiguration();
    }
}
