using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CK.Core;
using CK.Monitoring;
using CK.Monitoring.Handlers;
using CK.Testing.Monitoring;

namespace CK.Testing
{

    /// <summary>
    /// Provides default implementation of <see cref="IMonitorTestHelperCore"/>
    /// and easy to use accessor to the <see cref="IMonitorTestHelper"/> mixin.
    /// </summary>
    public class MonitorTestHelper : IMonitorTestHelperCore
    {
        const int _maxCurrentLogFolderCount = 5;
        const int _maxArchivedLogFolderCount = 20;

        readonly IActivityMonitor _monitor;
        readonly ActivityMonitorConsoleClient _console;
        readonly TestHelperConfiguration _config;
        readonly IBasicTestHelper _basic;
        static readonly CKTrait _loadConflictTag = ActivityMonitor.Tags.Register( "AssemblyLoadConflict" );
        static int _loadConflictCount = 0;
        static bool _logToCKMon;
        static bool _logToText;

        internal MonitorTestHelper( TestHelperConfiguration config, IBasicTestHelper basic )
        {
            _config = config;
            _basic = basic;

            basic.OnlyOnce( () =>
            {
                _logToCKMon = _config.GetBoolean( "Monitor/LogToCKMon" )
                                        ?? _config.GetBoolean( "Monitor/LogToBinFile" )
                                        ?? _config.GetBoolean( "Monitor/LogToBinFiles" )
                                        ?? true;
                _logToText = _config.GetBoolean( "Monitor/LogToText" )
                                        ?? _config.GetBoolean( "Monitor/LogToTextFile" )
                                        ?? _config.GetBoolean( "Monitor/LogToTextFiles" )
                                        ?? false;

                // LogLevel defaults to Debug while testing.
                string logLevel = _config.Get( "Monitor/LogLevel" );
                if( logLevel == null )
                {
                    ActivityMonitor.DefaultFilter = LogFilter.Debug;
                }
                else
                {
                    var lf = LogFilter.Parse( logLevel );
                    ActivityMonitor.DefaultFilter = lf;
                }
                LogFile.RootLogPath = basic.LogFolder;
                var conf = new GrandOutputConfiguration();
                if( _logToCKMon )
                {
                    var binConf = new BinaryFileConfiguration
                    {
                        UseGzipCompression = true,
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "CKMon/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( binConf );
                }
                if( _logToText )
                {
                    var txtConf = new TextFileConfiguration
                    {
                        Path = FileUtil.CreateUniqueTimedFolder( LogFile.RootLogPath + "Text/", null, DateTime.UtcNow )
                    };
                    conf.AddHandler( txtConf );
                }
                GrandOutput.EnsureActiveDefault( conf, clearExistingTraceListeners: false );
                var monitorListener = Trace.Listeners.OfType<MonitorTraceListener>().FirstOrDefault( m => m.GrandOutput == GrandOutput.Default );
                // If our standard MonitorTraceListener has been injected by the GrandOuput, then we remove the StaticBasicTestHelper.SafeTraceListener
                // that always throws Exceptions and never calls FailFast.
                // (Defensive programming) There is no real reason for this listener to not be in the listeners, but it can be.
                if( monitorListener != null )
                {
                    Trace.Listeners.Remove( "CK.Testing.SafeTraceListener" );
                }
            } );
            _monitor = new ActivityMonitor( "MonitorTestHelper" );
            _console = new ActivityMonitorConsoleClient();
            LogToConsole = _config.GetBoolean( "Monitor/LogToConsole" ) ?? false;
            basic.OnCleanupFolder += OnCleanupFolder;
            basic.OnlyOnce( () =>
            {
                var basePath = LogFile.RootLogPath + "Text" + FileUtil.DirectorySeparatorString;
                if( Directory.Exists( basePath ) )
                {
                    CleanupTimedFolders( _monitor, _basic, basePath, _maxCurrentLogFolderCount, _maxArchivedLogFolderCount );
                }
                basePath = LogFile.RootLogPath + "CKMon" + FileUtil.DirectorySeparatorString;
                if( Directory.Exists( basePath ) )
                {
                    CleanupTimedFolders( _monitor, _basic, basePath, _maxCurrentLogFolderCount, _maxArchivedLogFolderCount );
                }
            } );
        }

        static void CleanupTimedFolders( IActivityMonitor m, IBasicTestHelper basic, string basePath, int maxCurrentLogFolderCount, int maxArchivedLogFolderCount )
        {
            Debug.Assert( basePath.EndsWith( FileUtil.DirectorySeparatorString ) );
            // Note: The comparer is a reverse comparer. The most RECENT timed folder is the FIRST.
            GetTimedFolders( basePath, out SortedDictionary<DateTime, string> timedFolders, out string archivePath, false );
            if( timedFolders.Count > maxCurrentLogFolderCount )
            {
                int retryCount = 5;
                retry:
                try
                {
                    if( archivePath == null )
                    {
                        m.Trace( "Creating Archive folder." );
                        Directory.CreateDirectory( archivePath = basePath + "Archive" );
                    }
                    foreach( var old in timedFolders.Values.Skip( maxCurrentLogFolderCount ) )
                    {
                        var fName = Path.GetFileName( old );
                        m.Trace( $"Moving '{fName}' folder into Archive folder." );
                        var target = Path.Combine( archivePath, fName );
                        if( Directory.Exists( target ) ) target += '-' + Guid.NewGuid().ToString();
                        Directory.Move( old, target );
                    }
                    GetTimedFolders( archivePath, out timedFolders, out _, true );
                    foreach( var tooOld in timedFolders.Values.Skip( maxArchivedLogFolderCount ) )
                    {
                        basic.CleanupFolder( tooOld, false );
                    }
                }
                catch( Exception ex )
                {
                    if( --retryCount < 0 )
                    {
                        m.Error( $"Aborting Log's cleanup of timed folders in '{basePath}' after 5 retries.", ex );
                        return;
                    }
                    m.Warn( $"Log's cleanup of timed folders in '{basePath}' failed. Retrying in {retryCount * 100} ms.", ex );
                    Thread.Sleep( retryCount * 100 );
                    goto retry;
                }
            }
        }

        static void GetTimedFolders( string folder, out SortedDictionary<DateTime, string> timedFolders, out string archivePath, bool allowNameSuffix )
        {
            timedFolders = new SortedDictionary<DateTime, string>( Comparer<DateTime>.Create( ( x, y ) => y.CompareTo( x ) ) );
            archivePath = null;
            foreach( var d in Directory.EnumerateDirectories( folder ) )
            {
                var name = d.Substring( folder.Length );
                if( name == "Archive" ) archivePath = d + FileUtil.DirectorySeparatorString;
                else
                {
                    var n = name.AsSpan();
                    if( FileUtil.TryMatchFileNameUniqueTimeUtcFormat( ref n, out DateTime date ) && (allowNameSuffix || n.IsEmpty) )
                    {
                        // Take no risk: ignore (highly unlikely to happen) duplicates. 
                        timedFolders[date] = d;
                    }
                }
            }
        }

        void OnCleanupFolder( object sender, CleanupFolderEventArgs e )
        {
            _monitor.Info( $"Folder '{e.Folder}' has been cleaned up." );
        }

        IActivityMonitor IMonitorTestHelperCore.Monitor
        {
            [DebuggerStepThrough]
            get => _monitor;
        }

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

        bool IMonitorTestHelperCore.LogToConsole
        {
            get => LogToConsole;
            set => LogToConsole = value;
        }

        bool IMonitorTestHelperCore.LogToCKMon => _logToCKMon;

        bool IMonitorTestHelperCore.LogToText => _logToText;

        IDisposable IMonitorTestHelperCore.TemporaryEnsureConsoleMonitor()
        {
            bool prev = LogToConsole;
            LogToConsole = true;
            return Util.CreateDisposableAction( () => LogToConsole = prev );
        }

        void IMonitorTestHelperCore.WithWeakAssemblyResolver( Action action ) => DoWithWeakAssemblyResolver( action );

        static void DrainAssemblyLoadConflicts( IActivityMonitor m )
        {
            AssemblyLoadConflict[] currents = WeakAssemblyNameResolver.GetAssemblyConflicts();
            int prev = Interlocked.Exchange( ref _loadConflictCount, currents.Length );
            int delta = currents.Length - prev;
            if( delta > 0 )
            {
                using( m.OpenWarn( $"{delta} assembly load conflicts occurred:" ) )
                {
                    while( prev < currents.Length ) m.Warn( _loadConflictTag, currents[prev++].ToString() );
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
            T result = default;
            DoWithWeakAssemblyResolver( () => result = action() );
            return result;
        }

        void ITestHelperResolvedCallback.OnTestHelperGraphResolved( object finalMixin )
        {
            DrainAssemblyLoadConflicts( _monitor );
        }

        /// <summary>
        /// Gets the <see cref="IMonitorTestHelper"/> mixin.
        /// </summary>
        public static IMonitorTestHelper TestHelper => TestHelperResolver.Default.Resolve<IMonitorTestHelper>();

    }
}
