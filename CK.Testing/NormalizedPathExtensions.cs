using CK.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CK.Text
{
    public static class NormalizedPathExtensions
    {
        /// <summary>
        /// Finds a file in this path or in a parent path.
        /// </summary>
        /// <param name="this">This path.</param>
        /// <param name="fileName">The file name to look for in order of preference inside the same folder.</param>
        /// <returns>The path of the file. <see cref="NormalizedPath.IsEmpty"/> if not found.</returns>
        public static NormalizedPath FindClosestFile( this NormalizedPath @this, params string[] fileName )
        {
            while( !@this.IsEmpty )
            {
                foreach( var f in fileName )
                {
                    var candidate = @this.AppendPart( f );
                    if( File.Exists( candidate ) ) return candidate;
                }
                @this = @this.RemoveLastPart();
            }
            return @this;
        }
    }
}
