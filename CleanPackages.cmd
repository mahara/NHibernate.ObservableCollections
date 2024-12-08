@ECHO OFF


@CALL "Build.Properties.cmd"

IF NOT DEFINED ARTIFACTS_FOLDER_PATH EXIT /B 1


REM dotnet clean "%1" --configuration Debug
REM dotnet clean "%1" --configuration Release

IF EXIST "%ARTIFACTS_FOLDER_PATH%" (
    ECHO Deleting "%ARTIFACTS_FOLDER_PATH%" folder...

    RMDIR "%ARTIFACTS_FOLDER_PATH%\" /S /Q
)
