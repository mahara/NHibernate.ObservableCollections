@ECHO OFF

REM https://superuser.com/questions/149951/does-in-batch-file-mean-all-command-line-arguments
REM https://stackoverflow.com/questions/15420004/write-batch-file-with-hyphenated-parameters
REM https://superuser.com/questions/1505178/parsing-command-line-argument-in-batch-file
REM https://ss64.com/nt/for.html
REM https://ss64.com/nt/for_f.html
REM https://stackoverflow.com/questions/2591758/batch-script-loop
REM https://stackoverflow.com/questions/46576996/how-to-use-for-loop-to-get-set-variable-by-batch-file
REM https://stackoverflow.com/questions/3294599/do-batch-files-support-multiline-variables
REM https://stackoverflow.com/questions/36228474/batch-file-if-string-starts-with
REM https://stackoverflow.com/questions/2952401/remove-trailing-slash-from-batch-file-input
REM https://stackoverflow.com/questions/1964192/removing-double-quotes-from-variables-in-batch-file-creates-problems-with-cmd-en
REM
REM ================
REM TERMINOLOGY
REM ================
REM
REM     CALL :LABEL                         = batch subroutine call
REM     EXIT /B code                        = subroutine/script exit code; sets ERRORLEVEL
REM     IF ERRORLEVEL n                     = ERRORLEVEL threshold check
REM     second argument as variable name    = output parameter
REM     CALL SET %%dynamic_name%%           = indirect variable lookup
REM     FOR variable modifiers              = file-name/path parsing
REM     %%~nF                               = file name without extension
REM     %%~xF                               = file extension
REM     %~1                                 = argument dequoting
REM     !VAR!                               = delayed variable expansion inside blocks
REM
REM     %%U         = build unit item
REM     %%G         = generic parameter item
REM     %%H / %%I   = parsed fields from parameter item
REM                   BUILD/TEST: "input|frameworks"
REM                   PACKAGE:    "input"
REM     %%J         = selected framework item
REM     %%F         = file name/path item
REM
REM ================
REM EXIT CODES
REM ================
REM 0  = Success.
REM 1  = Missing required build infrastructure.
REM 2  = Invalid command-line argument.
REM 3  = Build unit build parameters are not defined.
REM 4  = Build framework selection failed.
REM 5  = Build input file path resolution failed.
REM 6  = dotnet build failed.
REM 7  = Test framework selection failed.
REM 8  = Test input file path resolution failed.
REM 9  = dotnet test failed.
REM 10 = Package input file path resolution failed.
REM 11 = dotnet pack failed.
REM 12 = Explicit.NuGet.Versions build failed.
REM 13 = Explicit.NuGet.Versions execution failed.


IF NOT DEFINED ARTIFACTS_FOLDER_PATH EXIT /B 1


SETLOCAL EnableDelayedExpansion


:PARSE_ARGUMENTS

IF "%~1" == "" GOTO INITIALIZE_PARAMETERS

IF /I "%~1" == "--no-build" (
    SET ARGUMENT___RUN_BUILD=false
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--no-test" (
    SET ARGUMENT___RUN_TEST=false
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--no-package" (
    SET ARGUMENT___RUN_PACKAGE=false
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--version" (
    CALL :SET_ARGUMENT_VALUE "%~1" "%~2" ARGUMENT___VERSION
    IF ERRORLEVEL 1 EXIT /B !ERRORLEVEL!
    SHIFT
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--configuration" (
    CALL :SET_ARGUMENT_VALUE "%~1" "%~2" ARGUMENT___CONFIGURATION
    IF ERRORLEVEL 1 EXIT /B !ERRORLEVEL!
    SHIFT
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--frameworks" (
    CALL :SET_ARGUMENT_VALUE "%~1" "%~2" ARGUMENT___FRAMEWORKS
    IF ERRORLEVEL 1 EXIT /B !ERRORLEVEL!
    SHIFT
    SHIFT
    GOTO PARSE_ARGUMENTS
)

IF /I "%~1" == "--disable-source-link" (
    SET ARGUMENT___ENABLE_SOURCE_LINK=false
    SHIFT
    GOTO PARSE_ARGUMENTS
)

ECHO Unknown argument "%~1".
ECHO Supported arguments are --no-build, --no-test, --no-package, --version, --configuration, --frameworks, and --disable-source-link.
EXIT /B 2


:SET_ARGUMENT_VALUE

IF "%~2" == "" (
    ECHO Missing value for %~1.
    EXIT /B 2
)

SET "ARGUMENT_VALUE=%~2"

IF "!ARGUMENT_VALUE:~0,2!" == "--" (
    ECHO Missing value for %~1.
    EXIT /B 2
)

SET "%~3=%~2"

EXIT /B 0


:INITIALIZE_PARAMETERS

SET PARAMETER___RUN_BUILD=%PARAMETER___RUN_BUILD___DEFAULT%
IF DEFINED ARGUMENT___RUN_BUILD (
    SET PARAMETER___RUN_BUILD=%ARGUMENT___RUN_BUILD%
)

SET PARAMETER___RUN_TEST=%PARAMETER___RUN_TEST___DEFAULT%
IF DEFINED ARGUMENT___RUN_TEST (
    SET PARAMETER___RUN_TEST=%ARGUMENT___RUN_TEST%
)

SET PARAMETER___RUN_PACKAGE=%PARAMETER___RUN_PACKAGE___DEFAULT%
IF DEFINED ARGUMENT___RUN_PACKAGE (
    SET PARAMETER___RUN_PACKAGE=%ARGUMENT___RUN_PACKAGE%
)

SET PARAMETER___VERSION=%PARAMETER___VERSION___DEFAULT%
IF DEFINED ARGUMENT___VERSION (
    SET "PARAMETER___VERSION=%ARGUMENT___VERSION%"
)

SET PARAMETER___CONFIGURATION=%PARAMETER___CONFIGURATION___DEFAULT%
IF DEFINED ARGUMENT___CONFIGURATION (
    SET "PARAMETER___CONFIGURATION=%ARGUMENT___CONFIGURATION%"
)

SET PARAMETER___FRAMEWORKS=
IF DEFINED ARGUMENT___FRAMEWORKS (
    SET "PARAMETER___FRAMEWORKS=%ARGUMENT___FRAMEWORKS%"
)

SET PARAMETER___ENABLE_SOURCE_LINK=%PARAMETER___ENABLE_SOURCE_LINK___DEFAULT%
IF DEFINED ARGUMENT___ENABLE_SOURCE_LINK (
    SET PARAMETER___ENABLE_SOURCE_LINK=%ARGUMENT___ENABLE_SOURCE_LINK%
)


:EXECUTE_BUILD_UNITS

IF NOT DEFINED BUILD_UNITS EXIT /B 1

ECHO.

FOR %%U IN (%BUILD_UNITS%) DO (
    SET "BUILD_UNIT=%%~U"

    ECHO ================================================================================
    ECHO Build Unit "!BUILD_UNIT!"
    ECHO ================================================================================

    CALL :LOAD_BUILD_UNIT_PARAMETERS "!BUILD_UNIT!"
    IF ERRORLEVEL 1 EXIT /B !ERRORLEVEL!

    CALL :EXECUTE_BUILD_UNIT
    IF ERRORLEVEL 1 EXIT /B !ERRORLEVEL!
)

EXIT /B 0


REM Build unit parameter dependency model:
REM
REM BUILD_UNIT
REM     requires BUILD_PARAMETERS
REM
REM     optional TEST_PARAMETERS
REM         depends on BUILD_PARAMETERS
REM
REM     optional PACKAGE_PARAMETERS
REM         depends on BUILD_PARAMETERS
REM
REM         optional PACKAGE_NEV_PARAMETERS
REM             depends on PACKAGE_PARAMETERS
REM
REM PACKAGE_NEV_PARAMETERS is package post-processing.
REM It must not run without PACKAGE_PARAMETERS because no package operation boundary exists.
:LOAD_BUILD_UNIT_PARAMETERS

SET "BUILD_UNIT=%~1"

SET BUILD_PARAMETERS=
SET TEST_PARAMETERS=
SET PACKAGE_PARAMETERS=
SET PACKAGE_NEV_PARAMETERS=

REM Load build-unit-specific variables into generic working variables.
REM CALL SET performs a second expansion pass, allowing indirect variable lookup.
CALL SET BUILD_PARAMETERS=%%BUILD_UNIT___%BUILD_UNIT%___BUILD_PARAMETERS%%
CALL SET TEST_PARAMETERS=%%BUILD_UNIT___%BUILD_UNIT%___TEST_PARAMETERS%%
CALL SET PACKAGE_PARAMETERS=%%BUILD_UNIT___%BUILD_UNIT%___PACKAGE_PARAMETERS%%
CALL SET PACKAGE_NEV_PARAMETERS=%%BUILD_UNIT___%BUILD_UNIT%___PACKAGE_NEV_PARAMETERS%%

IF NOT DEFINED BUILD_PARAMETERS (
    ECHO Build unit "!BUILD_UNIT!" is invalid because no build parameters defined.
    EXIT /B 3
)

EXIT /B 0


:EXECUTE_BUILD_UNIT

ECHO.

CALL :BUILD
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

ECHO.

CALL :TEST
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

ECHO.

CALL :PACKAGE
IF ERRORLEVEL 1 EXIT /B %ERRORLEVEL%

ECHO.

EXIT /B 0


:BUILD

IF /I "!PARAMETER___RUN_BUILD!" == "false" (
    ECHO Skipping build operations because --no-build was specified.
    EXIT /B 0
)

FOR %%G IN (%BUILD_PARAMETERS%) DO (
    FOR /F "tokens=1,2 delims=|" %%H IN (%%G) DO (
        SET BUILD_PARAMETER___INPUT_FILE_NAME=%%H
        SET BUILD_PARAMETER___FRAMEWORKS=%%I

        REM Resolve the input file name into a full file path.
        REM The second argument is the name of the variable that receives the resolved path.
        CALL :RESOLVE_BUILD_INPUT_FILE_PATH "!BUILD_PARAMETER___INPUT_FILE_NAME!" BUILD_PARAMETER___INPUT_FILE_PATH
        IF ERRORLEVEL 1 EXIT /B 5

        IF NOT DEFINED PARAMETER___FRAMEWORKS (
            ECHO --------------------------------------------------------------------------------
            ECHO Building "!BUILD_PARAMETER___INPUT_FILE_NAME!" ^(!PARAMETER___VERSION! ^| !PARAMETER___CONFIGURATION! ^| project-defined frameworks^)^.^.^.
            ECHO --------------------------------------------------------------------------------

            SET BUILD_COMMAND=dotnet build "!BUILD_PARAMETER___INPUT_FILE_PATH!" --property:BUILD_VERSION=!PARAMETER___VERSION! --configuration !PARAMETER___CONFIGURATION! --property:ENABLE_SOURCE_LINK=!PARAMETER___ENABLE_SOURCE_LINK! --property:GeneratePackageOnBuild=false
            REM ECHO !BUILD_COMMAND!
            !BUILD_COMMAND! || EXIT /B 6
        ) ELSE (
            CALL :SELECT_FRAMEWORKS "!BUILD_PARAMETER___FRAMEWORKS!" "!PARAMETER___FRAMEWORKS!" BUILD_PARAMETER___FRAMEWORKS
            IF ERRORLEVEL 1 EXIT /B 4

            FOR %%J IN (!BUILD_PARAMETER___FRAMEWORKS!) DO (
                SET BUILD_PARAMETER___FRAMEWORK=%%J

                ECHO --------------------------------------------------------------------------------
                ECHO Building "!BUILD_PARAMETER___INPUT_FILE_NAME!" ^(!PARAMETER___VERSION! ^| !PARAMETER___CONFIGURATION! ^| !BUILD_PARAMETER___FRAMEWORK!^)^.^.^.
                ECHO --------------------------------------------------------------------------------

                SET BUILD_COMMAND=dotnet build "!BUILD_PARAMETER___INPUT_FILE_PATH!" --property:BUILD_VERSION=!PARAMETER___VERSION! --configuration !PARAMETER___CONFIGURATION! --framework !BUILD_PARAMETER___FRAMEWORK! --property:ENABLE_SOURCE_LINK=!PARAMETER___ENABLE_SOURCE_LINK! --property:GeneratePackageOnBuild=false
                REM ECHO !BUILD_COMMAND!
                !BUILD_COMMAND! || EXIT /B 6
            )
        )
    )
)

EXIT /B 0


:TEST

REM https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test
REM https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-vstest
REM https://github.com/Microsoft/vstest-docs/blob/main/docs/report.md
REM https://github.com/spekt/nunit.testlogger/issues/56

IF NOT DEFINED TEST_PARAMETERS (
    ECHO Skipping test operations because no test parameters defined for build unit "!BUILD_UNIT!".
    EXIT /B 0
)

IF /I "!PARAMETER___RUN_TEST!" == "false" (
    ECHO Skipping test operations because --no-test was specified.
    EXIT /B 0
)

SET ARTIFACTS_OUTPUT___CONFIGURATION___FOLDER_PATH=!ARTIFACTS_OUTPUT_FOLDER_PATH!\!PARAMETER___CONFIGURATION!
SET ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH=!ARTIFACTS_TEST_RESULTS_FOLDER_PATH!\!PARAMETER___CONFIGURATION!

FOR %%G IN (%TEST_PARAMETERS%) DO (
    FOR /F "tokens=1,2 delims=|" %%H IN (%%G) DO (
        SET TEST_PARAMETER___INPUT_FILE_NAME=%%H
        SET TEST_PARAMETER___FRAMEWORKS=%%I

        REM Use FOR variable modifiers to split the file name into:
        REM - %%~xF = extension
        REM - %%~nF = file name without extension
        FOR %%F IN ("!TEST_PARAMETER___INPUT_FILE_NAME!") DO (
            SET TEST_PARAMETER___INPUT_FILE_EXTENSION=%%~xF
            SET TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=%%~nF
        )

        IF DEFINED PARAMETER___FRAMEWORKS (
            CALL :SELECT_FRAMEWORKS "!TEST_PARAMETER___FRAMEWORKS!" "!PARAMETER___FRAMEWORKS!" TEST_PARAMETER___FRAMEWORKS
            IF ERRORLEVEL 1 EXIT /B 7
        )

        FOR %%J IN (!TEST_PARAMETER___FRAMEWORKS!) DO (
            SET TEST_PARAMETER___FRAMEWORK=%%J

            REM Resolve the input file name into a full file path.
            REM The second argument is the name of the variable that receives the resolved path.
            CALL :RESOLVE_TEST_INPUT_FILE_PATH "!TEST_PARAMETER___INPUT_FILE_NAME!" "!TEST_PARAMETER___FRAMEWORK!" TEST_PARAMETER___INPUT_FILE_PATH
            IF ERRORLEVEL 1 EXIT /B 8

            ECHO --------------------------------------------------------------------------------
            ECHO Testing "!TEST_PARAMETER___INPUT_FILE_NAME!" ^(!PARAMETER___VERSION! ^| !PARAMETER___CONFIGURATION! ^| !TEST_PARAMETER___FRAMEWORK!^)^.^.^.
            ECHO --------------------------------------------------------------------------------

            IF /I "!TEST_PARAMETER___INPUT_FILE_EXTENSION!" == ".dll" (
                SET TEST_COMMAND=dotnet test "!TEST_PARAMETER___INPUT_FILE_PATH!" --framework !TEST_PARAMETER___FRAMEWORK! --results-directory "!ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH!" --logger "nunit;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-NUnit.xml;format=nunit3" --logger "liquid.md;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-Liquid.md"
            ) ELSE (
                REM SET TEST_COMMAND=dotnet test "!SOURCE_CODE_FOLDER_PATH!\!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!TEST_PARAMETER___INPUT_FILE_NAME!" --configuration !PARAMETER___CONFIGURATION! --framework !TEST_PARAMETER___FRAMEWORK! --no-build --results-directory "!ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH!" --logger "nunit;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-NUnit.xml;format=nunit3" --logger "liquid.md;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-Liquid.md"
                SET TEST_COMMAND=dotnet test "!TEST_PARAMETER___INPUT_FILE_PATH!" --configuration !PARAMETER___CONFIGURATION! --framework !TEST_PARAMETER___FRAMEWORK! --no-build --results-directory "!ARTIFACTS_TEST_RESULTS___CONFIGURATION___FOLDER_PATH!" --logger "nunit;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-NUnit.xml;format=nunit3" --logger "liquid.md;LogFileName=!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!_!PARAMETER___VERSION!_!PARAMETER___CONFIGURATION!_!TEST_PARAMETER___FRAMEWORK!_TestResults-Liquid.md"
            )

            REM ECHO !TEST_COMMAND!
            !TEST_COMMAND! || EXIT /B 9
        )
    )
)

EXIT /B 0


:PACKAGE

IF NOT DEFINED PACKAGE_PARAMETERS (
    ECHO Skipping package operations because no package parameters defined for build unit "!BUILD_UNIT!".
    EXIT /B 0
)

IF /I "!PARAMETER___RUN_PACKAGE!" == "false" (
    ECHO Skipping package operations because --no-package was specified.
    EXIT /B 0
)

IF DEFINED PARAMETER___FRAMEWORKS (
    ECHO Skipping package operations because --frameworks was specified.
    ECHO dotnet pack --no-build requires a full project-defined framework build.
    EXIT /B 0
)

SET ARTIFACTS_PACKAGE_OUTPUT___CONFIGURATION___FOLDER_PATH=!ARTIFACTS_PACKAGE_OUTPUT_FOLDER_PATH!\!PARAMETER___CONFIGURATION!

FOR %%G IN (%PACKAGE_PARAMETERS%) DO (
    FOR /F "tokens=1 delims=|" %%H IN (%%G) DO (
        SET PACKAGE_PARAMETER___INPUT_FILE_NAME=%%H

        CALL :RESOLVE_PACKAGE_INPUT_FILE_PATH "!PACKAGE_PARAMETER___INPUT_FILE_NAME!" PACKAGE_PARAMETER___INPUT_FILE_PATH
        IF ERRORLEVEL 1 EXIT /B 10

        ECHO --------------------------------------------------------------------------------
        ECHO Packing "!PACKAGE_PARAMETER___INPUT_FILE_NAME!" ^(!PARAMETER___VERSION! ^| !PARAMETER___CONFIGURATION!^)^.^.^.
        ECHO --------------------------------------------------------------------------------

        SET PACK_COMMAND=dotnet pack "!PACKAGE_PARAMETER___INPUT_FILE_PATH!" --configuration !PARAMETER___CONFIGURATION! --no-build --property:BUILD_VERSION=!PARAMETER___VERSION! --property:ENABLE_SOURCE_LINK=!PARAMETER___ENABLE_SOURCE_LINK!
        REM ECHO !PACK_COMMAND!
        !PACK_COMMAND! || EXIT /B 11
    )
)

IF NOT DEFINED PACKAGE_NEV_PARAMETERS (
    ECHO Skipping package dependency version update operations because no package dependency version update parameters defined for build unit "!BUILD_UNIT!".
    EXIT /B 0
)

dotnet build "!WORKSPACE_FOLDER_PATH!\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.slnx" --configuration Release >NUL || EXIT /B 12

FOR %%G IN (%PACKAGE_NEV_PARAMETERS%) DO (
    SET "PACKAGE_NEV_PARAMETER___DEPENDENCY_PACKAGE_ID_PREFIX_FILTER=%%~G"

    ECHO --------------------------------------------------------------------------------
    ECHO Updating package dependency versions for "!PACKAGE_NEV_PARAMETER___DEPENDENCY_PACKAGE_ID_PREFIX_FILTER!" ^(!PARAMETER___VERSION! ^| !PARAMETER___CONFIGURATION!^)^.^.^.
    ECHO --------------------------------------------------------------------------------

    SET PACKAGE_NEV_COMMAND="!ARTIFACTS_FOLDER_PATH!\tools\nev\nev.exe" "!ARTIFACTS_PACKAGE_OUTPUT___CONFIGURATION___FOLDER_PATH!" "!PACKAGE_NEV_PARAMETER___DEPENDENCY_PACKAGE_ID_PREFIX_FILTER!"
    REM ECHO !PACKAGE_NEV_COMMAND!
    !PACKAGE_NEV_COMMAND! || EXIT /B 13
)

EXIT /B 0


:RESOLVE_BUILD_INPUT_FILE_PATH

SET "BUILD_PARAMETER___INPUT_FILE_NAME=%~1"

SET BUILD_PARAMETER___INPUT_FILE_EXTENSION=
SET BUILD_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=

REM Use FOR variable modifiers to split the file name into:
REM - %%~xF = extension
REM - %%~nF = file name without extension
FOR %%F IN ("!BUILD_PARAMETER___INPUT_FILE_NAME!") DO (
    SET BUILD_PARAMETER___INPUT_FILE_EXTENSION=%%~xF
    SET BUILD_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=%%~nF
)

IF /I "!BUILD_PARAMETER___INPUT_FILE_EXTENSION!" == ".slnx" (
    SET "%~2=!WORKSPACE_FOLDER_PATH!\!BUILD_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!BUILD_PARAMETER___INPUT_FILE_EXTENSION!" == ".sln" (
    SET "%~2=!WORKSPACE_FOLDER_PATH!\!BUILD_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!BUILD_PARAMETER___INPUT_FILE_EXTENSION!" == ".csproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!BUILD_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!BUILD_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!BUILD_PARAMETER___INPUT_FILE_EXTENSION!" == ".fsproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!BUILD_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!BUILD_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!BUILD_PARAMETER___INPUT_FILE_EXTENSION!" == ".vbproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!BUILD_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!BUILD_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

ECHO Unsupported build input file name "!BUILD_PARAMETER___INPUT_FILE_NAME!".
ECHO Supported build input file extensions are .slnx, .sln, .csproj, .fsproj, and .vbproj.
EXIT /B 1


:RESOLVE_TEST_INPUT_FILE_PATH

SET "TEST_PARAMETER___INPUT_FILE_NAME=%~1"
SET TEST_PARAMETER___FRAMEWORK=%~2

SET TEST_PARAMETER___INPUT_FILE_EXTENSION=
SET TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=

FOR %%F IN ("!TEST_PARAMETER___INPUT_FILE_NAME!") DO (
    SET TEST_PARAMETER___INPUT_FILE_EXTENSION=%%~xF
    SET TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=%%~nF
)

IF /I "!TEST_PARAMETER___INPUT_FILE_EXTENSION!" == ".dll" (
    SET "%~3=!ARTIFACTS_OUTPUT___CONFIGURATION___FOLDER_PATH!\!TEST_PARAMETER___FRAMEWORK!\!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!TEST_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!TEST_PARAMETER___INPUT_FILE_EXTENSION!" == ".csproj" (
    SET "%~3=!SOURCE_CODE_FOLDER_PATH!\!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!TEST_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!TEST_PARAMETER___INPUT_FILE_EXTENSION!" == ".fsproj" (
    SET "%~3=!SOURCE_CODE_FOLDER_PATH!\!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!TEST_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!TEST_PARAMETER___INPUT_FILE_EXTENSION!" == ".vbproj" (
    SET "%~3=!SOURCE_CODE_FOLDER_PATH!\!TEST_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!TEST_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

ECHO Unsupported test input file name "!TEST_PARAMETER___INPUT_FILE_NAME!".
ECHO Supported test input file extensions are .dll, .csproj, .fsproj, and .vbproj.
EXIT /B 1


:RESOLVE_PACKAGE_INPUT_FILE_PATH

SET "PACKAGE_PARAMETER___INPUT_FILE_NAME=%~1"

SET PACKAGE_PARAMETER___INPUT_FILE_EXTENSION=
SET PACKAGE_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=

FOR %%F IN ("!PACKAGE_PARAMETER___INPUT_FILE_NAME!") DO (
    SET PACKAGE_PARAMETER___INPUT_FILE_EXTENSION=%%~xF
    SET PACKAGE_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION=%%~nF
)

IF /I "!PACKAGE_PARAMETER___INPUT_FILE_EXTENSION!" == ".csproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!PACKAGE_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!PACKAGE_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!PACKAGE_PARAMETER___INPUT_FILE_EXTENSION!" == ".fsproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!PACKAGE_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!PACKAGE_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

IF /I "!PACKAGE_PARAMETER___INPUT_FILE_EXTENSION!" == ".vbproj" (
    SET "%~2=!SOURCE_CODE_FOLDER_PATH!\!PACKAGE_PARAMETER___INPUT_FILE_NAME_WITHOUT_EXTENSION!\!PACKAGE_PARAMETER___INPUT_FILE_NAME!"
    EXIT /B 0
)

ECHO Unsupported package input file name "!PACKAGE_PARAMETER___INPUT_FILE_NAME!".
ECHO Supported package input file extensions are .csproj, .fsproj, and .vbproj.
EXIT /B 1


:SELECT_FRAMEWORKS

SET SELECT_PARAMETER___AVAILABLE_FRAMEWORKS=%~1
SET SELECT_PARAMETER___REQUESTED_FRAMEWORKS=%~2
SET SELECT_PARAMETER___SELECTED_FRAMEWORKS=

FOR %%R IN (%SELECT_PARAMETER___REQUESTED_FRAMEWORKS%) DO (
    SET "SELECT_PARAMETER___REQUESTED_FRAMEWORK=%%~R"

    SET SELECT_PARAMETER___EXACT_MATCH_FOUND=false

    FOR %%A IN (%SELECT_PARAMETER___AVAILABLE_FRAMEWORKS%) DO (
        SET "SELECT_PARAMETER___AVAILABLE_FRAMEWORK=%%~A"

        IF /I "!SELECT_PARAMETER___AVAILABLE_FRAMEWORK!" == "!SELECT_PARAMETER___REQUESTED_FRAMEWORK!" (
            CALL :APPEND_FRAMEWORK "!SELECT_PARAMETER___AVAILABLE_FRAMEWORK!" SELECT_PARAMETER___SELECTED_FRAMEWORKS
            SET SELECT_PARAMETER___EXACT_MATCH_FOUND=true
        )
    )

    IF /I "!SELECT_PARAMETER___EXACT_MATCH_FOUND!" == "false" (
        FOR %%A IN (%SELECT_PARAMETER___AVAILABLE_FRAMEWORKS%) DO (
            SET "SELECT_PARAMETER___AVAILABLE_FRAMEWORK=%%~A"

            ECHO(!SELECT_PARAMETER___AVAILABLE_FRAMEWORK! | FINDSTR /I /L /B /C:"!SELECT_PARAMETER___REQUESTED_FRAMEWORK!" >NUL 2>NUL

            IF NOT ERRORLEVEL 1 (
                CALL :APPEND_FRAMEWORK "!SELECT_PARAMETER___AVAILABLE_FRAMEWORK!" SELECT_PARAMETER___SELECTED_FRAMEWORKS
            )
        )
    )
)

IF NOT DEFINED SELECT_PARAMETER___SELECTED_FRAMEWORKS (
    ECHO No matching frameworks found.
    ECHO Available frameworks: "%SELECT_PARAMETER___AVAILABLE_FRAMEWORKS%"
    ECHO Requested frameworks: "%SELECT_PARAMETER___REQUESTED_FRAMEWORKS%"
    EXIT /B 1
)

SET "%~3=%SELECT_PARAMETER___SELECTED_FRAMEWORKS%"

EXIT /B 0


:APPEND_FRAMEWORK

IF NOT DEFINED %~2 (
    SET "%~2=%~1"
) ELSE (
    SET "%~2=!%~2!;%~1"
)

EXIT /B 0
