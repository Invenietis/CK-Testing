using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using CK.Core;

namespace CK.Testing;


/// <summary>
/// Static part of the implementation of <see cref="BasicTestHelper"/>.
/// </summary>
public partial class StaticBasicTestHelper
{
    static readonly string[] _allowedConfigurations = new[] { "Debug", "Release" };
    internal static readonly NormalizedPath _binFolder;
    internal static readonly string _buildConfiguration;
    internal static readonly NormalizedPath _testProjectFolder;
    internal static readonly NormalizedPath _pathToBin;
    internal static readonly NormalizedPath _solutionFolder;
    internal static readonly NormalizedPath _logFolder;
    internal static readonly HashSet<string> _onlyOnce;
    internal static readonly ExceptionDispatchInfo _initializationError;

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
                Throw.InvalidOperationException( $"Initialization error: Unable to find parent folder named '{_allowedConfigurations.Concatenate( "' or '" )}' above '{_binFolder}'." );
            }
            Throw.DebugAssert( buildConfDir != null );
            p = FindAbove( p, "bin" );
            if( p == null )
            {
                Throw.InvalidOperationException( $"Initialization error: Unable to find 'bin' folder above '{_binFolder}'." );
            }
            Throw.DebugAssert( p != null );
            p = Path.GetDirectoryName( p );
            if( string.IsNullOrEmpty( p ) )
            {
                Throw.InvalidOperationException( $"The '{_binFolder}' must not be directly on the root." );
            }
            _testProjectFolder = p;
            p = Path.GetDirectoryName( p );

            string? firstTestsFolderAbove = null;
            bool hasGit = false;
            // We stop looking for the "Tests" folder on the first, deepest one.
            while( !string.IsNullOrEmpty( p ) && !(hasGit = Directory.Exists( Path.Combine( p, ".git" ) )) )
            {
                if( firstTestsFolderAbove == null && Path.GetFileName( p ) == "Tests" ) firstTestsFolderAbove = p;
                p = Path.GetDirectoryName( p );
            }
            if( !hasGit )
            {
                Throw.InvalidOperationException( $"Initialization error: The project must be in a git repository (above '{_binFolder}')." );
            }
            if( firstTestsFolderAbove == null )
            {
                Throw.InvalidOperationException( $"Initialization error: A parent 'Tests' folder must exist above '{_testProjectFolder}'." );
            }
            if( string.IsNullOrEmpty( p ) )
            {
                Throw.InvalidOperationException( $"The '.git' cannot be directly on the root." );
            }
            _solutionFolder = p;
            _logFolder = _testProjectFolder.AppendPart( "Logs" );
            _pathToBin = _binFolder.RemoveParts( 0, _testProjectFolder.Parts.Count );
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
    /// Triggers this type initializer and re-throws any initialization error
    /// that may have occurred.
    /// <para>
    /// This ensures that the basic static members and hooks are initialized.
    /// This is almost always useless to call this explicitly since as soon as any TestHelper
    /// object is implied, this core type initializer is called.
    /// </para>
    /// </summary>
    public static void EnsureInitialized()
    {
        if( _initializationError != null ) _initializationError.Throw();
    }
}
