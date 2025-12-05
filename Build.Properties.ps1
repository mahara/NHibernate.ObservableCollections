#Requires -Version 7.0


# ================
# CONVENTIONS
# ================
# -   All "*FOLDER_PATH" variables must not have a trailing folder/directory separator.
#     This mirrors the Build.Properties.cmd convention.
# -   All "*FOLDER_PATH" variables defined below are base/root folder/directory paths,
#     even though their variable names don't use the word "BASE".


$BUILD_CONFIGURATION_FOLDER_NAME = 'build'
$SOURCE_CODE_FOLDER_NAME = 'src'
$ARTIFACTS_FOLDER_NAME = 'artifacts'
$ARTIFACTS_OUTPUT_FOLDER_NAME = 'bin'
$ARTIFACTS_PACKAGE_OUTPUT_FOLDER_NAME = 'packages'
$ARTIFACTS_TEST_RESULTS_FOLDER_NAME = 'testresults'

$WORKSPACE_FOLDER_PATH = $PSScriptRoot.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
$BUILD_CONFIGURATION_FOLDER_PATH = Join-Path -Path $WORKSPACE_FOLDER_PATH -ChildPath $BUILD_CONFIGURATION_FOLDER_NAME
$SOURCE_CODE_FOLDER_PATH = Join-Path -Path $WORKSPACE_FOLDER_PATH -ChildPath $SOURCE_CODE_FOLDER_NAME
$ARTIFACTS_FOLDER_PATH = Join-Path -Path $WORKSPACE_FOLDER_PATH -ChildPath $ARTIFACTS_FOLDER_NAME
$ARTIFACTS_OUTPUT_FOLDER_PATH = Join-Path -Path $ARTIFACTS_FOLDER_PATH -ChildPath $ARTIFACTS_OUTPUT_FOLDER_NAME
$ARTIFACTS_PACKAGE_OUTPUT_FOLDER_PATH = Join-Path -Path $ARTIFACTS_FOLDER_PATH -ChildPath $ARTIFACTS_PACKAGE_OUTPUT_FOLDER_NAME
$ARTIFACTS_TEST_RESULTS_FOLDER_PATH = Join-Path -Path $ARTIFACTS_FOLDER_PATH -ChildPath $ARTIFACTS_TEST_RESULTS_FOLDER_NAME


$PARAMETER___RUN_BUILD___DEFAULT = 'true'
$PARAMETER___RUN_TEST___DEFAULT = 'true'
$PARAMETER___RUN_PACKAGE___DEFAULT = 'true'
$PARAMETER___VERSION___DEFAULT = '5.7.0'
$PARAMETER___CONFIGURATION___DEFAULT = 'Release'
$PARAMETER___FRAMEWORKS___DEFAULT = 'net10.0;net9.0;net8.0;net48'
$PARAMETER___ENABLE_SOURCE_LINK___DEFAULT = 'true'


# ================================================================================
# BUILD UNITS
# ================================================================================
# Build units are executed in the order defined below.
# Each build unit completes build -> test -> package before the next build unit starts.

$BUILD_UNITS = @(
    'Iesi.ObservableCollections'
    'NHibernate.ObservableCollections'
)


$BUILD_UNIT_PARAMETERS = @{
    'Iesi.ObservableCollections' = @{
        BUILD_PARAMETERS = @(
            "Iesi.ObservableCollections.csproj|$PARAMETER___FRAMEWORKS___DEFAULT"
            "Iesi.ObservableCollections.Tests.csproj|$PARAMETER___FRAMEWORKS___DEFAULT"
            "Iesi.ObservableCollections.PerformanceTests.csproj|$PARAMETER___FRAMEWORKS___DEFAULT"
        )

        TEST_PARAMETERS = @(
            "Iesi.ObservableCollections.Tests.dll|$PARAMETER___FRAMEWORKS___DEFAULT"
        )

        PACKAGE_PARAMETERS = @(
            'Iesi.ObservableCollections.csproj'
        )

        # Not needed here because Iesi.ObservableCollections.nupkg
        # does not have dependencies on packages whose IDs start with "Iesi.".
        # PACKAGE_NEV_PARAMETERS = @(
        #     'Iesi.'
        # )
    }


    'NHibernate.ObservableCollections' = @{
        BUILD_PARAMETERS = @(
            "NHibernate.ObservableCollections.csproj|$PARAMETER___FRAMEWORKS___DEFAULT"
            'NHibernate.ObservableCollections.Helpers.csproj|net10.0-windows'
            'NHibernate.ObservableCollections.DemoApp.csproj|net10.0-windows'
        )

        PACKAGE_PARAMETERS = @(
            'NHibernate.ObservableCollections.csproj'
        )

        # Needed here because NHibernate.ObservableCollections.nupkg
        # has dependencies on packages whose IDs start with "Iesi.".
        PACKAGE_NEV_PARAMETERS = @(
            'Iesi.'
        )
    }
}
