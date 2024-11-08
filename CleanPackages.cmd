@ECHO OFF

REM ================
REM EXIT CODES
REM ================
REM 0 = Success.
REM 1 = Missing required build infrastructure.
REM 2 = Failed to delete artifacts folder.


@CALL "%~dp0Build.Properties.cmd"

IF NOT DEFINED ARTIFACTS_FOLDER_PATH EXIT /B 1


REM dotnet clean "%1" --configuration Debug
REM dotnet clean "%1" --configuration Release

IF EXIST "%ARTIFACTS_FOLDER_PATH%" (
    ECHO Deleting "%ARTIFACTS_FOLDER_PATH%" folder...

    RMDIR "%ARTIFACTS_FOLDER_PATH%" /S /Q
    IF ERRORLEVEL 1 EXIT /B 2
)

EXIT /B 0
