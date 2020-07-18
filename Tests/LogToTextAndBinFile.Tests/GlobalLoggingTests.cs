using System;
using NUnit.Framework;
using CK.Core;
using CK.Monitoring;
using FluentAssertions;
using System.IO;
using System.Linq;
using System.Text;

using static CK.Testing.MonitorTestHelper;

namespace GlobalLogs.Tests
{
    [TestFixture]
    public class GlobalLoggingTests
    {
        [Test]
        public void ckmon_files_and_text_files_are_produced()
        {
            var secret = Guid.NewGuid().ToString();
            var binSecret = Encoding.UTF8.GetBytes( secret );

            TestHelper.Monitor.Info( $"This will appear in ckmon, text files and the console: {secret}" );
            GrandOutput.Default.Should().NotBeNull();
            GrandOutput.Default.Dispose();
            Directory.EnumerateFiles( TestHelper.LogFolder, "*.log", SearchOption.AllDirectories )
                        .Select( f => File.ReadAllText( f ) )
                        .Count( text => text.Contains( secret ) )
                        .Should().Be( 1 );
            // ckmon files are now gzipped by default.
            int count = 0;
            foreach( var fName in Directory.EnumerateFiles( TestHelper.LogFolder, "*.ckmon", SearchOption.AllDirectories ) )
            {
                using( var input = LogReader.Open( fName ) )
                {
                    while( input.MoveNext() )
                    {
                        if( input.Current.LogType != LogEntryType.CloseGroup
                            && input.Current.Text.Contains( secret ) )
                        {
                            ++count;
                        }
                    }
                }
            }
            count.Should().Be( 1 );
            //
            TestHelper.WithWeakAssemblyResolver( () => TestHelper.Monitor.Info( "From WeakAssemblyResolver." ) );
            TestHelper.Monitor.Info( $"From WeakAssemblyResolver: {TestHelper.WithWeakAssemblyResolver( () => 3 )}" );
        }

        [Test]
        public void TestHelper_properties_are_available()
        {
            var w = new StringWriter();
            DumpProperties( w, "> ", TestHelper );
            var text = w.ToString();
            text.Should().Contain( "LogToBinFile = True" );
            TestHelper.Monitor.Info( text );
        }

        void DumpProperties( TextWriter w, string prefix, object o )
        {
            foreach( var p in o.GetType().GetProperties() )
            {
                if( p.PropertyType.IsValueType || p.PropertyType == typeof( string ) )
                {
                    w.WriteLine( $"{prefix}{p.Name} = {p.GetValue( o ) ?? "<null>"}" );
                }
                else if( typeof( System.Collections.IEnumerable ).IsAssignableFrom( p.PropertyType ) )
                {
                    w.Write( $"{prefix}{p.Name} = " );
                    var items = p.GetValue( o ) as System.Collections.IEnumerable;
                    if( items == null ) w.WriteLine( "<null>" );
                    else
                    {
                        var wPrefix = new String( ' ', prefix.Length + p.Name.Length + 3 );
                        w.WriteLine( $"[" );
                        foreach( var item in items )
                        {
                            Type t = item.GetType();
                            if( t.IsValueType || t == typeof( string ) )
                            {
                                w.Write( wPrefix );
                                w.WriteLine( item );
                            }
                        }
                        w.WriteLine( $"{wPrefix}]" );
                    }
                }
            }
        }

    }
}
