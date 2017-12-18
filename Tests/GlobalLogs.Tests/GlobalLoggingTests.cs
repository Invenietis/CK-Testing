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
            GrandOutput.Default.Dispose();
            Directory.EnumerateFiles( TestHelper.LogFolder, "*.txt", SearchOption.AllDirectories )
                        .Select( f => File.ReadAllText( f ) )
                        .Count( text => text.Contains( secret ) )
                        .Should().Be( 1 );
        }
    }
}
