@ECHO OFF


SET OUTPUT_FOLDER=bin

dotnet clean --configuration Debug
dotnet clean --configuration Release
IF EXIST %OUTPUT_FOLDER% RMDIR %OUTPUT_FOLDER% /S /Q
