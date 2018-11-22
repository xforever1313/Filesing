//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using Filesing.Api;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Filesing.Tests
{
    [TestFixture]
    public class FilesingConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        // ---------------- Tests ----------------

        [Test]
        public void ValidateTest()
        {
            // Make a valid one first.
            FilesingConfig uut = new FilesingConfig();
            uut.PatternConfigs.Add( new PatternConfig( new Regex( "Hello" ) ) );
            uut.SearchDirectoryLocation = "./";

            Assert.DoesNotThrow( () => uut.Validate() );

            // Start making things not valid.
            
            // Directory can not be null, empty, whitespace.
            uut.SearchDirectoryLocation = null;
            Assert.Throws<ValidationException>( () => uut.Validate() );
            uut.SearchDirectoryLocation = string.Empty;
            Assert.Throws<ValidationException>( () => uut.Validate() );
            uut.SearchDirectoryLocation = "      ";
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // Directory must exist.
            uut.SearchDirectoryLocation = "./lol";
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.SearchDirectoryLocation = "./";

            // Number of threads can not be negative.
            uut.NumberOfThreads = -1;
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // Patterns can not be empty.
            uut.PatternConfigs.Clear();
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }
    }
}