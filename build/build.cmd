@ECHO OFF


IF "%1" == "" GOTO no_config
IF "%1" NEQ "" GOTO set_config

:set_config
SET CONFIGURATION=%1
GOTO build

:no_config
SET CONFIGURATION=Release
GOTO build

:build
dotnet build .\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln --configuration "Release" || exit /b 1
dotnet build .\NHibernate.ObservableCollections.sln --configuration %CONFIGURATION% || exit /b 1
.\tools\Explicit.NuGet.Versions\bin\nev.exe "bin" "NHibernate.ObservableCollections" || exit /b 1
GOTO test

:test

REM https://github.com/Microsoft/vstest-docs/blob/main/docs/report.md
REM https://github.com/spekt/nunit.testlogger/issues/56

ECHO ---------------------------
ECHO Running NET 6.0 Tests
ECHO ---------------------------

dotnet test .\src\NHibernate.ObservableCollections.Tests --configuration %CONFIGURATION% --framework net6.0 --no-build --output bin\%CONFIGURATION%\net6.0 --results-directory bin\%CONFIGURATION% --logger "nunit;LogFileName=NHibernate.ObservableCollections-Net-TestResults.xml" || exit /b 1
REM dotnet .\bin\%CONFIGURATION%\net6.0\NHibernate.ObservableCollections.Tests.dll --result=bin\%CONFIGURATION%\NHibernate.ObservableCollections-Net-TestResults.xml;format=nunit3 || exit /b 1

ECHO --------------------
ECHO Running NET48 Tests
ECHO --------------------

dotnet test .\src\NHibernate.ObservableCollections.Tests --configuration %CONFIGURATION% --framework net48 --no-build --output bin\%CONFIGURATION%\net48 --results-directory bin\%CONFIGURATION% --logger "nunit;LogFileName=NHibernate.ObservableCollections-NetFramework-TestResults.xml;format=nunit3" || exit /b 1
REM %UserProfile%\.nuget\packages\nunit.consolerunner\3.15.0\tools\nunit3-console.exe bin\%CONFIGURATION%\net48\NHibernate.ObservableCollections.Tests.exe --result=bin\%CONFIGURATION%\NHibernate.ObservableCollections-NetFramework-TestResults.xml;format=nunit3 || exit /b 1



