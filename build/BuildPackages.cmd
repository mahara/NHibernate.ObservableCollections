@ECHO OFF


SET %1
SET %2

SET BUILD_CONFIGURATION=Release
SET BUILD_VERSION=1.0.0

GOTO SET_BUILD_CONFIGURATION


:SET_BUILD_CONFIGURATION
IF "%configuration%"=="" GOTO SET_BUILD_VERSION
SET BUILD_CONFIGURATION=%configuration%

GOTO SET_BUILD_VERSION


:SET_BUILD_VERSION
IF "%version%"=="" GOTO BUILD
SET BUILD_VERSION=%version%

GOTO BUILD


:BUILD

ECHO ----------------------------------------------------
ECHO Building "%BUILD_CONFIGURATION%" packages with version "%BUILD_VERSION%"...
ECHO ----------------------------------------------------

dotnet build ".\NHibernate.ObservableCollections.sln" --configuration %BUILD_CONFIGURATION% -property:APPVEYOR_BUILD_VERSION=%BUILD_VERSION% || EXIT /B 1

dotnet build ".\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln" --configuration Release || EXIT /B 1
".\tools\Explicit.NuGet.Versions\bin\nev.exe" ".\bin" "NHibernate.ObservableCollections." || EXIT /B 1

GOTO TEST


:TEST

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
