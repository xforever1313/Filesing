#!/bin/bash

dotnet $(dirname "$0")/Filesing.Cli.dll "$@"

exit $?
