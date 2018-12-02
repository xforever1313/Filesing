//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Filesing.Api
{
    /// <summary>
    /// This class contains the search pattern configuration
    /// for the files in the directory.
    /// </summary>
    public class PatternConfig
    {
        // ---------------- Fields ----------------

        private readonly List<IgnoreConfig> ignoreConfigs;
        private readonly List<RequireConfig> requireConfigs;

        // ---------------- Constructor -----------------

        public PatternConfig( Regex regex ) :
            this( regex, new List<IgnoreConfig>(), new List<RequireConfig>() )
        {
        }

        public PatternConfig( Regex regex, IEnumerable<IgnoreConfig> ignoreConfigs, IEnumerable<RequireConfig> requireConfigs )
        {
            ArgumentChecker.IsNotNull( regex, nameof( regex ) );
            ArgumentChecker.IsNotNull( ignoreConfigs, nameof( ignoreConfigs ) );
            ArgumentChecker.IsNotNull( requireConfigs, nameof( requireConfigs ) );

            this.Pattern = regex;
            this.ignoreConfigs = new List<IgnoreConfig>( ignoreConfigs );
            this.IgnoreConfigs = this.ignoreConfigs.AsReadOnly();

            this.requireConfigs = new List<RequireConfig>( requireConfigs );
            this.RequireConfigs = this.requireConfigs.AsReadOnly();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The pattern to search for in the files.
        /// </summary>
        public Regex Pattern { get; private set; }

        /// <summary>
        /// The ignore config for this pattern.
        /// </summary>
        public IReadOnlyList<IgnoreConfig> IgnoreConfigs { get; private set; }

        /// <summary>
        /// The require config for this pattern.
        /// </summary>
        public IReadOnlyList<RequireConfig> RequireConfigs { get; private set; }
    }
}
