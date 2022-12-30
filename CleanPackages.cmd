@ECHO OFF


ECHO.

SET "OUTPUT_FOLDER_PATH=bin"
SET "TEMPORARY_OUTPUT_FOLDER_PATH=obj"
SET "NEV_BIN_FOLDER_PATH=tools\Explicit.NuGet.Versions\bin"
SET "NEV_OBJ_FOLDER_PATH=tools\Explicit.NuGet.Versions\obj"

REM dotnet clean %1 --configuration Debug
REM dotnet clean %1 --configuration Release

IF EXIST %OUTPUT_FOLDER_PATH% (
    ECHO Deleting "%OUTPUT_FOLDER_PATH%" folder...

    RMDIR %OUTPUT_FOLDER_PATH% /S /Q
)
IF EXIST %TEMPORARY_OUTPUT_FOLDER_PATH% (
    ECHO Deleting "%TEMPORARY_OUTPUT_FOLDER_PATH%" folder...

    RMDIR %TEMPORARY_OUTPUT_FOLDER_PATH% /S /Q
)

IF EXIST %NEV_BIN_FOLDER_PATH% (
    ECHO Deleting "%NEV_BIN_FOLDER_PATH%" folder...

    RMDIR %NEV_BIN_FOLDER_PATH% /S /Q
)
IF EXIST %NEV_OBJ_FOLDER_PATH% (
    ECHO Deleting "%NEV_OBJ_FOLDER_PATH%" folder...

    RMDIR %NEV_OBJ_FOLDER_PATH% /S /Q
)

ECHO.
