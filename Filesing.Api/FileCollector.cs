//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SethCS.Basic;
using SethCS.Exceptions;

namespace Filesing.Api
{
    public class FileCollector
    {
        // ---------------- Fields ----------------

        private readonly GenericLogger log;

        // ---------------- Constructor ----------------

        public FileCollector( GenericLogger log )
        {
            this.log = log;
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Gets all of the files to search for patterns based on the passed
        /// in configuration.  Any file that is marked to be ignored is NOT included
        /// in the returned list.
        /// </summary>
        public IList<string> FindAllFiles( FilesingConfig config )
        {
            config.Validate();

            List<string> files = new List<string>();

            SearchDir( config.SearchDirectoryLocation, files, config );

            return files;
        }

        private void SearchDir( string baseDir, IList<string> files, FilesingConfig config )
        {
            this.log.WriteLine( FilesingConstants.HeavyVerbosity, "Searching '{0}'", baseDir );

            foreach( string file in Directory.GetFiles( baseDir ) )
            {
                string fileName = Path.Combine( baseDir, file ).NormalizePath();
                files.Add( fileName );
            }

            foreach( string dir in Directory.GetDirectories( baseDir ) )
            {
                SearchDir( Path.Combine( baseDir, dir ), files, config );
            }
        }
    }
}