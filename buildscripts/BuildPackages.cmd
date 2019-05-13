@ECHO OFF

:INITIALIZE_ARGUMENTS
SET %1
SET %2

REM ECHO arg1 = %1
REM ECHO arg2 = %2

GOTO INITIALIZE_VARIABLES


:INITIALIZE_VARIABLES
SET CONFIGURATION="Release"
SET BUILD_VERSION="5.0.0"

GOTO SET_CONFIGURATION


:SET_CONFIGURATION
IF "%config%"=="" GOTO SET_BUILD_VERSION
SET CONFIGURATION=%config%

GOTO SET_BUILD_VERSION


:SET_BUILD_VERSION
IF "%version%"=="" GOTO RESTORE_PACKAGES
SET BUILD_VERSION=%version%

ECHO ---------------------------------------------------
REM ECHO Building "%config%" packages with version "%version%"...
ECHO Building "%CONFIGURATION%" packages with version "%BUILD_VERSION%"...
ECHO ---------------------------------------------------

GOTO RESTORE_PACKAGES


:RESTORE_PACKAGES
REM dotnet restore .\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.csproj
dotnet restore .\buildscripts\BuildScripts.csproj
dotnet restore .\src\NHibernate.ObservableCollections\NHibernate.ObservableCollections.csproj

GOTO BUILD


:BUILD
REM dotnet build .\tools\Explicit.NuGet.Versions\Explicit.NuGet.Versions.sln --no-restore
dotnet build .\src\NHibernate.ObservableCollections\NHibernate.ObservableCollections.csproj -c %CONFIGURATION% /p:APPVEYOR_BUILD_VERSION=%BUILD_VERSION% --no-restore

REM GOTO TEST


REM :TEST

REM ECHO ----------------
REM ECHO Running Tests...
REM ECHO ----------------

REM dotnet test .\src\NHibernate.ObservableCollections.Tests --no-restore || exit /b 1

REM GOTO NUGET_EXPLICIT_VERSIONS


REM :NUGET_EXPLICIT_VERSIONS

REM .\tools\Explicit.NuGet.Versions\build\nev.exe ".\build" "NHibernate.ObservableCollections"



