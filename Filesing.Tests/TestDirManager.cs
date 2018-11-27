//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Filesing.Tests
{
    [AttributeUsage( AttributeTargets.Property )]
    public class DirectoryAttribute : Attribute
    {
    }

    [AttributeUsage( AttributeTargets.Property )]
    public class FileAttribute : Attribute
    {
    }

    public static class TestDirManager
    {
        // ---------------- Constructor ----------------

        static TestDirManager()
        {
            List<string> dirList = new List<string>();
            List<string> fileList = new List<string>();

            PropertyInfo[] props = typeof( TestDirManager ).GetProperties();
            foreach( PropertyInfo prop in props )
            {
                FileAttribute fileAttribute = prop.GetCustomAttribute<FileAttribute>();
                if( fileAttribute != null )
                {
                    fileList.Add( prop.GetValue( prop ).ToString() );
                }

                DirectoryAttribute directoryAttribute = prop.GetCustomAttribute<DirectoryAttribute>();
                if( directoryAttribute != null )
                {
                    dirList.Add( prop.GetValue( prop ).ToString() );
                }
            }

            DirList = dirList.AsReadOnly();
            FileList = fileList.AsReadOnly();
        }

        // ---------------- Properties ----------------

        public static IReadOnlyList<string> DirList { get; private set; }

        public static IReadOnlyList<string> FileList { get; private set; }

        // -------- Root Dir --------

        /// <summary>
        /// Root Directory of the test directory.
        /// </summary>
        [Directory]
        public static string TestDir => Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", // Up to Debug
            "..", // Up to bin
            "..", // Up to Filesing.Tests
            "TestDir"
        );

        [File]
        public static string RootFile1 => Path.Combine(
            TestDir,
            "rootfile1.txt"
        );

        [File]
        public static string RootFile2 => Path.Combine(
            TestDir,
            "RootFile1.txt"
        );

        // -------- Hidden Dir --------

        [Directory]
        public static string HiddenDir => Path.Combine(
            TestDir,
            ".hiddenDir"
        );

        [File]
        public static string HiddenDir_HelloTxt => Path.Combine(
            HiddenDir,
            "hello.txt"
        );

        [File]
        public static string HiddenDir_Hello2Txt => Path.Combine(
            HiddenDir,
            "hello2.txt"
        );

        // -------- Dir 1 --------

        [Directory]
        public static string Dir1 => Path.Combine(
            TestDir,
            "Dir1"
        );

        [File]
        public static string Dir1_HelloHtml => Path.Combine(
            Dir1,
            "Hello.html"
        );

        [File]
        public static string Dir1_HelloXml => Path.Combine(
            Dir1,
            "Hello.xml"
        );

        // -------- Dir 2 --------

        [Directory]
        public static string Dir2 => Path.Combine(
            TestDir,
            "DIR2"
        );

        [File]
        public static string Dir2_MyfileTxt => Path.Combine(
            Dir2,
            "myfile.txt"
        );

        [File]
        public static string Dir2_Myfile2Txt => Path.Combine(
            Dir2,
            "myFile2.txt"
        );

        // ---- My Dir ----

        [Directory]
        public static string Dir2_MyDir => Path.Combine(
            Dir2,
            "mydir"
        );

        [File]
        public static string Dir2_MyDir_Myfile1Text => Path.Combine(
            Dir2_MyDir,
            "myfile1.text"
        );

        [File]
        public static string Dir2_MyDir_Myfile2Text => Path.Combine(
            Dir2_MyDir,
            "myFile2.text"
        );

        // -------- Dir 3 --------

        [Directory]
        public static string Dir3 => Path.Combine(
            TestDir,
            "dir3"
        );

        [File]
        public static string Dir3_HiddenFile => Path.Combine(
            Dir3,
            ".hiddenfile"
        );

        [File]
        public static string Dir3_HiddenHtml => Path.Combine(
            Dir3,
            ".html"
        );
    }

    /// <summary>
    /// Sanity check to make sure all of the above files exist.
    /// </summary>
    [TestFixture]
    public class TestDirManagerTests
    {
        [Test]
        public void SanityCheckTest()
        {
            PropertyInfo[] props = typeof( TestDirManager ).GetProperties();
            foreach( PropertyInfo prop in props )
            {
                FileAttribute fileAttribute = prop.GetCustomAttribute<FileAttribute>();
                if( fileAttribute != null )
                {
                    string fileLocation = prop.GetValue( prop ).ToString();
                    if( File.Exists( fileLocation ) == false )
                    {
                        Assert.Fail( "Can not find file '{0}'", fileLocation );
                    }
                }

                DirectoryAttribute directoryAttribute = prop.GetCustomAttribute<DirectoryAttribute>();
                if( directoryAttribute != null )
                {
                    string dirLocation = prop.GetValue( prop ).ToString();
                    if( Directory.Exists( dirLocation ) == false )
                    {
                        Assert.Fail( "Can not find dir '{0}'", dirLocation );
                    }
                }
            }
        }
    }
}
