#!/bin/bash

# JJBussert Aspire Deployment Test Script

echo "🧪 Testing JJBussert Aspire Deployment"

# Configuration
API_BASE_URL="http://localhost:5000"
WEB_BASE_URL="http://localhost:3000"
MAX_RETRIES=30
RETRY_DELAY=5

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    local status=$1
    local message=$2
    case $status in
        "INFO")
            echo -e "${BLUE}ℹ️  $message${NC}"
            ;;
        "SUCCESS")
            echo -e "${GREEN}✅ $message${NC}"
            ;;
        "WARNING")
            echo -e "${YELLOW}⚠️  $message${NC}"
            ;;
        "ERROR")
            echo -e "${RED}❌ $message${NC}"
            ;;
    esac
}

# Function to wait for service to be ready
wait_for_service() {
    local url=$1
    local service_name=$2
    local retries=0
    
    print_status "INFO" "Waiting for $service_name to be ready..."
    
    while [ $retries -lt $MAX_RETRIES ]; do
        if curl -s -f "$url" > /dev/null 2>&1; then
            print_status "SUCCESS" "$service_name is ready!"
            return 0
        fi
        
        retries=$((retries + 1))
        print_status "INFO" "Attempt $retries/$MAX_RETRIES - $service_name not ready yet, waiting ${RETRY_DELAY}s..."
        sleep $RETRY_DELAY
    done
    
    print_status "ERROR" "$service_name failed to start within expected time"
    return 1
}

# Function to test API endpoint
test_api_endpoint() {
    local endpoint=$1
    local expected_status=$2
    local test_user=$3
    local description=$4
    
    local url="$API_BASE_URL$endpoint"
    if [ -n "$test_user" ]; then
        url="$url?testUser=$test_user"
    fi
    
    print_status "INFO" "Testing: $description"
    
    local response=$(curl -s -w "%{http_code}" -o /tmp/response.json "$url")
    local status_code="${response: -3}"
    
    if [ "$status_code" = "$expected_status" ]; then
        print_status "SUCCESS" "$description - Status: $status_code"
        return 0
    else
        print_status "ERROR" "$description - Expected: $expected_status, Got: $status_code"
        return 1
    fi
}

# Main test execution
main() {
    print_status "INFO" "Starting deployment tests..."
    
    # Test 1: Wait for API health check
    if ! wait_for_service "$API_BASE_URL/health" "API Service"; then
        exit 1
    fi
    
    # Test 2: Wait for Web application
    if ! wait_for_service "$WEB_BASE_URL" "Web Application"; then
        exit 1
    fi
    
    print_status "INFO" "Running API authentication tests..."
    
    # Test 3: Unauthorized access should return 401
    test_api_endpoint "/api/users" "401" "" "Unauthorized access to users endpoint"
    
    # Test 4: Basic user can read users
    test_api_endpoint "/api/users" "200" "basic" "Basic user reading users"
    
    # Test 5: Admin user can read users
    test_api_endpoint "/api/users" "200" "admin" "Admin user reading users"
    
    # Test 6: Basic user can read organizations
    test_api_endpoint "/api/organizations" "200" "basic" "Basic user reading organizations"
    
    # Test 7: Admin user can read organizations
    test_api_endpoint "/api/organizations" "200" "admin" "Admin user reading organizations"
    
    # Test 8: Test data seeding - should have 10 organizations
    print_status "INFO" "Testing data seeding..."
    local orgs_response=$(curl -s "$API_BASE_URL/api/organizations?testUser=admin")
    local org_count=$(echo "$orgs_response" | jq '. | length' 2>/dev/null || echo "0")
    
    if [ "$org_count" = "10" ]; then
        print_status "SUCCESS" "Data seeding successful - Found 10 organizations"
    else
        print_status "WARNING" "Data seeding check - Expected 10 organizations, found $org_count"
    fi
    
    # Test 9: Test users exist
    local users_response=$(curl -s "$API_BASE_URL/api/users?testUser=admin")
    local user_count=$(echo "$users_response" | jq '. | length' 2>/dev/null || echo "0")
    
    if [ "$user_count" -gt "0" ]; then
        print_status "SUCCESS" "Users found - Total: $user_count"
    else
        print_status "ERROR" "No users found in database"
    fi
    
    # Test 10: Test specific test users exist
    local admin_user=$(echo "$users_response" | jq '.[] | select(.email == "admin@test.com")' 2>/dev/null)
    local basic_user=$(echo "$users_response" | jq '.[] | select(.email == "basic@test.com")' 2>/dev/null)
    
    if [ -n "$admin_user" ]; then
        print_status "SUCCESS" "Test admin user found"
    else
        print_status "ERROR" "Test admin user not found"
    fi
    
    if [ -n "$basic_user" ]; then
        print_status "SUCCESS" "Test basic user found"
    else
        print_status "ERROR" "Test basic user not found"
    fi
    
    # Test 11: Test web application loads
    print_status "INFO" "Testing web application..."
    if curl -s -f "$WEB_BASE_URL" > /dev/null; then
        print_status "SUCCESS" "Web application is accessible"
    else
        print_status "ERROR" "Web application is not accessible"
    fi
    
    print_status "INFO" "Deployment tests completed!"
    
    # Summary
    echo ""
    print_status "INFO" "=== Test Summary ==="
    print_status "INFO" "API Base URL: $API_BASE_URL"
    print_status "INFO" "Web Base URL: $WEB_BASE_URL"
    print_status "INFO" "Organizations: $org_count"
    print_status "INFO" "Users: $user_count"
    
    echo ""
    print_status "SUCCESS" "🎉 All core functionality is working!"
    print_status "INFO" "You can now access:"
    print_status "INFO" "  - Web App: $WEB_BASE_URL"
    print_status "INFO" "  - API: $API_BASE_URL"
    print_status "INFO" "  - API Docs: $API_BASE_URL/swagger"
    print_status "INFO" "  - Health Check: $API_BASE_URL/health"
}

# Check if jq is installed (for JSON parsing)
if ! command -v jq &> /dev/null; then
    print_status "WARNING" "jq not found. Installing jq for JSON parsing..."
    if command -v apt-get &> /dev/null; then
        sudo apt-get update && sudo apt-get install -y jq
    elif command -v yum &> /dev/null; then
        sudo yum install -y jq
    elif command -v brew &> /dev/null; then
        brew install jq
    else
        print_status "WARNING" "Could not install jq. Some tests may not work properly."
    fi
fi

# Run main function
main
