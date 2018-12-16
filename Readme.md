Filesing
-------------

Filesing is a tool that can take in a configuration file and search a directory's contents for files that match regex(s) specified.  The configuration file can also specify which directories and/or files to ignore while searching.

Filesing's name is derived from a "[dowsing](https://en.wikipedia.org/wiki/Dowsing)" device... but instead of finding water, it searchs files for text that matches a list of regexes.

Build Status
------
[![Build status](https://ci.appveyor.com/api/projects/status/a8ghu6tqt2v2gb7d?svg=true)](https://ci.appveyor.com/project/xforever1313/filesing) [![Codacy Badge](https://api.codacy.com/project/badge/Grade/25263a32d625446fbe0dc19afa671b13)](https://www.codacy.com/app/xforever1313/Filesing?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=xforever1313/Filesing&amp;utm_campaign=Badge_Grade)

Packages
------
[![NuGet](https://img.shields.io/nuget/v/Filesing.svg)](https://www.nuget.org/packages/Filesing/) [![Chocolatey](https://img.shields.io/chocolatey/v/filesing.portable.svg)](https://chocolatey.org/packages/filesing.portable/)

Installing
------
You can install Filesing from [Chocolatey](https://chocolatey.org/packages/filesing.portable) via ```choco install filesing.portable```.  This puts filesing.exe on the path,
so you can call it from the command line.

You can also choose to install Filesing from [NuGet](https://www.nuget.org/packages/Filesing/) by running on a command line ```nuget install filesing -ExcludeVersion```.  This will install filesing in the current directory in a folder named "Filesing".  Inside of Filesing will contain all of the dlls and ```filesing.bat``` and ```filesing```.  On Windows, run ```filesing.bat``` to launch filesing.  On Unix systems, running ```filesing``` will launch filesing.

The difference between the Chocolatey and the NuGet package is Chocolatey includes everything needed from the Dotnet core runtime, NuGet does not.  Therefore, with the NuGet package, you must have the [dotnet core runtime](https://dotnet.microsoft.com/download) installed on the PC, and the dotnet executable must be on the PATH environmental variable.  You do not need this installed with the Chocolatey package, since its included with it.  The Chocolatey package is also Windows only, the NuGet package is not.

Usage
------

Run ```filesing``` on the command line in order to invoke it.  Here are the the command line arguments:
```
  -h, --help                 Shows this message and exits.
      --version              Shows the version and exits.
      --license              Shows the license information and exits.
  -f, --configfile=VALUE     The input file to determine which patterns to
                               search for.  Required if regex is not specified.
  -d, --searchdir=VALUE      The directory to search for files that match the
                               patterns.  Required.
  -j, --numthreads=VALUE     The number of threads to use.  0 for the processor
                               count. Defaulted to 1.
  -v, --verbosity=VALUE      How verbose the output should be.  Levels are 0, 1,
                                2, and 3.  0 (the default) or less prints the
                               least, 3 or more prints the most.
  -e, --exit_code_on_find=VALUE
                             If ANY matches are found, what the exit code
                               should be.  Defaulted to 0.
  -r, --regex=VALUE          Searches the diretory for this regex.  If a config
                               file is specified as well, the global ignore/
                               require settings in the config file are applied
                               when searching for this regex. Optional (uses
                               regexes in config file if not specified).
```

```searchdir``` is the directory to search.  Filesing will search the directory recursively for files whose path and/or contents match any of the specified regex(es).

```configfile``` is the configuration file to use.  The file contains the list of regexes to use while searching along with any files or directories to ignore.  An example of the config file can be found [here](https://raw.githubusercontent.com/xforever1313/Filesing/master/Documentation/SampleFilesingConfig.xml).  This argument is required if the ```regex``` command is not specified.  If ```regex``` is used with this command, all regexes in the config file are ignored, but global settings about files to ignore and files to search for are still used.

```regex``` is the regex to use while searching the directory.  If this argument is not specified, ```configfile``` is required, as filesing uses the config file to get the regexes to use.

```exit_code_on_find``` if ANY matches are found, filesing will exit with this exit code.  This can be useful in CI environments where if you DON'T want something committed for some reason, but it is found in the repository, you can fail the build.
