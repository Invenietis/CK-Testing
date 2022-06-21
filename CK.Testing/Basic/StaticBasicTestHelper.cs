using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using CK.Core;

namespace CK.Testing
{

    /// <summary>
    /// Static part of the implementation of <see cref="BasicTestHelper"/>.
    /// </summary>
    public class StaticBasicTestHelper
    {
        static readonly string[] _allowedConfigurations = new[] { "Debug", "Release" };
        internal static readonly NormalizedPath _binFolder;
        internal static readonly string _buildConfiguration;
        internal static readonly NormalizedPath _testProjectFolder;
        internal static readonly string _testProjectName;
        internal static readonly NormalizedPath _repositoryFolder;
        internal static readonly NormalizedPath _solutionFolder;
        internal static readonly NormalizedPath _logFolder;
        internal static readonly bool _isTestHost;
        internal static readonly HashSet<string> _onlyOnce;
        internal static readonly ExceptionDispatchInfo _initializationError;

        /// <summary>
        /// This listener is removed by CK.Testing.MonitorTestHelper because the MonitorTraceListener
        /// that throws MonitoringFailFastException is injected and does the job.
        /// The key used to remove this listener is its name: "CK.Testing.SafeTraceListener" that
        /// MUST NOT be changed since this magic string is used by the MonitorTestHelper.
        /// </summary>
        class SafeTraceListener : System.Diagnostics.DefaultTraceListener
        {
            const string _messagePrefix = "Assertion Failed: ";

            public SafeTraceListener()
            {
                Name = "CK.Testing.SafeTraceListener";
            }

            public override void Fail( string? message, string? detailMessage ) => throw new Exception( _messagePrefix + message + " - Detail: " + detailMessage );
            public override void Fail( string? message ) => throw new Exception( _messagePrefix + message );
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        static StaticBasicTestHelper()
        {
            _onlyOnce = new HashSet<string>();
            try
            {
                // Conservative approach here: we inject our own Listener if and only if it replaces the (only) default one.
                if( Trace.Listeners.Count == 1
                    && Trace.Listeners[0] is DefaultTraceListener def
                    && def.Name == "Default" )
                {
                    Trace.Listeners.Clear();
                    Trace.Listeners.Add( new SafeTraceListener() );
                }

                string? p = AppContext.BaseDirectory;
                _binFolder = p;
                string? buildConfDir = null;
                foreach( var config in _allowedConfigurations )
                {
                    buildConfDir = FindAbove( p, config );
                    if( buildConfDir != null )
                    {
                        _buildConfiguration = config;
                        break;
                    }
                }
                if( _buildConfiguration == null )
                {
                    throw new InvalidOperationException( $"Initialization error: Unable to find parent folder named '{_allowedConfigurations.Concatenate( "' or '" )}' above '{_binFolder}'." );
                }
                Debug.Assert( buildConfDir != null );
                p = Path.GetDirectoryName( buildConfDir );
                if( Path.GetFileName( p ) != "bin" )
                {
                    throw new InvalidOperationException( $"Initialization error: Folder '{_buildConfiguration}' MUST be in 'bin' folder (above '{_binFolder}')." );
                }
                Debug.Assert( p != null );
                p = Path.GetDirectoryName( p );
                if( string.IsNullOrEmpty( p ) )
                {
                    throw new InvalidOperationException( $"The '{_binFolder}' must not be directly on the root." );
                }

                _testProjectFolder = p;
                _testProjectName = Path.GetFileName( p );
                p = Path.GetDirectoryName( p );

                string? testsFolder = null;
                bool hasGit = false;
                while( !string.IsNullOrEmpty( p ) && !(hasGit = Directory.Exists( Path.Combine( p, ".git" ) )) )
                {
                    if( Path.GetFileName( p ) == "Tests" ) testsFolder = p;
                    p = Path.GetDirectoryName( p );
                }
                if( !hasGit )
                {
                    throw new InvalidOperationException( $"Initialization error: The project must be in a git repository (above '{_binFolder}')." );
                }
                if( string.IsNullOrEmpty( p ) )
                {
                    throw new InvalidOperationException( $"The '.git' cannot be directly on the root." );
                }
                _repositoryFolder = p;
                if( testsFolder == null )
                {
                    throw new InvalidOperationException( $"Initialization error: A parent 'Tests' folder must exist above '{_testProjectFolder}'." );
                }
                _solutionFolder = Path.GetDirectoryName( testsFolder )!;
                _logFolder = Path.Combine( _testProjectFolder, "Logs" );
                // The first works in .Net framework, the second one in netcore.
                _isTestHost = Environment.CommandLine.Contains( "testhost" )
                                || AppDomain.CurrentDomain.GetAssemblies().IndexOf( a => a.GetName().Name == "testhost" ) >= 0;
            }
            catch( Exception ex )
            {
                _initializationError = ExceptionDispatchInfo.Capture( ex );
            }
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        static string? FindAbove( string path, string folderName )
        {
            var p = path;
            while( p != null && Path.GetFileName( p ) != folderName )
            {
                p = Path.GetDirectoryName( p );
            }
            return p;
        }

        /// <summary>
        /// Empty method that triggers the type initializer: this ensures that the
        /// basic static members and hooks are initialized.
        /// This is almost always useless to call this explicitly since as soon as any TestHelper
        /// object is implied, this core type initializer is called.
        /// </summary>
        public static void Touch()
        {
        }
    }
}
