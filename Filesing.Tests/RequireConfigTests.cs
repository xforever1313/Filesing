//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Filesing.Api;
using NUnit.Framework;

namespace Filesing.Tests
{
    [TestFixture]
    public class RequireConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        /// <summary>
        /// If nothing is specified, nothing should be marked to be required.
        /// </summary>
        [Test]
        public void NothingSpecifiedTest()
        {
            RequireConfig config = new RequireConfig();
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.IsRequired( dir ) );
            }
            foreach( string file in TestDirManager.FileList )
            {
                Assert.IsFalse( config.IsRequired( file ) );
            }
        }

        /// <summary>
        /// Ensures the specific file check is working correctly.
        /// </summary>
        [Test]
        public void RequireSpecificFilesTest()
        {
            HashSet<string> requiredFiles = new HashSet<string>
            {
                TestDirManager.HiddenDir_HelloTxt,
                TestDirManager.Dir1_HelloHtml,
                TestDirManager.Dir2_MyDir,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir3_HiddenHtml,
                TestDirManager.RootFile1
            };

            RequireConfig config = new RequireConfig();
            foreach( string file in requiredFiles )
            {
                config.AddRequiredFile( file );
            }

            // No directores are required.
            foreach( string dir in TestDirManager.DirList )
            {
                Assert.IsFalse( config.IsRequired( dir ), dir );
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( requiredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.IsRequired( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.IsRequired( file ), file );
                }
            }
        }

        /// <summary>
        /// Ensures a root directory and its sub-directories
        /// are required.
        /// </summary>
        [Test]
        public void RequireSpecificDirectory1()
        {
            HashSet<string> requiredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text,
                TestDirManager.Dir2_MyfileTxt,
                TestDirManager.Dir2_Myfile2Txt,
            };

            HashSet<string> requiredDirs = new HashSet<string>
            {
                TestDirManager.Dir2,
                TestDirManager.Dir2_MyDir
            };

            RequireConfig config = new RequireConfig();
            config.AddRequiredDir( TestDirManager.Dir2 );

            foreach( string dir in TestDirManager.DirList )
            {
                if( requiredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.IsRequired( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.IsRequired( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( requiredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.IsRequired( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.IsRequired( file ), file );
                }
            }
        }

        /// <summary>
        /// Ensures a directory that's at the end of a tree
        /// and its contents get required.
        /// </summary>
        [Test]
        public void RequireSpecificDirectory2()
        {
            HashSet<string> requiredFiles = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir_Myfile1Text,
                TestDirManager.Dir2_MyDir_Myfile2Text
            };

            HashSet<string> requiredDirs = new HashSet<string>
            {
                TestDirManager.Dir2_MyDir
            };

            RequireConfig config = new RequireConfig();
            config.AddRequiredDir( TestDirManager.Dir2_MyDir );

            foreach( string dir in TestDirManager.DirList )
            {
                if( requiredDirs.Contains( dir ) )
                {
                    Assert.IsTrue( config.IsRequired( dir ), dir );
                }
                else
                {
                    Assert.IsFalse( config.IsRequired( dir ), dir );
                }
            }
            foreach( string file in TestDirManager.FileList )
            {
                if( requiredFiles.Contains( file ) )
                {
                    Assert.IsTrue( config.IsRequired( file ), file );
                }
                else
                {
                    Assert.IsFalse( config.IsRequired( file ), file );
                }
            }
        }
    }
}
