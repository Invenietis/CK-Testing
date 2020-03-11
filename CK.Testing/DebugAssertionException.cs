using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Specific Exception type for <see cref="Debug.Fail(string)"/> or <see cref="Debug.Assert(bool)"/>.
    /// </summary>
    [Serializable]
    public class DebugAssertionException : Exception
    {
        /// <summary>
        /// Initializes a new <see cref="DebugAssertionException"/> that is thrown whenever <see cref="Debug.Fail(string)"/> or <see cref="Debug.Assert(bool)"/>
        /// failed.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="detailMessage">Optional detail of the message.</param>
        public DebugAssertionException( string message, string detailMessage = null )
            : base( String.IsNullOrEmpty( detailMessage ) ? message : $"{message} (Detail: '{detailMessage}')." )
        {
        }

    }
}
