using System;
using System.Collections.Generic;
using System.Text;

namespace CK.Testing
{
    /// <summary>
    /// Event argument to the <see cref="IBasicTestHelper.OnCleanupFolder"/> event.
    /// </summary>
    public class CleanupFolderEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="CleanupFolderEventArgs"/>.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public CleanupFolderEventArgs( string folder )
        {
            Folder = folder;
        }

        /// <summary>
        /// Gets the folder path that has been cleaned up.
        /// </summary>
        public string Folder { get; }
    }
}
