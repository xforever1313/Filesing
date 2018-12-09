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
using System.Text;
using System.Text.RegularExpressions;
using Filesing.Api;
using NUnit.Framework;

namespace Filesing.Tests
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // ---------------- Fields ----------------

        private static readonly string testFilesDir = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", // Up to Debug
            "..", // Up to bin
            "..", // Up to Filesing.Tests
            "TestFiles"
        );

        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        [Test]
        public void XmlLoadTest()
        {
            string fileName = Path.Combine( testFilesDir, "TestConfig.xml" );
            FilesingConfig config = XmlLoader.LoadConfigFromXml( fileName, TestDirManager.TestDir );

            // Check global ignore config
            {
                Assert.AreEqual( 1, config.GlobalIgnoreConfigs.Count );

                IgnoreConfig globalIgnoreConfig = config.GlobalIgnoreConfigs[0];

                // Should only contain 1 file:
                Assert.AreEqual( 1, globalIgnoreConfig.IgnoredFiles.Count );
                PathExistsInCollection( globalIgnoreConfig.IgnoredFiles, TestDirManager.RootFile1 );

                // Should only contain 1 dir:
                Assert.AreEqual( 1, globalIgnoreConfig.IgnoredDirectories.Count );
                PathExistsInCollection( globalIgnoreConfig.IgnoredDirectories, TestDirManager.Dir2_MyDir );

                // Should only contain 1 file with regex:
                Assert.AreEqual( 1, globalIgnoreConfig.IgnoredFilesWithRegex.Count );
                FileRegexExistsInCollection( globalIgnoreConfig.IgnoredFilesWithRegex, "myFile.+", false );

                // Should only contain 1 dir with regex:
                Assert.AreEqual( 1, globalIgnoreConfig.IgnoredDirectoriesWithRegex.Count );
                DirRegexExistsInCollection( globalIgnoreConfig.IgnoredDirectoriesWithRegex, "dir3", true );

                // Should contain 2 extensions to ignore.
                Assert.AreEqual( 2, globalIgnoreConfig.IgnoredFileExtensions.Count );
                FileRegexExistsInCollection( globalIgnoreConfig.IgnoredFileExtensions, @"\.html", true );
                FileRegexExistsInCollection( globalIgnoreConfig.IgnoredFileExtensions, @"\.dll", false );
            }

            // Check Requirements Config
            {
                Assert.AreEqual( 1, config.GlobalRequireConfigs.Count );

                RequireConfig globalRequireConfig = config.GlobalRequireConfigs[0];

                // Should only contain 1 file:
                Assert.AreEqual( 1, globalRequireConfig.RequiredFiles.Count );
                PathExistsInCollection( globalRequireConfig.RequiredFiles, TestDirManager.Dir1_HelloHtml );

                // Should only contain 1 dir:
                Assert.AreEqual( 1, globalRequireConfig.RequiredDirs.Count );
                PathExistsInCollection( globalRequireConfig.RequiredDirs, TestDirManager.Dir1 );
            }

            // Check Patterns
            Assert.AreEqual( 3, config.PatternConfigs.Count );

            // Check pattern 0
            {
                PatternConfig config0 = config.PatternConfigs[0];
                Assert.AreEqual( "public", config0.Pattern.ToString() );
                Assert.AreEqual( RegexOptions.Compiled | RegexOptions.IgnoreCase, config0.Pattern.Options );

                // No ignores or requirements.
                Assert.AreEqual( 0, config0.IgnoreConfigs.Count );
                Assert.AreEqual( 0, config0.RequireConfigs.Count );
            }

            // Check pattern 1:
            {
                PatternConfig config1 = config.PatternConfigs[1];
                Assert.AreEqual( "class", config1.Pattern.ToString() );
                Assert.AreEqual( RegexOptions.Compiled, config1.Pattern.Options );

                // Check ignore config
                {
                    Assert.AreEqual( 1, config1.IgnoreConfigs.Count );
                    IgnoreConfig ignoreConfig = config1.IgnoreConfigs[0];

                    // Should only contain 1 file:
                    Assert.AreEqual( 1, ignoreConfig.IgnoredFiles.Count );
                    PathExistsInCollection( ignoreConfig.IgnoredFiles, TestDirManager.RootFile2 );

                    // Should only contain 1 dir:
                    Assert.AreEqual( 1, ignoreConfig.IgnoredDirectories.Count );
                    PathExistsInCollection( ignoreConfig.IgnoredDirectories, TestDirManager.Dir1 );

                    // Should only contain 1 file with regex:
                    Assert.AreEqual( 1, ignoreConfig.IgnoredFilesWithRegex.Count );
                    FileRegexExistsInCollection( ignoreConfig.IgnoredFilesWithRegex, "Hello", false );

                    // Should only contain 1 dir with regex:
                    Assert.AreEqual( 1, ignoreConfig.IgnoredDirectoriesWithRegex.Count );
                    DirRegexExistsInCollection( ignoreConfig.IgnoredDirectoriesWithRegex, "Dir1", false );

                    // Should contain 2 extensions to ignore.
                    Assert.AreEqual( 2, ignoreConfig.IgnoredFileExtensions.Count );
                    FileRegexExistsInCollection( ignoreConfig.IgnoredFileExtensions, @"\.txt", true );
                    FileRegexExistsInCollection( ignoreConfig.IgnoredFileExtensions, @"\.EXE", false );
                }

                // Check requirements config
                {
                    Assert.AreEqual( 1, config1.RequireConfigs.Count );

                    RequireConfig requireConfig = config1.RequireConfigs[0];

                    // Should only contain 1 file:
                    Assert.AreEqual( 1, requireConfig.RequiredFiles.Count );
                    PathExistsInCollection( requireConfig.RequiredFiles, TestDirManager.Dir2_MyfileTxt );

                    // Should only contain 1 dir:
                    Assert.AreEqual( 1, requireConfig.RequiredDirs.Count );
                    PathExistsInCollection( requireConfig.RequiredDirs, TestDirManager.Dir2_MyDir );
                }
            }

            // Check pattern 2:
            {
                PatternConfig config2 = config.PatternConfigs[2];
                Assert.AreEqual( "hello", config2.Pattern.ToString() );
                Assert.AreEqual( RegexOptions.Compiled | RegexOptions.IgnoreCase, config2.Pattern.Options );

                // No ignores or requirements.
                Assert.AreEqual( 0, config2.IgnoreConfigs.Count );
                Assert.AreEqual( 0, config2.RequireConfigs.Count );
            }
        }

        // ---------------- Test Helpers ----------------

        private static void PathExistsInCollection( IReadOnlyCollection<string> collection, string path )
        {
            string fullPath = Path.GetFullPath( path );
            Assert.IsTrue( collection.Contains( fullPath ) );
        }

        private static void FileRegexExistsInCollection( IReadOnlyCollection<Regex> collection, string pattern, bool ignoreCase )
        {
            RegexOptions options = RegexOptions.Compiled;
            if( ignoreCase )
            {
                options |= RegexOptions.IgnoreCase;
            }

            Regex expectedRegex = new Regex( pattern, options );

            Regex foundRegex = collection.FirstOrDefault(
                r =>
                {
                    return r.Options.Equals( expectedRegex.Options ) && r.ToString().Equals( expectedRegex.ToString() );
                }
            );
            Assert.IsNotNull( foundRegex );
        }

        private static void DirRegexExistsInCollection( IReadOnlyCollection<Regex> collection, string pattern, bool ignoreCase )
        {
            Regex expectedRegex = IgnoreConfig.CreateIgnoreDirRegex( pattern, ignoreCase );

            Regex foundRegex = collection.FirstOrDefault(
                r =>
                {
                    return r.Options.Equals( expectedRegex.Options ) && r.ToString().Equals( expectedRegex.ToString() );
                }
            );
            Assert.IsNotNull( foundRegex );
        }
    }
}
