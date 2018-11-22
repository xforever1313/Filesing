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
    public static class FileProcessor
    {
        public static IReadOnlyList<MatchResult> ProcessFile( string filePath, FilesingConfig config )
        {
            using( FileStream inFile = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                return ProcessStream( filePath, inFile, config );
            }
        }

        public static IReadOnlyList<MatchResult> ProcessStream( string filePath, Stream stream, FilesingConfig config )
        {
            List<MatchResult> results = new List<MatchResult>();

            using( StreamReader reader = new StreamReader( stream ) )
            {
                string line = null;
                int lineNumber = 0;
                do
                {
                    ++lineNumber;

                    line = reader.ReadLine();
                    if( line != null )
                    {
                        foreach( PatternConfig pattern in config.PatternConfigs )
                        {
                            if( pattern.Pattern.IsMatch( line ) )
                            {
                                MatchResult result = new MatchResult
                                {
                                    File = filePath,
                                    Line = line,
                                    LineNumber = lineNumber,
                                    Pattern = pattern.Pattern.ToString()
                                };

                                results.Add( result );

                                Console.WriteLine( result );
                            }
                        }
                    }
                }
                while( line != null );

                return results;
            }
        }
    }
}