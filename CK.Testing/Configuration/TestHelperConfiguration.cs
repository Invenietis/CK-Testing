using CK.Core;
using CK.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        readonly Dictionary<NormalizedPath, string> _config;
        readonly SimpleServiceContainer _container;

        /// <summary>
        /// Initializes a new <see cref="TestHelperConfiguration"/>.
        /// </summary>
        public TestHelperConfiguration()
        {
            _config = new Dictionary<NormalizedPath, string>();
            _container = new SimpleServiceContainer();
            _container.Add<ITestHelperConfiguration>( this );
            var root = new NormalizedPath( AppContext.BaseDirectory );
            SimpleReadFromAppSetting( root.FindClosestFile( "TestHelper.config", "Test.config", "App.config" ) );
            SimpleReadFromEnvironment();
        }

        /// <summary>
        /// Gets the configuration value associated to a key with a lookup up to the root of the configuration.
        /// </summary>
        /// <param name="key">The path of the key to find.</param>
        /// <param name="defaultValue">The default value when not found.</param>
        /// <returns>The configured value or the default value.</returns>
        public string Get( NormalizedPath key, string defaultValue = null )
        {
            while( !key.IsEmpty )
            {
                if( _config.TryGetValue( key, out string result ) ) return result;
                if( key.Parts.Count == 1 ) break;
                key = key.RemovePart( key.Parts.Count - 2 );
            }
            return defaultValue;
        }

        void Add( string key, string value )
        {
            _config[key.Replace( "::", FileUtil.DirectorySeparatorString )] = value;
        }

        void SimpleReadFromAppSetting( NormalizedPath appConfigFile )
        {
            if( !appConfigFile.IsEmpty )
            {
                XDocument doc = XDocument.Load( appConfigFile );
                foreach( var e in doc.Root.Descendants( "appSettings" ).Elements( "add" ) )
                {
                    Add( e.AttributeRequired( "key" ).Value, e.AttributeRequired( "value" ).Value );
                }
            }
        }

        void SimpleReadFromEnvironment()
        {
            var env = Environment.GetEnvironmentVariables()
                        .Cast<DictionaryEntry>()
                        .Select( e => Tuple.Create((string)e.Key, (string)e.Value) )
                        .Where( t => t.Item1.StartsWith( "TestHelper::", StringComparison.OrdinalIgnoreCase ) );

            foreach( var kv in env ) Add( kv.Item1, kv.Item2 );
        }

        /// <summary>
        /// Gets a singleton that exposes the global configuration.
        /// </summary>
        public static ITestHelperConfiguration Default { get; } = new TestHelperConfiguration();
    }
}
