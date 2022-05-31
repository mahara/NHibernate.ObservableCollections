#!/bin/bash


shopt -s expand_aliases

DOTNETPATH=$(which dotnet)
if [ ! -f "$DOTNETPATH" ]; then
	echo "Please install Microsoft .NET from: https://dotnet.microsoft.com/en-us/download"
	exit 1
fi

DOCKERPATH=$(which docker)
if [ -f "$DOCKERPATH" ]; then
	alias mono="$PWD/build/docker-run-mono.sh"
else
	MONOPATH=$(which mono)
	if [ ! -f "$MONOPATH" ]; then
		echo "Please install either Docker, or Xamarin/Mono from https://www.mono-project.com/docs/getting-started/install/"
		exit 1
	fi
fi

mono --version

# Linux/Darwin
OSNAME=$(uname -s)
echo "OSNAME: $OSNAME"

dotnet build --configuration Release || exit 1

echo ----------------------------
echo Running .NET (net6.0) Tests
echo ----------------------------

dotnet ./src/Framework.Tests/bin/Release/net6.0/Framework.Tests.dll --result=FrameworkNetTestResults.xml;format=nunit3

echo ------------------------------------
echo Running .NET Framework (net48) Tests
echo ------------------------------------

mono ./src/Framework.Tests/bin/Release/net48/Framework.Tests.exe --result=FrameworkNetFrameworkTestResults.xml;format=nunit3

# Ensure that all test runs produced protocol files.
if [[ !( -f FrameworkNetTestResults.xml &&
         -f FrameworkNetFrameworkTestResults.xml ) ]]; then
    echo "Incomplete test results. Some test runs might not have terminated properly. Failing the build."
    exit 1
fi

# Unit test failure.
NET_FAILCOUNT=$(grep -F "One or more child tests had errors" FrameworkNetTestResults.xml | wc -l)
if [ $NET_FAILCOUNT -ne 0 ]
then
    echo ".NET (net6.0) Tests have failed, failing the build"
    exit 1
fi

NETFRAMEWORK_FAILCOUNT=$(grep -F "One or more child tests had errors" FrameworkNetFrameworkTestResults.xml | wc -l)
if [ $NETFRAMEWORK_FAILCOUNT -ne 0 ]
then
    echo ".NET Framework (net48) Tests have failed, failing the build"
    exit 1
fi
