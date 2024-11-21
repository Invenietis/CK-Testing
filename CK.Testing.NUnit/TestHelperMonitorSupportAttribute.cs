using CK.Core;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using static CK.Testing.MonitorTestHelper;


namespace CK.Testing.NUnit;

/// <summary>
/// Makes each NUnit tests log as groups and logs the <see cref="TestResult.Message"/> and <see cref="TestResult.StackTrace"/>
/// on error into the <see cref="Monitoring.IMonitorTestHelperCore.Monitor"/>.
/// </summary>
[AttributeUsage( AttributeTargets.Assembly, AllowMultiple = false )]
public class TestHelperMonitorSupportAttribute : Attribute, ITestAction
{
    readonly Stack<IDisposableGroup> _groups;

    public TestHelperMonitorSupportAttribute()
    {
        _groups = new Stack<IDisposableGroup>();
    }

    void ITestAction.BeforeTest( ITest test )
    {
        _groups.Push( TestHelper.Monitor.UnfilteredOpenGroup( LogLevel.Info | LogLevel.IsFiltered, null, $"Running '{test.Name}'.", null ) );
    }

    void ITestAction.AfterTest( ITest test )
    {
        var result = TestExecutionContext.CurrentContext.CurrentResult;
        var g = _groups.Pop();
        if( result.ResultState.Status != TestStatus.Passed )
        {
            g.ConcludeWith( () => result.ResultState.Status.ToString() );
            var message = result.Message;
            if( !string.IsNullOrWhiteSpace( message ) )
            {
                TestHelper.Monitor.OpenError( message );
                if( result.StackTrace != null ) TestHelper.Monitor.Trace( result.StackTrace );
            }
        }
        g.Dispose();
    }

    ActionTargets ITestAction.Targets => ActionTargets.Test | ActionTargets.Suite;

}
