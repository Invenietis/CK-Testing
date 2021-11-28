using System;
using System.Collections.Generic;
using System.Text;
using CK.Core;

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
        /// <param name="folderIsAvailable">See <see cref="FolderIsAvailable"/>.</param>
        public CleanupFolderEventArgs( NormalizedPath folder, bool folderIsAvailable )
        {
            Folder = folder;
            FolderIsAvailable = folderIsAvailable;
        }

        /// <summary>
        /// Gets the folder path that has been cleaned up.
        /// </summary>
        public NormalizedPath Folder { get; }

        /// <summary>
        /// Gets whether the folder is available (its content has been cleared and a write test has been executed)
        /// or the folder is not available because it has been totally removed.
        /// </summary>
        public bool FolderIsAvailable { get; }
    }
}
