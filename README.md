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

1. Clone the repository
2. Restore dependencies:
   ```bash
   dotnet restore
   cd JJBussert.Aspire.Web && npm install
   ```

3. Run the application:
   ```bash
   dotnet run --project JJBussert.Aspire.AppHost
   ```

4. Access the Aspire dashboard at the URL shown in the console output

## Testing

Run all tests:
```bash
dotnet test
```

Run React tests:
```bash
cd JJBussert.Aspire.Web
npm test
```

## Features

- **Service Discovery**: All services use Aspire service discovery
- **Database Seeding**: Automatic creation of 10 organizations with 5-20 users each using Bogus
- **Health Checks**: Built-in health monitoring for all services
- **OpenTelemetry**: Distributed tracing and metrics
- **Automated Testing**: Integration tests using Aspire.Hosting.Testing
- **CI/CD**: GitHub Actions workflow for build, test, and deployment

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
