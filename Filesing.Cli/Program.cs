﻿//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Filesing.Api;
using Mono.Options;
using SethCS.Basic;

namespace Filesing.Cli
{
    class Program
    {
        static int Main( string[] args )
        {
            try
            {
                bool showHelp = false;
                bool showVersion = false;
                bool showLicense = false;
                string inFile = string.Empty;
                string searchDir = string.Empty;
                int numThreads = 1;

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
                        "The input file to determine which patterns to search for.  Required.",
                        f => inFile = f
                    },
                    {
                        "d|searchdir=",
                        "The directory to search for files that match the patterns.  Required.",
                        d => searchDir = d
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
                    FilesingConfig config = new FilesingConfig();
                    config.NumberOfThreads = 2;
                    config.SearchDirectoryLocation = searchDir;
                    config.ExtensionsToIgnore.Add( new Regex( @"\.exe", RegexOptions.IgnoreCase ) );
                    config.ExtensionsToIgnore.Add( new Regex( @"\.dll", RegexOptions.IgnoreCase ) );
                    config.ExtensionsToIgnore.Add( new Regex( @"\.pdb", RegexOptions.IgnoreCase ) );
                    config.ExtensionsToIgnore.Add( new Regex( @"\.db", RegexOptions.IgnoreCase ) );
                    config.ExtensionsToIgnore.Add( new Regex( @"\.dat", RegexOptions.IgnoreCase ) );
                    config.ExtensionsToIgnore.Add( new Regex( @"\.pyc", RegexOptions.IgnoreCase ) );

                    PatternConfig patternConfig = new PatternConfig(
                        new Regex( "class", RegexOptions.IgnoreCase | RegexOptions.Compiled )
                    );
                    config.PatternConfigs.Add( patternConfig );

                    GenericLogger log = new GenericLogger();

                    // Generic Logger's WriteLine adds a new line.  So,
                    // For Console, only call Write, as if we call WriteLine,
                    // we'll get 2 new lines... one from GenericLogger, one from
                    // Console.WriteLine.
                    log.OnWriteLine += Console.Write;
                    log.OnErrorWriteLine += Console.Error.Write;

                    using( FilesingRunner runner = new FilesingRunner( config, log ) )
                    {
                        runner.Start();
                        runner.Join();
                    }
                }
            }
            catch( OptionException e )
            {
                Console.WriteLine( e.Message );
                return -1;
            }
            catch( Exception e )
            {
                Console.WriteLine( "FATAL ERROR: " + e.Message );
                return -2;
            }

            return 0;
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
