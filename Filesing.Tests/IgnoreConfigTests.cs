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
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir3_HiddenHtml
            };

            List<string> extensions = new List<string>
            {
                ".html",
                ".text"
            };

            IgnoreConfig config = new IgnoreConfig();
            foreach( string extension in extensions )
            {
                config.AddIgnoredFileExtension( extension );
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
    }
}
