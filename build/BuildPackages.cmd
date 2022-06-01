@ECHO OFF


:INITIALIZE_ARGUMENTS
SET %1
SET %2

GOTO INITIALIZE_VARIABLES


:INITIALIZE_VARIABLES
SET CONFIGURATION="Release"
SET BUILD_VERSION="1.0.0"

GOTO SET_CONFIGURATION


:SET_CONFIGURATION
IF "%config%"=="" GOTO SET_BUILD_VERSION
SET CONFIGURATION=%config%

GOTO SET_BUILD_VERSION


:SET_BUILD_VERSION
IF "%version%"=="" GOTO BUILD
SET BUILD_VERSION=%version%

GOTO BUILD


:BUILD

ECHO ----------------------------------------------------
ECHO Building "%CONFIGURATION%" packages with version "%BUILD_VERSION%"...
ECHO ----------------------------------------------------

dotnet build .\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln --configuration "Release" || exit /b 1
dotnet build .\NHibernate.ObservableCollections.sln --configuration %CONFIGURATION% -property:APPVEYOR_BUILD_VERSION=%BUILD_VERSION% || exit /b 1
.\tools\Explicit.NuGet.Versions\bin\nev.exe "bin" "NHibernate.ObservableCollections" || exit /b 1

GOTO TEST


:TEST

REM https://github.com/Microsoft/vstest-docs/blob/main/docs/report.md
REM https://github.com/spekt/nunit.testlogger/issues/56

ECHO ----------------------------
ECHO Running .NET (net6.0) Tests
ECHO ----------------------------

dotnet test .\src\NHibernate.ObservableCollections.Tests --configuration %CONFIGURATION% --framework net6.0 --no-build --output bin\%CONFIGURATION%\net6.0 --results-directory bin\%CONFIGURATION% --logger "nunit;LogFileName=NHibernate.ObservableCollections-Net-TestResults.xml" || exit /b 1
REM dotnet .\bin\%CONFIGURATION%\net6.0\NHibernate.ObservableCollections.Tests.dll --result=bin\%CONFIGURATION%\NHibernate.ObservableCollections-Net-TestResults.xml;format=nunit3 || exit /b 1

ECHO ------------------------------------
ECHO Running .NET Framework (net48) Tests
ECHO ------------------------------------

dotnet test .\src\NHibernate.ObservableCollections.Tests --configuration %CONFIGURATION% --framework net48 --no-build --output bin\%CONFIGURATION%\net48 --results-directory bin\%CONFIGURATION% --logger "nunit;LogFileName=NHibernate.ObservableCollections-NetFramework-TestResults.xml;format=nunit3" || exit /b 1
REM %UserProfile%\.nuget\packages\nunit.consolerunner\3.15.0\tools\nunit3-console.exe bin\%CONFIGURATION%\net48\NHibernate.ObservableCollections.Tests.exe --result=bin\%CONFIGURATION%\NHibernate.ObservableCollections-NetFramework-TestResults.xml;format=nunit3 || exit /b 1
