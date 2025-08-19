@echo off
echo 🚀 Starting JJBussert Aspire Application

REM Check if .NET 9 is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK not found. Please install .NET 9.0 SDK
    exit /b 1
)

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Node.js not found. Please install Node.js 20+
    exit /b 1
)

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker Desktop
    exit /b 1
)

echo ✅ Prerequisites check passed

REM Restore .NET dependencies
echo 📦 Restoring .NET dependencies...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ Failed to restore .NET dependencies
    exit /b 1
)

REM Install npm dependencies
echo 📦 Installing npm dependencies...
cd JJBussert.Aspire.Web
npm install
if %errorlevel% neq 0 (
    echo ❌ Failed to install npm dependencies
    exit /b 1
)
cd ..

REM Build the solution
echo 🔨 Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ❌ Build failed
    exit /b 1
)

echo ✅ Build successful

REM Run tests
echo 🧪 Running tests...
dotnet test --configuration Release --verbosity minimal
if %errorlevel% neq 0 (
    echo ⚠️  Some tests failed, but continuing...
) else (
    echo ✅ All tests passed
)

REM Start the Aspire AppHost
echo 🌟 Starting Aspire AppHost...
echo 📊 The Aspire dashboard will be available at the URL shown below
echo 🌐 The React app will be available through the dashboard
echo 🔐 Use ?testUser=admin or ?testUser=basic for authentication testing
echo.

dotnet run --project JJBussert.Aspire.AppHost
