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
        CopyFileToDirectory( "./LICENSE_1_0.txt", packageOutput );
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

Task( makeRelaseTarget )
.Does(
    () =>
    {

    }
)
.IsDependentOn( "pack_nuget" )
.Description( "Creates a release." );

RunTarget( target );

