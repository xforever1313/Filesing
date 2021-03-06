﻿//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Filesing.Api;
using Mono.Options;
using SethCS.Basic;
using SethCS.IO;

namespace Filesing.Cli
{
    static class Program
    {
        static int Main( string[] args )
        {
            GenericLogger log = new GenericLogger();

            // Generic Logger's WriteLine adds a new line.  So,
            // For Console, only call Write, as if we call WriteLine,
            // we'll get 2 new lines... one from GenericLogger, one from
            // Console.WriteLine.
            log.OnWriteLine += Console.Write;
            log.OnWarningWriteLine += Log_OnWarningWriteLine;
            log.OnErrorWriteLine += Log_OnErrorWriteLine;

            try
            {
                bool showHelp = false;
                bool showVersion = false;
                bool showLicense = false;
                string inFile = string.Empty;
                string searchDir = string.Empty;
                int numThreads = 1;
                int verbosity = 0;
                int foundExitCode = 0;
                Regex regex = null;

                OptionSet options = new OptionSet
                {
                    {
                        "h|help",
                        "Shows this message and exits.",
                        h => showHelp = ( h != null )
                    },
                    {
                        "version",
                        "Shows the version and exits.",
                        v => showVersion = ( v != null )
                    },
                    {
                        "license",
                        "Shows the license information and exits.",
                        l => showLicense = ( l != null )
                    },
                    {
                        "f|configfile=",
                        "The input file to determine which patterns to search for.  Required if regex is not specified.",
                        f => inFile = Path.GetFullPath( f )
                    },
                    {
                        "d|searchdir=",
                        "The directory to search for files that match the patterns.  Required.",
                        d => searchDir = Path.GetFullPath( d )
                    },
                    {
                        "j|numthreads=",
                        "The number of threads to use.  0 for the processor count. Defaulted to 1.",
                        j =>
                        {
                            if ( int.TryParse( j, out numThreads ) == false )
                            {
                                throw new ArgumentException(
                                    "Number of threads must be an integer, got: '" + j + "'"
                                );
                            }
                        }
                    },
                    {
                        "v|verbosity=",
                        "How verbose the output should be.  Levels are 0, 1, 2, and 3.  0 (the default) or less prints the least, 3 or more prints the most.",
                        v =>
                        {
                            if ( int.TryParse( v, out verbosity ) == false)
                            {
                                throw new ArgumentException(
                                    "Verbosity must be an integer, got: '" + v + "'"
                                );
                            }
                        }
                    },
                    {
                        "e|exit_code_on_find=",
                        "If ANY matches are found, what the exit code should be.  Defaulted to 0.",
                        e =>
                        {
                            if ( int.TryParse( e, out foundExitCode ) == false )
                            {
                                throw new ArgumentException(
                                    "exit_code_on_find must be an integer, got: '" + e + "'"
                                );
                            }
                        }
                    },
                    {
                        "r|regex=",
                        "Searches the diretory for this regex.  If a config file is specified as well, the global " +
                        "ignore/require settings in the config file are applied when searching for this regex." + 
                        " Optional (uses regexes in config file if not specified).",
                        r => { regex = new Regex( r, RegexOptions.Compiled ); }
                    }
                };

                options.Parse( args );

                // Help will take precedence, followed by version, then
                // license.
                if( showHelp )
                {
                    options.WriteOptionDescriptions( Console.Out );
                }
                else if( showVersion )
                {
                    ShowVersion();
                }
                else if( showLicense )
                {
                    ShowLicense();
                }
                else
                {
                    log.Verbosity = verbosity;

                    FilesingConfig config = GenerateConfig( regex, searchDir, log, inFile );
                    config.NumberOfThreads = numThreads;

                    log.WriteLine(
                        FilesingConstants.LightVerbosity,
                        "- Search directory specified: '" + searchDir + "'"
                    );

                    IReadOnlyList<MatchResult> results = null;
                    Stopwatch stopWatch = new Stopwatch();
                    try
                    {
                        stopWatch.Start();
                        using( FilesingRunner runner = new FilesingRunner( config, log ) )
                        {
                            runner.Start();
                            results = runner.Join();
                        }
                    }
                    finally
                    {
                        stopWatch.Stop();
                    }

                    log.WriteLine( FilesingConstants.LightVerbosity, "Operation took: " + stopWatch.Elapsed.TotalSeconds + "s" );

                    if( results.Count == 0 )
                    {
                        log.WriteLine( "No Matches Found!" );
                    }
                    else
                    {
                        log.WriteLine( "Matches Found:" );
                        foreach( MatchResult result in results )
                        {
                            log.WriteLine( result.ToString() );
                        }
                        return foundExitCode;
                    }
                }
            }
            catch( OptionException e )
            {
                log.WriteLine( e.Message );
                return -1;
            }
            catch( Exception e )
            {
                log.ErrorWriteLine( "FATAL ERROR: " + e.Message );
                return -2;
            }

            return 0;
        }

        private static FilesingConfig GenerateConfig( Regex regex, string searchDir, GenericLogger log, string inFile )
        {
            FilesingConfig config;

            if( regex == null )
            {
                log.WriteLine(
                    FilesingConstants.LightVerbosity,
                    "- Using regexes from config file '{0}'",
                    inFile
                );

                config = XmlLoader.LoadConfigFromXml( inFile, searchDir );
            }
            else
            {
                log.WriteLine(
                    FilesingConstants.LightVerbosity,
                    "- Regex specified on command line.  Using regex '" + regex.ToString() + "'"
                );

                if( string.IsNullOrWhiteSpace( inFile ) )
                {
                    log.WriteLine(
                        FilesingConstants.LightVerbosity,
                        "- No config file specified, not ignoring any files or directories."
                    );
                    config = new FilesingConfig();
                }
                else
                {
                    log.WriteLine(
                        FilesingConstants.LightVerbosity,
                        "- Config file '{0}' specified.  Using config file's global ignores and requires.  Ignoring file's patterns since 'regex' was specified on CLI.",
                        inFile
                    );
                    config = XmlLoader.LoadConfigFromXml( inFile, searchDir );
                }

                config.PatternConfigs.Clear();
                PatternConfig patternConfig = new PatternConfig( regex );
                config.PatternConfigs.Add( patternConfig );
            }

            config.SearchDirectoryLocation = searchDir;

            return config;
        }

        private static void Log_OnWarningWriteLine( string obj )
        {
            using( ConsoleColorResetter reset = new ConsoleColorResetter( ConsoleColor.Yellow, null ) )
            {
                Console.Write( obj );
            }
        }

        private static void Log_OnErrorWriteLine( string obj )
        {
            using( ConsoleColorResetter reset = new ConsoleColorResetter( ConsoleColor.Red, null ) )
            {
                Console.Error.Write( obj );
            }
        }

        private static void ShowLicense()
        {
            const string license =
@"Filesing - Copyright Seth Hendrick 2018.

Boost Software License - Version 1.0 - August 17th, 2003

Permission is hereby granted, free of charge, to any person or organization
obtaining a copy of the software and accompanying documentation covered by
this license (the ""Software"") to use, reproduce, display, distribute,
execute, and transmit the Software, and to prepare derivative works of the
Software, and to permit third-parties to whom the Software is furnished to
do so, all subject to the following:

The copyright notices in the Software and this entire statement, including
the above license grant, this restriction and the following disclaimer,
must be included in all copies of the Software, in whole or in part, and
all derivative works of the Software, unless such copies or derivative
works are solely in the form of machine-executable object code generated by
a source language processor.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.
";
            Console.WriteLine( license );
        }

        private static void ShowVersion()
        {
            Console.WriteLine(
                Assembly.GetExecutingAssembly().GetName().Version
            );
        }
    }
}
