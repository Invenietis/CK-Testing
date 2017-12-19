using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CK.Core;
using CK.Monitoring;
using CK.Monitoring.Handlers;
using CK.Text;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="Monitoring.IMonitorTestHelperCore"/>
    /// and easyt to use accessor to the <see cref="IMonitorTestHelper"/> mixin.
    /// </summary>
    public class MonitorTestHelper : Monitoring.IMonitorTestHelperCore
    {
        readonly IActivityMonitor _monitor;
        readonly ActivityMonitorConsoleClient _console;
        readonly ITestHelperConfiguration _config;
        static int _onlyOnce;

        public MonitorTestHelper( ITestHelperConfiguration config, IBasicTestHelper basic )
        {
            _config = config;
            GlobalCKMonFiles = _config.GetBoolean( "Monitor/GlobalCKMonFiles" ) ?? false;
            GlobalTextFiles = _config.GetBoolean( "Monitor/GlobalTextFiles" ) ?? false;

            if( Interlocked.Increment( ref _onlyOnce ) == 1 )
            {
                LogFile.RootLogPath = basic.LogFolder;
                var conf = new GrandOutputConfiguration();
                if( GlobalCKMonFiles )
                {
                    var binConf = new BinaryFileConfiguration
                    {
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "CKMon/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( binConf );
                }
                if( GlobalTextFiles )
                {
                    var txtConf = new TextFileConfiguration
                    {
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "Text/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( txtConf );
                }
                GrandOutput.EnsureActiveDefault( conf );
            }
            _monitor = new ActivityMonitor();
            _console = new ActivityMonitorConsoleClient();
            LogToConsole = _config.GetBoolean( "Monitor/LogToConsole" ) ?? false;
            basic.OnCleanupFolder += OnCleanupFolder;
        }

        void OnCleanupFolder( object sender, CleanupFolderEventArgs e )
        {
            _monitor.Info( $"Folder '{e.Folder}' has been cleaned up." );
        }

        public IActivityMonitor Monitor => _monitor;

        public bool LogToConsole
       {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( _monitor.Output.Clients.Contains( _console ) != value )
                {
                    if( value )
                    {
                        _monitor.Output.RegisterClient( _console );
                        _monitor.Info( "Switching console log ON." );
                    }
                    else
                    {
                        _monitor.Info( "Switching console log OFF." );
                        _monitor.Output.UnregisterClient( _console );
                    }
                }
            }
        }

        public bool GlobalCKMonFiles { get; }

        public bool GlobalTextFiles { get; }

        /// <summary>
        /// Gets the <see cref="IMonitorTestHelper"/> mixin.
        /// </summary>
        public static IMonitorTestHelper TestHelper => TestHelperResolver.Default.Resolve<IMonitorTestHelper>();

    }
}
