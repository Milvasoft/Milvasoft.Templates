#!/bin/bash
# Builds the test projects in Debug (the tests run against the Debug build) and runs the unit and
# integration test assemblies with the xUnit v3 in-process runner
# (dotnet exec), writing an HTML report per assembly to artifacts/test-results. `dotnet test` (Microsoft.Testing.Platform,
# selected in global.json) works too; this script runs the assemblies directly instead.
# Requires Docker for the integration tests (Testcontainers). Output goes to build/last_run_log.txt.
set -euo pipefail

cd "$(dirname "$0")/.."

LOG=$(realpath -m ./build/last_run_log.txt)
RESULTS=$(realpath -m ./artifacts/test-results)
STAMP=$(date +"%Y_%m_%d_%H_%M")

mkdir -p "$RESULTS"
: > "$LOG"

dotnet build tests/Milvonion.UnitTests/Milvonion.UnitTests.csproj -nologo >> "$LOG"
dotnet build tests/Milvonion.IntegrationTests/Milvonion.IntegrationTests.csproj -nologo >> "$LOG"

status=0

for project in Milvonion.UnitTests Milvonion.IntegrationTests; do
    pushd "tests/$project/bin/Debug/net10.0" > /dev/null
    dotnet exec "$project.dll" -noColor -result-html "$RESULTS/${project}_$STAMP.html" >> "$LOG" 2>&1 || status=1
    popd > /dev/null
done

grep -E "Total:" "$LOG" || true
exit $status
