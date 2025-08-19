# Contributing to JJBussert Aspire Application

Thank you for your interest in contributing to this .NET Aspire application! This document provides guidelines and information for contributors.

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Node.js 20+
- Docker Desktop
- Git

### Quick Setup

```bash
# Clone the repository
git clone https://github.com/JJBussert/AspireTest.git
cd AspireTest

# Quick start (automated)
./run-local.sh  # Linux/macOS
run-local.bat   # Windows

# Or manual setup
dotnet restore
cd JJBussert.Aspire.Web && npm install && cd ..
dotnet build --configuration Release
dotnet test
```

## 🏗️ Architecture Overview

```
JJBussert.Aspire.sln
├── JJBussert.Aspire.AppHost/          # Aspire orchestration
├── JJBussert.Aspire.Api/              # Carter-based minimal API
├── JJBussert.Aspire.Web/              # React TypeScript frontend
├── JJBussert.Aspire.Data/             # Entity Framework Core
├── JJBussert.Aspire.Domain/           # Domain models
├── JJBussert.Aspire.DataService/      # Background worker
├── JJBussert.Aspire.ServiceDefaults/  # Shared Aspire config
└── JJBussert.Aspire.Test/             # XUnit integration tests
```

## 🔧 Development Guidelines

### Code Style

- Follow the `.editorconfig` settings
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Follow C# naming conventions (PascalCase for public members)
- Use `var` for local variables when type is obvious

### Testing

- Write tests for all new features
- Maintain or improve test coverage
- Use the authentication test patterns:
  - `?testUser=admin` for admin scenarios
  - `?testUser=basic` for basic user scenarios
  - No parameter for unauthorized scenarios

### Authentication Testing Examples

```csharp
// Admin user test
var response = await _apiClient.GetAsync("/api/users?testUser=admin");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

// Basic user test
var response = await _apiClient.GetAsync("/api/users?testUser=basic");
Assert.Equal(HttpStatusCode.OK, response.StatusCode);

// Unauthorized test
var unauthClient = _app.CreateHttpClient("api");
var response = await unauthClient.GetAsync("/api/users");
Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
```

## 🧪 Testing Strategy

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run React tests
cd JJBussert.Aspire.Web
npm test
```

### Test Categories

1. **Unit Tests**: Individual component testing
2. **Integration Tests**: Full Aspire application testing
3. **Authentication Tests**: Role-based access scenarios
4. **Health Check Tests**: Service availability
5. **End-to-End Tests**: Complete user workflows

## 📝 Pull Request Process

1. **Fork** the repository
2. **Create** a feature branch: `git checkout -b feature/your-feature-name`
3. **Make** your changes following the guidelines
4. **Test** your changes thoroughly
5. **Commit** with descriptive messages
6. **Push** to your fork
7. **Create** a pull request with:
   - Clear description of changes
   - Test results
   - Screenshots (if UI changes)

### Commit Message Format

```
type(scope): description

- feat: new feature
- fix: bug fix
- docs: documentation changes
- style: formatting changes
- refactor: code refactoring
- test: adding tests
- chore: maintenance tasks
```

## 🔐 Authentication Development

### Local Development

The application uses a test authentication handler for development:

- **Admin User**: `?testUser=admin` - Full CRUD access
- **Basic User**: `?testUser=basic` - Read-only access
- **Unauthenticated**: No parameter - Should return 401

### SWA CLI Integration

For Azure Static Web Apps CLI testing:

```bash
cd JJBussert.Aspire.Web
npm run swa:start
```

## 🐛 Bug Reports

When reporting bugs, please include:

- **Environment**: OS, .NET version, Node.js version
- **Steps to reproduce**
- **Expected behavior**
- **Actual behavior**
- **Error messages** (if any)
- **Screenshots** (if applicable)

## 💡 Feature Requests

For new features:

- **Use case**: Why is this needed?
- **Proposed solution**: How should it work?
- **Alternatives**: Other approaches considered?
- **Impact**: Who benefits from this feature?

## 📚 Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Carter Framework](https://github.com/CarterCommunity/Carter)
- [Azure Static Web Apps CLI](https://azure.github.io/static-web-apps-cli/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)

## 🤝 Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Follow the project's technical standards

## 📞 Getting Help

- **Issues**: Use GitHub Issues for bugs and feature requests
- **Discussions**: Use GitHub Discussions for questions
- **Documentation**: Check the README.md first

Thank you for contributing! 🎉
