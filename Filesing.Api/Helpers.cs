//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Filesing.Api
{
    public static class Helpers
    {
        public static string NormalizePath( this string path )
        {
            // GetFullPath will replace something like C:\\\Hello World/there\..\there/file.exe with
            // c:\Hello World\there\file.exe.
            string fullPath = Path.GetFullPath( path );

            // This will trim off something like:
            // C:\users\hello\
            // to
            // C:\users\hello
            return fullPath.TrimEnd( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar );
        }

        public static bool ArePathsEqual( string path1, string path2 )
        {
            return NormalizePath( path1 ) == NormalizePath( path2 );
        }
    }
}
