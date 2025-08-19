# JJBussert Aspire Test Application

A .NET Aspire application demonstrating orchestration of multiple services including a React frontend, .NET API with Carter, SQL Server database, and automated testing.

## Architecture

- **JJBussert.Aspire.AppHost**: Aspire orchestration host
- **JJBussert.Aspire.Api**: Minimal API using Carter framework
- **JJBussert.Aspire.Web**: React TypeScript frontend
- **JJBussert.Aspire.Data**: Entity Framework Core data layer
- **JJBussert.Aspire.Domain**: Domain models (User, Organization)
- **JJBussert.Aspire.DataService**: Background worker for database seeding
- **JJBussert.Aspire.ServiceDefaults**: Shared Aspire service configuration
- **JJBussert.Aspire.Test**: XUnit integration tests

## Prerequisites

- .NET 9.0 SDK
- Node.js 20+
- Docker Desktop (for SQL Server container)

## Getting Started

### Quick Start (Automated)

**Linux/macOS:**
```bash
./run-local.sh
```

**Windows:**
```cmd
run-local.bat
```

### Manual Setup

1. **Prerequisites:**
   - .NET 9.0 SDK
   - Node.js 20+
   - Docker Desktop (running)

2. **Clone and setup:**
   ```bash
   git clone <repository-url>
   cd AspireTest
   dotnet restore
   cd JJBussert.Aspire.Web && npm install && cd ..
   ```

3. **Build and test:**
   ```bash
   dotnet build --configuration Release
   dotnet test
   ```

4. **Run the application:**
   ```bash
   dotnet run --project JJBussert.Aspire.AppHost
   ```

5. **Access the application:**
   - Aspire dashboard: URL shown in console output
   - React app: Available through the dashboard
   - API: Available through service discovery

### Authentication Testing

The application includes built-in test authentication for development:

- **Admin User**: Add `?testUser=admin` to API requests
- **Basic User**: Add `?testUser=basic` to API requests
- **Unauthenticated**: Make requests without the testUser parameter

Example API calls:
```bash
# Admin user (can create/update/delete)
curl "http://localhost:5000/api/users?testUser=admin"

# Basic user (read-only)
curl "http://localhost:5000/api/users?testUser=basic"

# Unauthenticated (should return 401)
curl "http://localhost:5000/api/users"
```

## Testing

### Unit & Integration Tests

```bash
# Run all .NET tests
dotnet test

# Run React tests
cd JJBussert.Aspire.Web
npm test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Deployment Testing

Test the entire deployed application:

```bash
# Linux/macOS
./test-deployment.sh

# Windows
test-deployment.bat
```

### Docker Compose Testing

```bash
# Start all services
docker-compose up -d

# Run deployment tests
./test-deployment.sh

# Stop all services
docker-compose down
```

## Features

- **Service Discovery**: All services use Aspire service discovery
- **Authentication**: SWA CLI integration with role-based authorization (Admin/Basic)
- **Database Seeding**: Automatic creation of 10 organizations with 5-20 users each using Bogus
- **Health Checks**: Built-in health monitoring for all services
- **OpenTelemetry**: Distributed tracing and metrics
- **Automated Testing**: Comprehensive integration tests using Aspire.Hosting.Testing
- **Role-Based Security**: Admin users can CRUD, Basic users read-only
- **Development Auth**: Built-in test authentication for automated testing
- **CI/CD Ready**: GitHub Actions workflow for build, test, and deployment

## Database Schema

- **Organizations**: Company entities with name and description
- **Users**: User entities with roles (Admin/Basic) linked to organizations

## Test Users

The seeder creates test users for authentication testing:
- `admin@test.com` (Admin role)
- `basic@test.com` (Basic role)

## API Endpoints

- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `GET /api/organizations` - List all organizations
- `GET /api/organizations/{id}` - Get organization by ID
- `POST /api/organizations` - Create new organization
- `PUT /api/organizations/{id}` - Update organization
- `DELETE /api/organizations/{id}` - Delete organization

## Development

The application is designed to be fully automatable and testable via CLI without manual intervention. All services are orchestrated through Aspire with proper service discovery and health checks.

### Authentication Flow

1. **Development Mode**: Uses test authentication handler that creates users based on `?testUser` parameter
2. **SWA CLI Mode**: Uses Azure Static Web Apps CLI for local authentication emulation
3. **Production**: Integrates with Azure Static Web Apps authentication

### Testing Strategy

- **Unit Tests**: Individual component testing
- **Integration Tests**: Full Aspire application testing with authentication scenarios
- **Authentication Tests**: Admin, Basic, and Unauthorized user scenarios
- **Health Check Tests**: Verify all services are healthy
- **End-to-End Tests**: Complete user workflows

### CI/CD Pipeline

The included GitHub Actions workflow (`ci-workflow.yml`) provides:

- ✅ .NET solution build and test
- ✅ React app build and test with coverage
- ✅ SQL Server integration testing
- ✅ Docker image builds
- ✅ Automated deployment (when configured)

To add the workflow to your repository:
1. Create `.github/workflows/` directory
2. Copy `ci-workflow.yml` to `.github/workflows/ci.yml`
3. Commit and push to trigger the pipeline

## 🚀 Deployment

### Docker Compose (Recommended for Local/Testing)

```bash
# Start all services
docker-compose up -d

# Test deployment
./test-deployment.sh  # Linux/macOS
test-deployment.bat   # Windows

# Stop services
docker-compose down
```

### Azure Container Apps

See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions including:
- Azure Container Apps deployment
- Kubernetes deployment
- Production configuration
- Security considerations
- Monitoring setup

### Local Development Tips

- Use the provided scripts (`run-local.sh` or `run-local.bat`) for quick setup
- Monitor the Aspire dashboard for service health and logs
- Use `?testUser=admin` or `?testUser=basic` for API testing
- Check Docker Desktop for SQL Server container status
- All services auto-restart on code changes through Aspire
- Use `test-deployment.sh` to validate your local setup
