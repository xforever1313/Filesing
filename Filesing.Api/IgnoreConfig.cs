//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Filesing.Api
{
    /// <summary>
    /// This class specifies which files or directories should be ignored.
    /// </summary>
    public class IgnoreConfig
    {
        // ---------------- Fields ----------------

        private readonly HashSet<string> ignoredFiles;
        private readonly HashSet<string> ignoredDirectories;
        private readonly List<Regex> ignoredFilesWithRegex;
        private readonly List<Regex> ignoredDirsWithRegex;
        private readonly HashSet<string> ignoredFileExtensions;

        // ---------------- Constructor ----------------

        public IgnoreConfig()
        {
            this.ignoredFiles = new HashSet<string>();
            this.ignoredDirectories = new HashSet<string>();

            this.ignoredFilesWithRegex = new List<Regex>();
            this.IgnoredFilesWithRegex = this.ignoredFilesWithRegex.AsReadOnly();

            this.ignoredDirsWithRegex = new List<Regex>();
            this.IgnoredDirectoriesWithRegex = this.ignoredDirsWithRegex.AsReadOnly();

            this.ignoredFileExtensions = new HashSet<string>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Specific files to ignore.  Case-sensitive.
        /// </summary>
        public IReadOnlyCollection<string> IgnoredFiles => this.ignoredFiles;

        /// <summary>
        /// Specific directories to ignore.  Case-sensitive.
        /// </summary>
        public IReadOnlyCollection<string> IgnoredDirectories => this.ignoredDirectories;

        /// <summary>
        /// If a file matches any of these regexes, it will be ignored.
        /// </summary>
        public IReadOnlyList<Regex> IgnoredFilesWithRegex { get; private set; }

        /// <summary>
        /// If a directory matches any of these regexes, it will be ignored.
        /// </summary>
        public IReadOnlyList<Regex> IgnoredDirectoriesWithRegex { get; private set; }

        /// <summary>
        /// If a file's extension matches any of these regexes, it will be ignored.
        /// </summary>
        public IReadOnlyCollection<string> IgnoredFileExtensions => this.ignoredFileExtensions;

        // ---------------- Functions ----------------

        public void AddSpecificFileToIgnore( string file )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( file, nameof( file ) );
            this.ignoredFiles.Add( file.NormalizePath() );
        }

        public void AddSpecificDirToIgnore( string dir )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( dir, nameof( dir ) );
            this.ignoredDirectories.Add( dir.NormalizePath() );
        }

        public void AddFileRegexToIgnore( Regex fileRegex )
        {
            ArgumentChecker.IsNotNull( fileRegex, nameof( fileRegex ) );
            this.ignoredFilesWithRegex.Add( fileRegex );
        }

        public void AddDirRegexToIgnore( Regex dirRegex )
        {
            ArgumentChecker.IsNotNull( dirRegex, nameof( dirRegex ) );
            this.ignoredDirsWithRegex.Add( dirRegex );
        }

        public void AddIgnoredFileExtension( string extension )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( extension, nameof( extension ) );

            // Path.GetExtension always returns lowercase.
            this.ignoredFileExtensions.Add( extension.ToLower() );
        }


        /// <summary>
        /// Should we ignore the given path?
        /// </summary>
        /// <param name="path">The path to check</param>
        /// <returns>True if the path should be ignored, else false.</returns>
        public bool ShouldIgnore( string path )
        {
            path = path.NormalizePath();

            // First are we a diretory or a file?
            if( Directory.Exists( path ) )
            {
                if( this.IgnoredDirectories.Contains( path ) )
                {
                    return true;
                }

                foreach( Regex ignoredDir in this.IgnoredDirectoriesWithRegex )
                {
                    string dirName = Path.GetFileName( path );
                    if( ignoredDir.IsMatch( dirName ) )
                    {
                        return true;
                    }
                }
            }
            else if( File.Exists( path ) )
            {
                if( this.IgnoredFiles.Contains( path ) )
                {
                    return true;
                }

                foreach( Regex ignoredFileRegex in this.IgnoredFilesWithRegex )
                {
                    string fileName = Path.GetFileName( path );
                    if( ignoredFileRegex.IsMatch( fileName ) )
                    {
                        return true;
                    }
                }

                string extension = Path.GetExtension( path );
                if( this.IgnoredFileExtensions.Contains( extension ) )
                {
                    return true;
                }
            }
            else
            {
                // Not a file or directory, ignore it.
                return true;
            }

            return false;
        }
    }
}
