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
using CK.Testing.Monitoring;
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
        readonly IBasicTestHelper _basic;
        static readonly CKTrait _loadConflictTag = ActivityMonitor.Tags.Register( "AssemblyLoadConflict" );
        static int _loadConflictCount = 0;
        static bool _globalCKMonFiles;
        static bool _globalTextFiles;

        internal MonitorTestHelper( ITestHelperConfiguration config, IBasicTestHelper basic )
        {
            _config = config;
            _basic = basic;

            basic.OnlyOnce( () =>
            {
                _globalCKMonFiles = _config.GetBoolean( "Monitor/GlobalCKMonFiles" ) ?? false;
                _globalTextFiles = _config.GetBoolean( "Monitor/GlobalTextFiles" ) ?? false;
                string logLevel = _config.Get( "Monitor/LogLevel" );
                if( logLevel != null )
                {
                    var lf = LogFilter.Parse( logLevel );
                    ActivityMonitor.DefaultFilter = lf;
                }
                LogFile.RootLogPath = basic.LogFolder;
                var conf = new GrandOutputConfiguration();
                if( _globalCKMonFiles )
                {
                    var binConf = new BinaryFileConfiguration
                    {
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "CKMon/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( binConf );
                }
                if( _globalTextFiles )
                {
                    var txtConf = new TextFileConfiguration
                    {
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "Text/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( txtConf );
                }
                if( conf.Handlers.Count > 0 )
                {
                    GrandOutput.EnsureActiveDefault( conf );
                }
            } );
            _monitor = new ActivityMonitor();
            _console = new ActivityMonitorConsoleClient();
            LogToConsole = _config.GetBoolean( "Monitor/LogToConsole" ) ?? false;
            basic.OnCleanupFolder += OnCleanupFolder;
        }

        void OnCleanupFolder( object sender, CleanupFolderEventArgs e )
        {
            _monitor.Info( $"Folder '{e.Folder}' has been cleaned up." );
        }

        IActivityMonitor Monitoring.IMonitorTestHelperCore.Monitor => _monitor;

        bool LogToConsole
        {
            get => _monitor.Output.Clients.Contains( _console );
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

        bool Monitoring.IMonitorTestHelperCore.LogToConsole
        {
            get => LogToConsole;
            set => LogToConsole = value;
        }

        bool Monitoring.IMonitorTestHelperCore.GlobalCKMonFiles { get; }

        bool Monitoring.IMonitorTestHelperCore.GlobalTextFiles { get; }

        IDisposable Monitoring.IMonitorTestHelperCore.TemporaryEnsureConsoleMonitor()
        {
            bool prev = LogToConsole;
            LogToConsole = true;
            return Util.CreateDisposableAction( () => LogToConsole = prev );
        }

        void Monitoring.IMonitorTestHelperCore.WithWeakAssemblyResolver( Action action ) => DoWithWeakAssemblyResolver( action );

        static void DrainAssemblyLoadConflicts( IActivityMonitor m )
        {
            AssemblyLoadConflict[] currents = WeakAssemblyNameResolver.GetAssemblyConflicts();
            int prev = Interlocked.Exchange( ref _loadConflictCount, currents.Length );
            int delta = currents.Length - prev;
            if( delta > 0 )
            {
                using( m.OpenWarn( $"{delta} assembly load conflicts occurred:" ) )
                {
                    while( prev < currents.Length ) m.Warn( currents[prev++].ToString(), _loadConflictTag );
                }
            }
        }

        void DoWithWeakAssemblyResolver( Action action )
        {
            try
            {
                using( WeakAssemblyNameResolver.TemporaryInstall() )
                {
                    action();
                }
            }
            catch( Exception ex )
            {
                _monitor.Error( ex );
                throw;
            }
            finally
            {
                DrainAssemblyLoadConflicts( _monitor );
            }
        }

        T Monitoring.IMonitorTestHelperCore.WithWeakAssemblyResolver<T>( Func<T> action )
        {
            T result = default(T);
            DoWithWeakAssemblyResolver( () => result = action() );
            return result;
        }

        void ITestHelperResolvedCallback.OnTestHelperGraphResolved()
        {
            DrainAssemblyLoadConflicts( _monitor );
        }

        /// <summary>
        /// Gets the <see cref="IMonitorTestHelper"/> mixin.
        /// </summary>
        public static IMonitorTestHelper TestHelper => TestHelperResolver.Default.Resolve<IMonitorTestHelper>();

    }
}
