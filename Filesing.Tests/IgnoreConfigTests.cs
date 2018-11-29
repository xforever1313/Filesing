//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Filesing.Api;
using NUnit.Framework;

namespace Filesing.Tests
{
    [TestFixture]
    public class IgnoreConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// If nothing is specified, nothing should be marked to be ignored.
        /// </summary>
        [Test]
        public void NothingSpecifiedTest()
        {
            IgnoreConfig config = new IgnoreConfig();
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.ShouldIgnore( dir ) );
            }
            foreach( string file in TestDirManager.FileList )
            {
                Assert.IsFalse( config.ShouldIgnore( file ) );
            }
        }

        /// <summary>
        /// Ensures the specific file check is working correctly.
        /// </summary>
        [Test]
        public void IgnoreSpecificFilesTest()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.HiddenDir_HelloTxt,
                TestDirManager.Dir1_HelloHtml,
                TestDirManager.Dir2_MyDir,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir3_HiddenHtml,
                TestDirManager.RootFile1
            };

            IgnoreConfig config = new IgnoreConfig();
            foreach( string file in ignoredFiles )
            {
                config.AddSpecificFileToIgnore( file );
            }

            // No directores are ignored.
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.ShouldIgnore( dir ), dir );
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// Ensures the regex file check is working as expected.
        /// </summary>
        [Test]
        public void IgnoreFileRegexTest()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.HiddenDir_HelloTxt,
                TestDirManager.HiddenDir_Hello2Txt,
                TestDirManager.Dir1_HelloHtml,
                TestDirManager.Dir1_HelloXml,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir2_Myfile2Txt
            };

            List<Regex> regexes = new List<Regex>
            {
                new Regex( "hello", RegexOptions.IgnoreCase ),
                new Regex( "myFile.+" )
            };

            IgnoreConfig config = new IgnoreConfig();
            foreach( Regex regex in regexes )
            {
                config.AddFileRegexToIgnore( regex );
            }

            // No directores are ignored.
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.ShouldIgnore( dir ), dir );
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        [Test]
        public void IgnoreFileExtensionTest()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir1_HelloHtml,
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir3_HiddenHtml
            };

            List<Regex> regexes = new List<Regex>
            {
                new Regex( @"\.html", RegexOptions.IgnoreCase ),
                new Regex( @"\.text" )
            };

            IgnoreConfig config = new IgnoreConfig();
            foreach( Regex regex in regexes )
            {
                config.AddIgnoredFileExtension( regex );
            }

            // No directores are ignored.
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.ShouldIgnore( dir ), dir );
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// For this test, whenever "dirx" shows up,
        /// ignore.  This will ignore dir1, dir2, and dir3.
        /// </summary>
        [Test]
        public void IgnoreDirectoryRegexTest1()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir1_HelloHtml,
                TestDirManager.Dir1_HelloXml,
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir2_MyfileTxt,
                TestDirManager.Dir2_Myfile2Txt,
                TestDirManager.Dir3_HiddenFile,
                TestDirManager.Dir3_HiddenHtml,
            };

            HashSet<string> ignoredDirs = new HashSet<string>
            {
                TestDirManager.Dir1,
                TestDirManager.Dir2,
                TestDirManager.Dir2_MyDir,
                TestDirManager.Dir3
            };

            IgnoreConfig config = new IgnoreConfig();
            config.AddDirNameToIgnore(
                @"dir\d",
                true
            );

            foreach( string dir in TestDirManager.DirList )
            {
                if( ignoredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// For this test, whenever "dir" shows up,
        /// ignore.  We have no directories labeled this,
        /// (they all have numbers at the end), so nothing should show up. 
        /// </summary>
        [Test]
        public void IgnoreDirectoryRegexTest2()
        {
            IgnoreConfig config = new IgnoreConfig();
            config.AddDirNameToIgnore(
                @"dir",
                true
            );

            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.ShouldIgnore( dir ), dir );
            }
            foreach( string file in TestDirManager.FileList )
            {
                Assert.IsFalse( config.ShouldIgnore( file ), file );
            }
        }

        /// <summary>
        /// For this test, whenever "DIRX" shows up,
        /// ignore ONLY if the casing matches.
        /// This will ignore dir2.
        /// </summary>
        [Test]
        public void IgnoreDirectoryRegexTest3()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir2_MyfileTxt,
                TestDirManager.Dir2_Myfile2Txt,
            };

            HashSet<string> ignoredDirs = new HashSet<string>
            {
                TestDirManager.Dir2,
                TestDirManager.Dir2_MyDir,
            };

            IgnoreConfig config = new IgnoreConfig();
            config.AddDirNameToIgnore(
                @"DIR\d",
                false
            );

            foreach( string dir in TestDirManager.DirList )
            {
                if( ignoredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// For this test, whenever "mydir" shows up,
        /// ignore.  This will ignore dir2/mydir.
        /// </summary>
        [Test]
        public void IgnoreDirectoryRegexTest4()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text
            };

            HashSet<string> ignoredDirs = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir
            };

            IgnoreConfig config = new IgnoreConfig();
            config.AddDirNameToIgnore(
                @"mydir",
                true
            );

            foreach( string dir in TestDirManager.DirList )
            {
                if( ignoredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// Ensures a root directory and its sub-directories
        /// are ignored.
        /// </summary>
        [Test]
        public void IgnoreSpecificDirectory1()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir2_MyfileTxt,
                TestDirManager.Dir2_Myfile2Txt,
            };

            HashSet<string> ignoredDirs = new HashSet<string>
            {
                TestDirManager.Dir2,
                TestDirManager.Dir2_MyDir
            };

            IgnoreConfig config = new IgnoreConfig();
            config.AddSpecificDirToIgnore( TestDirManager.Dir2 );

            foreach( string dir in TestDirManager.DirList )
            {
                if( ignoredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }

        /// <summary>
        /// Ensures a directory that's at the end of a tree
        /// and its contents get ignored.
        /// </summary>
        [Test]
        public void IgnoreSpecificDirectory2()
        {
            HashSet<string> ignoredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text
            };

            HashSet<string> ignoredDirs = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir
            };

            IgnoreConfig config = new IgnoreConfig();
            config.AddSpecificDirToIgnore( TestDirManager.Dir2_MyDir );

            foreach( string dir in TestDirManager.DirList )
            {
                if( ignoredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( ignoredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.ShouldIgnore( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.ShouldIgnore( file ), file );
                }
            }
        }
    }
}
