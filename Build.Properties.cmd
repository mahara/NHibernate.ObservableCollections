@ECHO OFF

REM ================
REM CONVENTIONS
REM ================
REM -   All "*FOLDER_PATH" variables must not have a trailing backslash (a folder/directory separator).
REM     This is different from the convention used for MSBuild folder/directory properties.
REM     -   %~dp0 has a trailing backslash, so it needs to be removed by using this code:
REM             SET WORKSPACE_FOLDER_PATH=%WORKSPACE_FOLDER_PATH:~0,-1%
REM     -   When building folder/directory paths by combining path segments,
REM         a backslash must be added between them, for example:
REM             SET BUILD_CONFIGURATION_FOLDER_PATH=%WORKSPACE_FOLDER_PATH%\%BUILD_CONFIGURATION_FOLDER_NAME%
REM     -   When passing a double-quoted folder/directory path as an argument to an external process,
REM         the trailing backslash must be removed, or not explicitly specified, because it would create issues
REM         with legacy Windows command-line argument parsing behavior, for example:
REM             SET PACKAGE_NEV_COMMAND="!ARTIFACTS_FOLDER_PATH!\tools\nev\nev.exe" "!ARTIFACTS_PACKAGES___CONFIGURATION___FOLDER_PATH!" "!PACKAGE_NEV_PARAMETER___DEPENDENCY_PACKAGE_ID_PREFIX_FILTER!"
REM         Adding a backslash at the end of "!ARTIFACTS_PACKAGES___CONFIGURATION___FOLDER_PATH!"
REM         would create a trailing \" sequence and cause the external process to receive
REM         one merged argument like 'C:\...\Release" Iesi.', instead of two separate arguments.
REM -   All "*FOLDER_PATH" variables defined below are base/root folder/directory paths,
REM     even though their variable names don't use the word "BASE".


SET BUILD_CONFIGURATION_FOLDER_NAME=build
SET SOURCE_CODE_FOLDER_NAME=src
SET ARTIFACTS_FOLDER_NAME=artifacts
SET ARTIFACTS_OUTPUT_FOLDER_NAME=bin
SET ARTIFACTS_PACKAGES_FOLDER_NAME=packages
SET ARTIFACTS_TEST_RESULTS_FOLDER_NAME=testresults

SET WORKSPACE_FOLDER_PATH=%~dp0
SET WORKSPACE_FOLDER_PATH=%WORKSPACE_FOLDER_PATH:~0,-1%
SET BUILD_CONFIGURATION_FOLDER_PATH=%WORKSPACE_FOLDER_PATH%\%BUILD_CONFIGURATION_FOLDER_NAME%
SET SOURCE_CODE_FOLDER_PATH=%WORKSPACE_FOLDER_PATH%\%SOURCE_CODE_FOLDER_NAME%
SET ARTIFACTS_FOLDER_PATH=%WORKSPACE_FOLDER_PATH%\%ARTIFACTS_FOLDER_NAME%
SET ARTIFACTS_OUTPUT_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_OUTPUT_FOLDER_NAME%
SET ARTIFACTS_PACKAGES_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_PACKAGES_FOLDER_NAME%
SET ARTIFACTS_TEST_RESULTS_FOLDER_PATH=%ARTIFACTS_FOLDER_PATH%\%ARTIFACTS_TEST_RESULTS_FOLDER_NAME%


SET PARAMETER___RUN_BUILD___DEFAULT=true
SET PARAMETER___RUN_TEST___DEFAULT=true
SET PARAMETER___RUN_PACKAGE___DEFAULT=true

SET BUILD_PARAMETER___VERSION___DEFAULT=6.0.0
SET BUILD_PARAMETER___CONFIGURATION___DEFAULT=Release
SET BUILD_PARAMETER___ENABLE_SOURCE_LINK___DEFAULT=true
SET BUILD_PARAMETER___FRAMEWORKS___DEFAULT=net10.0;net9.0;net8.0;net48

SET TEST_PARAMETER___FRAMEWORKS___DEFAULT=net10.0;net9.0;net8.0;net48


REM ==================
REM BUILD UNITS
REM ==================
REM Build units are executed in the order defined below.
REM Each build unit completes build -> test -> package before the next build unit starts.

SET BUILD_UNITS=^
    "IESI_OBSERVABLE_COLLECTIONS";^
    "NHIBERNATE_OBSERVABLE_COLLECTIONS"


REM ==================
REM BUILD UNIT: IESI_OBSERVABLE_COLLECTIONS
REM ==================

SET BUILD_UNIT___IESI_OBSERVABLE_COLLECTIONS___BUILD_PARAMETERS=^
    "Iesi.ObservableCollections.csproj|%BUILD_PARAMETER___FRAMEWORKS___DEFAULT%";^
    "Iesi.ObservableCollections.Tests.csproj|%BUILD_PARAMETER___FRAMEWORKS___DEFAULT%";^
    "Iesi.ObservableCollections.PerformanceTests.csproj|%BUILD_PARAMETER___FRAMEWORKS___DEFAULT%"

SET BUILD_UNIT___IESI_OBSERVABLE_COLLECTIONS___TEST_PARAMETERS=^
    "Iesi.ObservableCollections.Tests.csproj|%TEST_PARAMETER___FRAMEWORKS___DEFAULT%";^
    "Iesi.ObservableCollections.Tests.dll|%TEST_PARAMETER___FRAMEWORKS___DEFAULT%"

SET BUILD_UNIT___IESI_OBSERVABLE_COLLECTIONS___PACKAGE_PARAMETERS=^
    "Iesi.ObservableCollections.csproj"

REM Not needed here because Iesi.ObservableCollections.nupkg
REM does not have dependencies on packages whose IDs start with "Iesi.".
REM SET BUILD_UNIT___IESI_OBSERVABLE_COLLECTIONS___PACKAGE_NEV_PARAMETERS=^
REM     "Iesi."


REM ==================
REM BUILD UNIT: NHIBERNATE_OBSERVABLE_COLLECTIONS
REM ==================

SET BUILD_UNIT___NHIBERNATE_OBSERVABLE_COLLECTIONS___BUILD_PARAMETERS=^
    "NHibernate.ObservableCollections.csproj|%BUILD_PARAMETER___FRAMEWORKS___DEFAULT%";^
    "NHibernate.ObservableCollections.Helpers.csproj|net10.0-windows";^
    "NHibernate.ObservableCollections.DemoApp.csproj|net10.0-windows"

SET BUILD_UNIT___NHIBERNATE_OBSERVABLE_COLLECTIONS___PACKAGE_PARAMETERS=^
    "NHibernate.ObservableCollections.csproj"

REM Needed here because NHibernate.ObservableCollections.nupkg
REM has dependencies on packages whose IDs start with "Iesi.".
SET BUILD_UNIT___NHIBERNATE_OBSERVABLE_COLLECTIONS___PACKAGE_NEV_PARAMETERS=^
    "Iesi."
