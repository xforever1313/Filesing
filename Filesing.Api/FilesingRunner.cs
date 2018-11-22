//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using SethCS.Basic;

namespace Filesing.Api
{
    /// <summary>
    /// This is the class that runs through all of the files
    /// and searches for matches.
    /// </summary>
    public class FilesingRunner : IDisposable
    {
        // ---------------- Fields ----------------

        private bool keepRunning;
        private object keepRunningObject;
        private bool started;

        private List<Thread> threads;

        private FilesingConfig config;

        private IList<string> filesToProcess;

        private GenericLogger log;

        // ---------------- Constructor -----------------

        public FilesingRunner( FilesingConfig config, GenericLogger log )
        {
            if( config == null )
            {
                throw new ArgumentNullException( nameof( config ) );
            }

            this.threads = new List<Thread>( config.NumberOfThreads );
            this.keepRunning = false;
            this.started = false;
            this.keepRunningObject = new object();

            for( int i = 0; i < config.NumberOfThreads; ++i )
            {
                string threadName = i.ToString();
                Thread thread = new Thread( () => this.ThreadEntry( threadName ) );
                thread.Name = threadName;
            }

            this.config = config;
            this.log = log;
        }

        ~FilesingRunner()
        {
            try
            {
                this.Dispose( false );
            }
            catch
            {
                // Don't let GC thread die.
            }
        }

        // ---------------- Properties ----------------

        public bool IsRunning
        {
            get
            {
                lock( this.keepRunningObject )
                {
                    return this.keepRunning;
                }
            }

            private set
            {
                lock( this.keepRunningObject )
                {
                    this.keepRunning = value;
                }
            }
        }

        // ---------------- Functions ----------------

        public void Start()
        {
            if( this.started )
            {
                throw new InvalidOperationException( "Operation already started" );
            }

            FileCollector collector = new FileCollector( this.log );
            this.filesToProcess = collector.FindAllFiles( this.config );

            this.started = true;
            this.IsRunning = true;
            foreach( Thread thread in this.threads )
            {
                thread.Start();
            }
        }

        public void Join()
        {
            foreach( Thread thread in this.threads )
            {
                thread.Join();
            }
        }

        public void Dispose()
        {
            this.Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected void Dispose( bool fromDispose )
        {
            this.IsRunning = false;

            // If we are from Dispose() and not from Finalize,
            // gracefully stop the threads (which is what setting IsRunning to false does).
            // However, if we are from finalize,
            // stop the threads as quickly as possible, as this class is being
            // GCed.
            if( fromDispose == false )
            {
                foreach( Thread thread in this.threads )
                {
                    thread.Interrupt();
                }
            }

            foreach( Thread thread in this.threads )
            {
                thread.Join();
            }
        }

        private void ThreadEntry( string threadName )
        {
            try
            {
                this.log.WriteLine( threadName + "> Started." );

                string file;
                lock( this.filesToProcess )
                {
                    if( filesToProcess.Count == 0 )
                    {
                        return;
                    }

                    file = this.filesToProcess[0];
                    this.filesToProcess.RemoveAt( 0 );
                }

                this.log.WriteLine( threadName + "> Processing " + file );
            }
            catch( Exception err )
            {
                this.log.ErrorWriteLine(
                    threadName + " > FATAL ERROR, exiting: " + Environment.NewLine + err.Message
                );
            }
            finally
            {
                this.log.WriteLine( threadName + "> Exiting." );
            }
        }
    }
}
