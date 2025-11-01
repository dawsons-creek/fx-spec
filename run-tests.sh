#!/bin/bash
# Convenience script to run FxSpec tests using the FxSpec runner

set -e

# Build the test project
echo "Building tests..."
dotnet build tests/FxSpec.Core.Tests/FxSpec.Core.Tests.fsproj

# Build the runner
echo "Building runner..."
dotnet build src/FxSpec.Runner/FxSpec.Runner.fsproj

# Run the tests
echo ""
echo "Running tests with FxSpec runner..."
echo ""
dotnet run --project src/FxSpec.Runner/FxSpec.Runner.fsproj -- \
  tests/FxSpec.Core.Tests/bin/Debug/net9.0/FxSpec.Core.Tests.dll "$@"

