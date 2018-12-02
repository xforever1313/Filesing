//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Filesing.Api
{
    /// <summary>
    /// This class specifies which files or directories
    /// will be searched even if they are marked as ignored
    /// (e.g. maybe a user wants an entire directory ignored
    /// EXCEPT for one file in it).
    /// </summary>
    public class RequireConfig
    {
        // ---------------- Fields ----------------

        private readonly HashSet<string> requiredFiles;
        private readonly HashSet<string> requiredDirs;

        private static readonly string dirSepString =
            Regex.Escape( "" + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar );

        // ---------------- Constructor ----------------

        public RequireConfig()
        {
            this.requiredFiles = new HashSet<string>();
            this.requiredDirs = new HashSet<string>();
        }

        // ---------------- Properties ----------------

        public IReadOnlyCollection<string> RequiredFiles => this.requiredFiles;

        public IReadOnlyCollection<string> RequiredDirs => this.requiredDirs;

        // ---------------- Functions ----------------

        public void AddRequiredFile( string filePath )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( filePath, nameof( filePath ) );
            this.requiredFiles.Add( filePath.NormalizePath() );
        }

        /// <summary>
        /// Adds a required directory to be processed.
        /// Note that this will also require all sub-directories and files,
        /// even if they are marked "ignore".
        /// </summary>
        public void AddRequiredDir( string dirPath )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( dirPath, nameof( dirPath ) );
            this.requiredDirs.Add( dirPath.NormalizePath() );
        }

        /// <summary>
        /// Is the given path required to be processed?
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path must be processed, else false.</returns>
        public bool IsRequired( string path )
        {
            path = path.NormalizePath();

            if( Directory.Exists( path ) )
            {
                return this.CheckDir( path );
            }
            else if( File.Exists( path ) )
            {
                if( this.requiredFiles.Contains( path ) )
                {
                    return true;
                }

                // Next check directory.  If file is in a required directory,
                // it must be processed.
                string dirPath = Path.GetDirectoryName( path );
                return this.CheckDir( dirPath );
            }
            else
            {
                // If the path does not exist, it is marked as not required.
                return false;
            }
        }

        private bool CheckDir( string path )
        {
            if( this.requiredDirs.Contains( path ) )
            {
                return true;
            }

            // If a required directory is c:\users\me,
            // but the directory that is being checked is c:\users\me\Documents,
            // we need to make sure that c:\users\me\Documents is checked.
            foreach( string requiredDir in this.requiredDirs )
            {
                string regex = string.Format(
                    "^{0}[{1}]",
                    Regex.Escape( requiredDir ),
                    dirSepString
                );

                if( Regex.IsMatch( path, regex ) )
                {
                    return true;
                }
            }


            // Otherwise, it is not required.
            return false;
        }
    }
}
