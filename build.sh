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
		echo "Please install either Docker or Xamarin/Mono from https://www.mono-project.com/docs/getting-started/install/"
		exit 1
	fi
fi

mono --version

# Linux/Darwin
OSNAME=$(uname -s)
echo "OSNAME: $OSNAME"

dotnet build ".\NHibernate.ObservableCollections.sln" --configuration Release || exit 1

echo ------------------------------------
echo Running .NET (net6.0) Unit Tests
echo ------------------------------------

dotnet test ".\bin\Release\net6.0\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.dll" --results-directory ".\bin\Release" --logger "nunit;LogFileName=NHibernate.ObservableCollections.Tests-Net-TestResults.xml;format=nunit3" || exit 1

echo --------------------------------------------
echo Running .NET Framework (net48) Unit Tests
echo --------------------------------------------

mono ".\bin\Release\net48\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.exe" --work ".\bin\Release" --result "NHibernate.ObservableCollections.Tests-NetFramework-TestResults.xml;format=nunit3" || exit 1

# Ensure that all unit test runs produced protocol files.
if [[ !( -f ".\bin\Release\NHibernate.ObservableCollections.Tests-Net-TestResults.xml" &&
         -f ".\bin\Release\NHibernate.ObservableCollections.Tests-NetFramework-TestResults.xml" ) ]]; then
    echo "Incomplete test results. Some test runs might not have terminated properly. Failing the build."

    exit 1
fi

# Unit Test Failures
NET_FAILCOUNT=$(grep -F "One or more child tests had errors." ".\bin\Release\NHibernate.ObservableCollections.Tests-Net-TestResults.xml" | wc -l)
if [ $NET_FAILCOUNT -ne 0 ]
then
    echo ".NET (net6.0) Unit Tests have failed, failing the build."

    exit 1
fi

NETFRAMEWORK_FAILCOUNT=$(grep -F "One or more child tests had errors." ".\bin\Release\NHibernate.ObservableCollections.Tests-NetFramework-TestResults.xml" | wc -l)
if [ $NETFRAMEWORK_FAILCOUNT -ne 0 ]
then
    echo ".NET Framework (net48) Unit Tests have failed, failing the build."

    exit 1
fi
