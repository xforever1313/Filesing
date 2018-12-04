string target = Argument( "target", "build" );

const string version = "1.0.0";
const string makeRelaseTarget = "make_release";

bool isRelease = ( target == makeRelaseTarget );

DotNetCoreMSBuildSettings msBuildSettings = new DotNetCoreMSBuildSettings();
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

