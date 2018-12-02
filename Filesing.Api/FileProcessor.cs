//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using SethCS.Basic;

namespace Filesing.Api
{
    public class FileProcessor
    {
        // ---------------- Fields ----------------

        private readonly FilesingConfig config;
        private readonly GenericLogger log;
        private readonly string name;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">Reference to the global filesing config.</param>
        /// <param name="log">The log to write to.</param>
        /// <param name="name">The name of this file processor (e.g. could be a thread name).</param>
        public FileProcessor( FilesingConfig config, GenericLogger log, string name )
        {
            this.config = config;
            this.log = log;
            this.name = name;
        }

        // ---------------- Functions ----------------

        public IReadOnlyList<MatchResult> ProcessFile( string filePath  )
        {
            using( FileStream inFile = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
            {
                return ProcessStream( filePath, inFile );
            }
        }

        public IReadOnlyList<MatchResult> ProcessStream( string filePath, Stream stream )
        {
            List<PatternConfig> patternsToUse = new List<PatternConfig>();
            foreach( PatternConfig patternConfig in config.PatternConfigs )
            {
                if( ShouldProcessFile( filePath, config, patternConfig ) )
                {
                    patternsToUse.Add( patternConfig );
                }
            }

            List<MatchResult> results = new List<MatchResult>();
            if( patternsToUse.Count == 0 )
            {
                this.Status( FilesingConstants.LightVerbosity, "Ignoring file '" + filePath + "'" );
                return results;
            }

            this.Status( FilesingConstants.LightVerbosity, "Processing file: '" + filePath + "'" );

            // Check filename first.
            foreach( PatternConfig pattern in patternsToUse )
            {
                if( pattern.Pattern.IsMatch( filePath ) )
                {
                    MatchResult result = new MatchResult
                    {
                        File = filePath,
                        Line = string.Empty,
                        LineNumber = 0,
                        Pattern = pattern.Pattern.ToString()
                    };

                    results.Add( result );

                    this.Status( FilesingConstants.HeavyVerbosity, result.ToString() );
                }
            }

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
                        foreach( PatternConfig pattern in patternsToUse )
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

                                this.Status( FilesingConstants.HeavyVerbosity, result.ToString() );
                            }
                        }
                    }
                }
                while( line != null );

                return results;
            }
        }

        public bool ShouldProcessFile( string filePath, FilesingConfig config, PatternConfig currentPattern )
        {
            bool globalIgnore = false;
            for( int i = 0; ( i < config.GlobalIgnoreConfigs.Count ) && ( globalIgnore == false ); ++i )
            {
                // If the path should be ignored, flag it as such, and break out of the loop.
                globalIgnore = config.GlobalIgnoreConfigs[i].ShouldIgnore( filePath );
            }

            bool globalRequire = false;
            for( int i = 0; ( i < config.GlobalRequireConfigs.Count ) && ( globalRequire == false ); ++i )
            {
                // If the path is required, flag it as such, and break out of the loop.
                globalRequire = config.GlobalRequireConfigs[i].IsRequired( filePath );
            }

            bool patternIgnore = false;
            for( int i = 0; ( i < currentPattern.IgnoreConfigs.Count ) && ( patternIgnore == false ); ++i )
            {
                patternIgnore = currentPattern.IgnoreConfigs[i].ShouldIgnore( filePath );
            }

            bool patternRequire = false;
            for( int i = 0; ( i < currentPattern.RequireConfigs.Count ) && ( patternRequire == false ); ++i )
            {
                patternRequire = currentPattern.RequireConfigs[i].IsRequired( filePath );
            }

            if( patternRequire )
            {
                // If our pattern says its required,
                // return true, as it overrides any global config.
                return true;
            }
            else if( patternIgnore )
            {
                // If our pattern says to ignore the file,
                // ignore the file, as it overrides any global config.
                return false;
            }
            else if( globalRequire )
            {
                // If the file falls into the categories that globally are required,
                // it should be processed.
                return true;
            }
            else if( globalIgnore )
            {
                // If thf file falls into the categories that globally are ignored,
                // it should not be processed.
                return false;
            }

            // By default, always process the file.
            return true;
        }

        private void Status( int verbosity, string message )
        {
            this.log.WriteLine( verbosity, this.name + "> " + message );
        }
    }
}