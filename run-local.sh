#!/bin/bash

# JJBussert Aspire Local Development Script

echo "🚀 Starting JJBussert Aspire Application"

# Check if .NET 9 is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9.0 SDK"
    exit 1
fi

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js not found. Please install Node.js 20+"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker Desktop"
    exit 1
fi

echo "✅ Prerequisites check passed"

# Restore .NET dependencies
echo "📦 Restoring .NET dependencies..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "❌ Failed to restore .NET dependencies"
    exit 1
fi

# Install npm dependencies
echo "📦 Installing npm dependencies..."
cd JJBussert.Aspire.Web
npm install

if [ $? -ne 0 ]; then
    echo "❌ Failed to install npm dependencies"
    exit 1
fi

cd ..

# Build the solution
echo "🔨 Building solution..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi

echo "✅ Build successful"

# Run tests
echo "🧪 Running tests..."
dotnet test --configuration Release --verbosity minimal

if [ $? -ne 0 ]; then
    echo "⚠️  Some tests failed, but continuing..."
else
    echo "✅ All tests passed"
fi

# Start the Aspire AppHost
echo "🌟 Starting Aspire AppHost..."
echo "📊 The Aspire dashboard will be available at the URL shown below"
echo "🌐 The React app will be available through the dashboard"
echo "🔐 Use ?testUser=admin or ?testUser=basic for authentication testing"
echo ""

dotnet run --project JJBussert.Aspire.AppHost
