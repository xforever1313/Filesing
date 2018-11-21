//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Filesing.Api
{
    /// <summary>
    /// This class contains the search pattern configuration
    /// for the files in the directory.
    /// </summary>
    public class PatternConfig
    {
        // ---------------- Fields ----------------

        private HashSet<string> filesToIgnore;

        // ---------------- Constructor -----------------

        public PatternConfig( Regex regex, IEnumerable<string> filesToIgnore = null )
        {
            this.Pattern = regex;
            this.filesToIgnore = new HashSet<string>();

            if( filesToIgnore != null )
            {
                foreach( string file in filesToIgnore )
                {
                    this.filesToIgnore.Add( file );
                }
            }
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The pattern to search for in the files.
        /// </summary>
        public Regex Pattern { get; private set; }

        /// <summary>
        /// What files to ignore this pattern with.
        /// </summary>
        public IReadOnlyCollection<string> FilesToIgnore
        {
            get
            {
                return this.filesToIgnore;
            }
        }
    }
}
