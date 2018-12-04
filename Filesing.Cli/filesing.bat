@echo off

dotnet.exe %~p0\filesing.Cli.dll %*

exit %errorlevel%
