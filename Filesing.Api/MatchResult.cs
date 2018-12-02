//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Filesing.Api
{
    public class MatchResult
    {
        // ---------------- Constructor ----------------

        public MatchResult()
        {
        }

        // ---------------- Properties ----------------

        public string File { get; internal set; }

        /// <summary>
        /// The line number.  If this is set to 0, it means
        /// it matched the file name itself.
        /// </summary>
        public int LineNumber { get; internal set; }

        public string Line { get; internal set; }

        public string Pattern { get; internal set; }

        // ---------------- Functions ----------------

        public override string ToString()
        {
            if( this.LineNumber == 0 )
            {
                return string.Format(
                    "Matched '{0}' in file path '{1}'.",
                    this.Pattern,
                    this.File
                );
            }
            else
            {
                return string.Format(
                    "Matched '{0}' in '{1}' on line {2}: '{3}'.",
                    this.Pattern,
                    this.File,
                    this.LineNumber,
                    this.Line
                );
            }
        }
    }
}