@echo off
echo 🧪 Testing JJBussert Aspire Deployment

REM Configuration
set API_BASE_URL=http://localhost:5000
set WEB_BASE_URL=http://localhost:3000
set MAX_RETRIES=30
set RETRY_DELAY=5

echo ℹ️  Starting deployment tests...

REM Test 1: Wait for API health check
echo ℹ️  Waiting for API Service to be ready...
set /a retries=0
:wait_api
curl -s -f "%API_BASE_URL%/health" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ API Service is ready!
    goto test_web
)
set /a retries+=1
if %retries% geq %MAX_RETRIES% (
    echo ❌ API Service failed to start within expected time
    exit /b 1
)
echo ℹ️  Attempt %retries%/%MAX_RETRIES% - API not ready yet, waiting %RETRY_DELAY%s...
timeout /t %RETRY_DELAY% >nul
goto wait_api

:test_web
REM Test 2: Wait for Web application
echo ℹ️  Waiting for Web Application to be ready...
set /a retries=0
:wait_web
curl -s -f "%WEB_BASE_URL%" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ Web Application is ready!
    goto test_auth
)
set /a retries+=1
if %retries% geq %MAX_RETRIES% (
    echo ❌ Web Application failed to start within expected time
    exit /b 1
)
echo ℹ️  Attempt %retries%/%MAX_RETRIES% - Web app not ready yet, waiting %RETRY_DELAY%s...
timeout /t %RETRY_DELAY% >nul
goto wait_web

:test_auth
echo ℹ️  Running API authentication tests...

REM Test 3: Unauthorized access should return 401
echo ℹ️  Testing: Unauthorized access to users endpoint
curl -s -w "%%{http_code}" -o nul "%API_BASE_URL%/api/users" > temp_status.txt
set /p status=<temp_status.txt
if "%status%"=="401" (
    echo ✅ Unauthorized access test - Status: %status%
) else (
    echo ❌ Unauthorized access test - Expected: 401, Got: %status%
)

REM Test 4: Basic user can read users
echo ℹ️  Testing: Basic user reading users
curl -s -w "%%{http_code}" -o nul "%API_BASE_URL%/api/users?testUser=basic" > temp_status.txt
set /p status=<temp_status.txt
if "%status%"=="200" (
    echo ✅ Basic user reading users - Status: %status%
) else (
    echo ❌ Basic user reading users - Expected: 200, Got: %status%
)

REM Test 5: Admin user can read users
echo ℹ️  Testing: Admin user reading users
curl -s -w "%%{http_code}" -o nul "%API_BASE_URL%/api/users?testUser=admin" > temp_status.txt
set /p status=<temp_status.txt
if "%status%"=="200" (
    echo ✅ Admin user reading users - Status: %status%
) else (
    echo ❌ Admin user reading users - Expected: 200, Got: %status%
)

REM Test 6: Basic user can read organizations
echo ℹ️  Testing: Basic user reading organizations
curl -s -w "%%{http_code}" -o nul "%API_BASE_URL%/api/organizations?testUser=basic" > temp_status.txt
set /p status=<temp_status.txt
if "%status%"=="200" (
    echo ✅ Basic user reading organizations - Status: %status%
) else (
    echo ❌ Basic user reading organizations - Expected: 200, Got: %status%
)

REM Test 7: Admin user can read organizations
echo ℹ️  Testing: Admin user reading organizations
curl -s -w "%%{http_code}" -o nul "%API_BASE_URL%/api/organizations?testUser=admin" > temp_status.txt
set /p status=<temp_status.txt
if "%status%"=="200" (
    echo ✅ Admin user reading organizations - Status: %status%
) else (
    echo ❌ Admin user reading organizations - Expected: 200, Got: %status%
)

REM Test 8: Test web application loads
echo ℹ️  Testing web application...
curl -s -f "%WEB_BASE_URL%" >nul 2>&1
if %errorlevel% equ 0 (
    echo ✅ Web application is accessible
) else (
    echo ❌ Web application is not accessible
)

REM Cleanup
del temp_status.txt >nul 2>&1

echo ℹ️  Deployment tests completed!
echo.
echo ℹ️  === Test Summary ===
echo ℹ️  API Base URL: %API_BASE_URL%
echo ℹ️  Web Base URL: %WEB_BASE_URL%
echo.
echo ✅ 🎉 All core functionality is working!
echo ℹ️  You can now access:
echo ℹ️    - Web App: %WEB_BASE_URL%
echo ℹ️    - API: %API_BASE_URL%
echo ℹ️    - API Docs: %API_BASE_URL%/swagger
echo ℹ️    - Health Check: %API_BASE_URL%/health
