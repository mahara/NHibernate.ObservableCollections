@ECHO OFF


SET OUTPUT_FOLDER=bin
SET TEMPORARY_OUTPUT_FOLDER=obj

dotnet clean .\NHibernate.ObservableCollections.sln --configuration Debug
dotnet clean .\NHibernate.ObservableCollections.sln --configuration Release
IF EXIST %OUTPUT_FOLDER% RMDIR %OUTPUT_FOLDER% /S /Q
IF EXIST %TEMPORARY_OUTPUT_FOLDER% RMDIR %TEMPORARY_OUTPUT_FOLDER% /S /Q
