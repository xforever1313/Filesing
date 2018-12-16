string target = Argument( "target", "build" );

const string version = "1.0.0"; // This is the version of filesing.  Update before releasing.
const string makeRelaseTarget = "make_release";

bool isRelease = ( target == makeRelaseTarget );

DotNetCoreMSBuildSettings msBuildSettings = new DotNetCoreMSBuildSettings();

// Sets filesing's assembly version.
msBuildSettings.WithProperty( "Version", version )
    .WithProperty( "AssemblyVersion", version )
    .SetMaxCpuCount( System.Environment.ProcessorCount )
    .WithProperty( "FileVersion", version );

string configuration;
string packageOutput;
if ( isRelease )
{
    configuration = "Release";
    packageOutput = "./dist/Release";
}
else
{
    configuration = "Debug";
    packageOutput = "./dist/Debug";
}

packageOutput = MakeAbsolute( new FilePath( packageOutput ) ).FullPath;
msBuildSettings.SetConfiguration( configuration );

Task( "build" )
.Does(
    () => 
    {
        DotNetCoreBuildSettings settings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = msBuildSettings
        };
        DotNetCoreBuild( "./Filesing.sln", settings );
    }
)
.Description( "Compiles Filesing." );

Task( "unit_test" )
.Does(
    () =>
    {
		DotNetCoreTestSettings settings = new DotNetCoreTestSettings
		{
			NoBuild = true,
			NoRestore = true,
            Configuration = configuration
		};
        DotNetCoreTest( "./Filesing.Tests/Filesing.Tests.csproj", settings );
    }
)
.IsDependentOn( "build" )
.Description( "Runs Filesing's Tests." );

Task( "make_dist" )
.Does(
    () =>
    {
        CleanDirectories( packageOutput );

        DotNetCorePublishSettings settings = new DotNetCorePublishSettings
        {
            OutputDirectory = packageOutput,
            NoBuild = true,
			NoRestore = true,
            Configuration = configuration
        };

        DotNetCorePublish( "./Filesing.Cli/Filesing.Cli.csproj", settings );
        CopyFile( "./LICENSE_1_0.txt", System.IO.Path.Combine( packageOutput, "License.txt" ) );
        CopyFileToDirectory( "./Filesing.Cli/filesing.bat", packageOutput );
        CopyFileToDirectory( "./Filesing.Cli/filesing", packageOutput );
        CopyFileToDirectory( "./Readme.md", packageOutput );
    }
)
.IsDependentOn( "unit_test" )
.Description( "Copies filesing into a distribution folder." );

Task( "pack_nuget" )
.Does(
    () =>
    {
        NuGetPackSettings settings = new NuGetPackSettings
        {
            Version = version,
            BasePath = packageOutput,
            OutputDirectory = packageOutput,
            Symbols = false,
            // This surpresses warnings such as '.dll' is not inside the 'lib' folder and blah blah blah.
            NoPackageAnalysis = true,
            // Cake's build.cake does this.  It takes all of the files in the dist folder
            // and adds them to the nuget package.  It:
            // 1. Grabs all of the files
            // 2. Gets the full path of the files
            // 3. Removes the full path of the file, so just the file name remains (plus the subdirectory in the dist folder)
            // 4. Creates a NuSpecContent object, which the Files property requires.
            Files = GetFiles( System.IO.Path.Combine( packageOutput, "*" ) )
                        .Select( file => file.FullPath.Substring( packageOutput.Length + 1 ) )
                        .Select( file => new NuSpecContent { Source = file, Target = file } )
                        .ToArray()
        };

        NuGetPack( "./nuspec/Filesing.nuspec", settings );
    }
)
.IsDependentOn( "make_dist" )
.Description( "Creates Filesing's NuGet package." );

Task( "pack_choco" )
.Does(
    () =>
    {
        string winOutput = System.IO.Path.Combine(
            System.IO.Directory.GetCurrentDirectory(),
            "dist/Win-Release"
        );

        CleanDirectories( winOutput );

        DotNetCorePublishSettings winSettings = new DotNetCorePublishSettings
        {
            OutputDirectory = winOutput,
            Configuration = "Release",
            Runtime = "win-x64",
            MSBuildSettings = msBuildSettings,
            NoBuild = false,
            NoRestore = false
        };

        // Yes, this will override the global msbuild settings since they
        // are the same reference, but chocolatey is the last thing to get
        // built (as everything else is built before unit_test, which is what
        // this task depends on).  So, changing this property *shouldn't* matter.
        winSettings.MSBuildSettings.WithProperty( "TrimUnusedDependencies", "true" );

        DotNetCorePublish( "./Filesing.Cli/Filesing.Cli.csproj", winSettings );
        CopyFileToDirectory( "./Readme.md", winOutput );
        CopyFile( "./LICENSE_1_0.txt", System.IO.Path.Combine( winOutput, "License.txt" ) );
        CopyFileToDirectory( "./nuspec/VERIFICATION.txt", winOutput );

        // Sanity check, make sure our exe is NOT Filesing.Cli.exe, but simply Filesing.exe.
        if ( FileExists( System.IO.Path.Combine( winOutput, "Filesing.exe" ) ) == false )
        {
            throw new FileNotFoundException(
                "Filesing.exe was not found in chocolatey's dist folder.  Is it still named Filesing.Cli.exe?  Check the cakefiles and the CLI .csproj."
            );
        }

        ChocolateyPackSettings settings = new ChocolateyPackSettings
        {
            Version = version,
            OutputDirectory = winOutput,
            // Cake's build.cake does this.  It takes all of the files in the dist folder
            // and adds them to the nuget package.  It:
            // 1. Grabs all of the files
            // 2. Gets the full path of the files
            // 3. Removes the full path of the file, so just the file name remains (plus the subdirectory in the dist folder)
            // 4. Creates a ChocolateyNuSpecContent  object, which the Files property requires.
            Files = GetFiles( System.IO.Path.Combine( winOutput, "*" ) )
                        .Select( file => file.FullPath.Substring( winOutput.Length + 1 ) )
                        .Select( file => new ChocolateyNuSpecContent  { Source = System.IO.Path.Combine( winOutput, file ) } )
                        .ToArray()
        };

        ChocolateyPack( "./nuspec/Filesing.Portable.nuspec", settings );
    }
)
.IsDependentOn( "unit_test" )
.WithCriteria( Environment.OSVersion.Platform == PlatformID.Win32NT )
.Description( "Create's Filesing's Chocolatey package.  Windows only." );

Task( makeRelaseTarget )
.Does(
    () =>
    {
    }
)
.IsDependentOn( "pack_nuget" )
.IsDependentOn( "pack_choco" )
.Description( "Creates a release." );

RunTarget( target );
