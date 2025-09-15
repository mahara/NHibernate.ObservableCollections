@ECHO OFF


:INITIALIZE_BUILD_PARAMETERS
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
IF "%version%"=="" GOTO SET_FOLDERS
SET BUILD_VERSION=%version%

GOTO SET_FOLDERS


:SET_FOLDERS
SET ARTIFACTS_FOLDER_NAME=artifacts
SET ARTIFACTS_OUTPUT_FOLDER_NAME=bin
SET ARTIFACTS_PACKAGES_FOLDER_NAME=packages
SET ARTIFACTS_TEST_RESULTS_FOLDER_NAME=testresults

SET ARTIFACTS_FOLDER_PATH=%ARTIFACTS_FOLDER_NAME%

SET ARTIFACTS_OUTPUT_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_OUTPUT_FOLDER_NAME%
SET ARTIFACTS_OUTPUT_BUILD_CONFIGURATION_FOLDER_PATH=%ARTIFACTS_OUTPUT_FOLDER_PATH%\%BUILD_CONFIGURATION%

SET ARTIFACTS_PACKAGES_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_PACKAGES_FOLDER_NAME%\%BUILD_CONFIGURATION%

SET ARTIFACTS_TEST_RESULTS_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_TEST_RESULTS_FOLDER_NAME%\%BUILD_CONFIGURATION%

GOTO BUILD


:BUILD

ECHO ----------------------------------------------------
ECHO Building "%BUILD_CONFIGURATION%" packages with version "%BUILD_VERSION%"...
ECHO ----------------------------------------------------

dotnet build "NHibernate.ObservableCollections-All.sln" --configuration %BUILD_CONFIGURATION% --property:PACKAGE_VERSION=%BUILD_VERSION% || EXIT /B 1

dotnet build "tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln" --configuration Release
SET NEV_COMMAND="%ARTIFACTS_FOLDER_PATH%\tools\nev\nev.exe" "%ARTIFACTS_PACKAGES_FOLDER_PATH%\" "NHibernate.ObservableCollections."
ECHO %NEV_COMMAND%
%NEV_COMMAND%

GOTO TEST


:TEST

REM https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test
REM https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-vstest
REM https://github.com/Microsoft/vstest-docs/blob/main/docs/report.md
REM https://github.com/spekt/nunit.testlogger/issues/56

ECHO ------------------------------------
ECHO Running .NET (net8.0) Unit Tests
ECHO ------------------------------------

dotnet test "%ARTIFACTS_OUTPUT_BUILD_CONFIGURATION_FOLDER_PATH%\net8.0\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.dll" --results-directory "%ARTIFACTS_TEST_RESULTS_FOLDER_PATH%" --logger "nunit;LogFileName=NHibernate.ObservableCollections_net8.0_TestResults.xml;format=nunit3"

ECHO ------------------------------------
ECHO Running .NET (net7.0) Unit Tests
ECHO ------------------------------------

dotnet test "%ARTIFACTS_OUTPUT_BUILD_CONFIGURATION_FOLDER_PATH%\net7.0\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.dll" --results-directory "%ARTIFACTS_TEST_RESULTS_FOLDER_PATH%" --logger "nunit;LogFileName=NHibernate.ObservableCollections_net7.0_TestResults.xml;format=nunit3"

ECHO ------------------------------------
ECHO Running .NET (net6.0) Unit Tests
ECHO ------------------------------------

dotnet test "%ARTIFACTS_OUTPUT_BUILD_CONFIGURATION_FOLDER_PATH%\net6.0\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.dll" --results-directory "%ARTIFACTS_TEST_RESULTS_FOLDER_PATH%" --logger "nunit;LogFileName=NHibernate.ObservableCollections_net6.0_TestResults.xml;format=nunit3"

ECHO --------------------------------------------
ECHO Running .NET Framework (net48) Unit Tests
ECHO --------------------------------------------

dotnet test "%ARTIFACTS_OUTPUT_BUILD_CONFIGURATION_FOLDER_PATH%\net48\NHibernate.ObservableCollections.Tests\NHibernate.ObservableCollections.Tests.exe" --results-directory "%ARTIFACTS_TEST_RESULTS_FOLDER_PATH%" --logger "nunit;LogFileName=NHibernate.ObservableCollections_net48_TestResults.xml;format=nunit3"
