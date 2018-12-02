string target = Argument( "target", "build" );

Task( "build" )
.Does(
    () => 
    {
        DotNetCoreBuild( "./Filesing.sln" );
    }
);

Task( "unit_test" )
.Does(
    () =>
    {
        DotNetCoreTest( "./Filesing.Tests/Filesing.Tests.csproj" );
    }
);

RunTarget( target );

