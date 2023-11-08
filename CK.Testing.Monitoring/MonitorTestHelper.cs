using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public sealed class MonitorTestHelper : IMonitorTestHelperCore
    {
        const int _maxCurrentLogFolderCount = 5;
        const int _maxArchivedLogFolderCount = 20;

        readonly IActivityMonitor _monitor;
        readonly ActivityMonitorConsoleClient _console;
        readonly TestHelperConfiguration _config;
        readonly IBasicTestHelper _basic;
        static bool _logToCKMon;
        static bool _logToText;

        static readonly CKTrait _loadConflictTag = ActivityMonitor.Tags.Register( "AssemblyLoadConflict" );
        static int _loadConflictCount = 0;

        internal MonitorTestHelper( TestHelperConfiguration config, IBasicTestHelper basic )
        {
            _config = config;
            _basic = basic;

            // Defensive programming: even if more than one MonitorTestHelper is instantiated, the GrandOutput and the related
            // configurations must be initialized once.
            basic.OnlyOnce( () =>
            {
                _logToCKMon = _config.DeclareBoolean( "Monitor/LogToCKMon",
                                                      true,
                                                      $"Emits binary logs to {_basic.LogFolder}/CKMon folder.",
                                                      null,
                                                      "Monitor/LogToBinFile",
                                                      "Monitor/LogToBinFiles" ).Value;

                _logToText = _config.DeclareBoolean( "Monitor/LogToText",
                                                      true,
                                                      $"Emits text logs to {_basic.LogFolder}/Text folder.",
                                                      null,
                                                      "Monitor/LogToTextFile",
                                                      "Monitor/LogToTextFiles" ).Value;

                // LogLevel defaults to Debug while testing.
                string logLevel = _config.Declare( "Monitor/LogLevel",
                                                   "Debug",
                                                   "Initializes the static ActivityMonitor.DefaultFilter value.",
                                                   () => ActivityMonitor.DefaultFilter.ToString() ).Value;
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
            LogToConsole = _config.DeclareBoolean( "Monitor/LogToConsole",
                                                   false,
                                                   "Writes the text logs to the console.",
                                                   () => LogToConsole.ToString() ).Value;
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


        sealed class Resumer
        {
            internal readonly TaskCompletionSource _tcs;
            readonly Timer _timer;
            readonly Func<bool, bool> _resume;
            bool _reentrant;

            internal Resumer( Func<bool, bool> resumeF )
            {
                _timer = new Timer( OnTimer, null, 1000, 1000 );
                _tcs = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                _resume = resumeF;
            }

            void OnTimer( object? _ )
            {
                if( _reentrant ) return;
                _reentrant = true;
                if( _resume( false ) )
                {
                    _tcs.SetResult();
                    _timer.Dispose();
                }
                _reentrant = false;
            }
        }

        Task IMonitorTestHelperCore.SuspendAsync( Func<bool, bool> resume,
                                                  string? testName,
                                                  int lineNumber,
                                                  string? fileName )
        {
            Throw.CheckNotNullArgument( resume );
            if( !Debugger.IsAttached )
            {
                _monitor.Warn( $"TestHelper.SuspendAsync called from '{testName}' method while no debugger is attached. Ignoring it.", lineNumber, fileName );
                return Task.CompletedTask;
            }
            _monitor.Info( $"TestHelper.SuspendAsync called from '{testName}' method.", lineNumber, fileName );
            return new Resumer( resume )._tcs.Task;
        }

        /// <summary>
        /// Gets the <see cref="IMonitorTestHelper"/> mixin.
        /// </summary>
        public static IMonitorTestHelper TestHelper => TestHelperResolver.Default.Resolve<IMonitorTestHelper>();

    }
}
