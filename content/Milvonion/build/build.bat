@echo off
rem Builds the test projects in Debug (the tests run against the Debug build) and runs the unit and
rem integration test assemblies with the xUnit v3 in-process runner
rem (dotnet exec), writing an HTML report per assembly to artifacts/test-results. `dotnet test` (Microsoft.Testing.Platform,
rem selected in global.json) works too; this script runs the assemblies directly instead.
rem Requires Docker for the integration tests (Testcontainers). Output goes to build\last_run_log.txt.
setlocal

cd /d "%~dp0.."

set LOG=%CD%\build\last_run_log.txt
set RESULTS=%CD%\artifacts\test-results
for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyy_MM_dd_HH_mm"') do set STAMP=%%i

if not exist "%RESULTS%" mkdir "%RESULTS%"
type nul > "%LOG%"

dotnet build tests\Milvonion.UnitTests\Milvonion.UnitTests.csproj -nologo >> "%LOG%" || exit /b 1
dotnet build tests\Milvonion.IntegrationTests\Milvonion.IntegrationTests.csproj -nologo >> "%LOG%" || exit /b 1

set STATUS=0

for %%p in (Milvonion.UnitTests Milvonion.IntegrationTests) do (
    pushd tests\%%p\bin\Debug\net10.0
    dotnet exec %%p.dll -noColor -result-html "%RESULTS%\%%p_%STAMP%.html" >> "%LOG%" 2>&1 || set STATUS=1
    popd
)

findstr /c:"Total:" "%LOG%"
exit /b %STATUS%
