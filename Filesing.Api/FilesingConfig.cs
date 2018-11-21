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
    /// The configuration for running Filesing.
    /// </summary>
    public class FilesingConfig
    {
        // ---------------- Fields ----------------

        // ---------------- Constructor -----------------

        public FilesingConfig()
        {
            this.PatternConfigs = new List<PatternConfig>();
            this.NumberOfThreads = 1;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The directory to search for files that match the pattern.
        /// </summary>
        public string SearchDirectoryLocation { get; set; }

        /// <summary>
        /// The number of threads to use while processing files.
        /// Defaulted to 1
        /// Set to 0 to user the number of cores on the PC.
        /// </summary>
        public int NumberOfThreads { get; set; }

        /// <summary>
        /// Patterns to search for.
        /// </summary>
        public IList<PatternConfig> PatternConfigs { get; private set; }

        // ---------------- Functions ----------------
    }
}
