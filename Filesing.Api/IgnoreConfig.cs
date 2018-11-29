//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
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
        private readonly List<Regex> ignoredFileExtensions;

        private static readonly string dirSepString =
            Regex.Escape( "" + Path.DirectorySeparatorChar + Path.AltDirectorySeparatorChar );

        // ---------------- Constructor ----------------

        public IgnoreConfig()
        {
            this.ignoredFiles = new HashSet<string>();
            this.ignoredDirectories = new HashSet<string>();

            this.ignoredFilesWithRegex = new List<Regex>();
            this.IgnoredFilesWithRegex = this.ignoredFilesWithRegex.AsReadOnly();

            this.ignoredDirsWithRegex = new List<Regex>();
            this.IgnoredDirectoriesWithRegex = this.ignoredDirsWithRegex.AsReadOnly();

            this.ignoredFileExtensions = new List<Regex>();
            this.IgnoredFileExtensions = this.ignoredFileExtensions.AsReadOnly();
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
        public IReadOnlyList<Regex> IgnoredFileExtensions { get; private set; }

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

        /// <summary>
        /// Adds a name of a directory to ignore.  This shall include
        /// the directory and any sub-directories.
        /// </summary>
        /// <example>
        /// This is the equivalent of putting a directory name in a .gitignore.
        /// In a .gitignore with C# projects, you usually want to ignore all bin directories,
        /// so you put 'bin' in the .gitignore.  If you want to do the same here,
        /// pass in 'bin'.
        /// </example>
        /// <param name="dirRegex">
        /// Regex to search for.
        /// </param>
        public void AddDirNameToIgnore( string dirRegex, bool ignoreCase = false )
        {
            ArgumentChecker.IsNotNull( dirRegex, nameof( dirRegex ) );

            RegexOptions options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            options |= RegexOptions.Compiled;

            string regex = string.Format(
                "[{0}](({1}[{0}])|({1}$))",
                dirSepString,
                dirRegex
            );

            this.ignoredDirsWithRegex.Add(
                new Regex(
                    regex,
                    options
                )
            );
        }

        public void AddIgnoredFileExtension( Regex extensionRegex )
        {
            ArgumentChecker.IsNotNull( extensionRegex, nameof( extensionRegex ) );
            this.ignoredFileExtensions.Add( extensionRegex );
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
                return this.CheckDir( path );
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

                foreach( Regex extensionRegex in this.IgnoredFileExtensions )
                {
                    string extension = Path.GetExtension( path );
                    if( extensionRegex.IsMatch( extension ) )
                    {
                        return true;
                    }
                }

                // Next check directory:
                string dirPath = Path.GetDirectoryName( path );
                return this.CheckDir( dirPath );
            }
            else
            {
                // Not a file or directory, ignore it.
                return true;
            }
        }

        private bool CheckDir( string path )
        {
            if( this.IgnoredDirectories.Contains( path ) )
            {
                return true;
            }

            // If an ignored directory is c:\users\me,
            // but a directory checking is c:\users\me\Documents,
            // we need to make sure c:\users\me\Documents gets ignored.
            foreach( string ignoredDir in this.IgnoredDirectories )
            {
                string regex = string.Format(
                    "^{0}[{1}]",
                    Regex.Escape( ignoredDir ),
                    dirSepString
                );

                if( Regex.IsMatch( path, regex ) )
                {
                    return true;
                }
            }

            foreach( Regex ignoredDir in this.IgnoredDirectoriesWithRegex )
            {
                if( ignoredDir.IsMatch( path ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
