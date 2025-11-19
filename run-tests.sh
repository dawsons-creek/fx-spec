#!/bin/bash
# Convenience script to run FX.Spec tests using the FX.Spec runner

set -e

# Build the test project
echo "Building tests..."
dotnet build tests/FX.Spec.Core.Tests/FX.Spec.Core.Tests.fsproj

# Build the runner
echo "Building runner..."
dotnet build src/FX.Spec.Runner/FX.Spec.Runner.fsproj

# Run the tests
echo ""
echo "Running tests with FX.Spec runner..."
echo ""
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/FX.Spec.Core.Tests/bin/Debug/net9.0/FX.Spec.Core.Tests.dll "$@"

