@ECHO OFF


IF "%1" == "" GOTO no_config
IF "%1" NEQ "" GOTO set_config

:set_config
SET BUILD_CONFIGURATION=%1
GOTO build

:no_config
SET BUILD_CONFIGURATION=Release
GOTO build

:build
dotnet build ".\NHibernate.ObservableCollections.sln" --configuration %BUILD_CONFIGURATION% || EXIT /B 1

dotnet build ".\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln" --configuration Release || EXIT /B 1
".\tools\Explicit.NuGet.Versions\bin\nev.exe" ".\bin" "NHibernate.ObservableCollections." || EXIT /B 1
GOTO test

:test

REM https://github.com/Microsoft/vstest-docs/blob/main/docs/report.md
REM https://github.com/spekt/nunit.testlogger/issues/56

ECHO ------------------------------------
ECHO Running .NET (net6.0) Unit Tests
ECHO ------------------------------------

dotnet test ".\bin\%BUILD_CONFIGURATION%\net6.0\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.dll" --results-directory ".\bin\%BUILD_CONFIGURATION%" --logger "nunit;LogFileName=NHibernate.ObservableCollections.Tests-Net-TestResults.xml;format=nunit3" || EXIT /B 1

ECHO --------------------------------------------
ECHO Running .NET Framework (net48) Unit Tests
ECHO --------------------------------------------

dotnet test ".\bin\%BUILD_CONFIGURATION%\net48\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.exe" --results-directory ".\bin\%BUILD_CONFIGURATION%" --logger "nunit;LogFileName=NHibernate.ObservableCollections.Tests-NetFramework-TestResults.xml;format=nunit3" || EXIT /B 1
