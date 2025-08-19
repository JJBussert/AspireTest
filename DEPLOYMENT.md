# Deployment Guide

This guide covers deploying the JJBussert Aspire application to various environments.

## 🚀 Deployment Options

### 1. Azure Container Apps (Recommended)

Azure Container Apps provides the best experience for .NET Aspire applications.

#### Prerequisites
- Azure CLI installed
- Azure subscription
- Docker Desktop

#### Steps

1. **Install Azure Developer CLI**
   ```bash
   # Windows
   winget install microsoft.azd
   
   # macOS
   brew tap azure/azd && brew install azd
   
   # Linux
   curl -fsSL https://aka.ms/install-azd.sh | bash
   ```

2. **Initialize Azure Developer Environment**
   ```bash
   azd init
   azd auth login
   ```

3. **Deploy to Azure**
   ```bash
   azd up
   ```

#### Configuration

Create `azure.yaml` in the root directory:
```yaml
name: jjbussert-aspire
metadata:
  template: aspire-starter@latest
services:
  api:
    project: ./JJBussert.Aspire.Api
    host: containerapp
  web:
    project: ./JJBussert.Aspire.Web
    host: staticwebapp
  dataservice:
    project: ./JJBussert.Aspire.DataService
    host: containerapp
```

### 2. Local Development with Aspire

For local development, Aspire handles all orchestration automatically.

#### Prerequisites
- .NET 9.0 SDK
- Node.js 20+
- Docker Desktop (for SQL Server container)

#### Quick Start

```bash
# Automated setup
./run-local.sh  # Linux/macOS
run-local.bat   # Windows

# Manual setup
dotnet restore
cd JJBussert.Aspire.Web && npm install && cd ..
dotnet run --project JJBussert.Aspire.AppHost
```

#### What Aspire Orchestrates

- **SQL Server Container**: Automatically started with persistent storage
- **API Service**: .NET minimal API with Carter
- **Web Application**: React app with NPM/SWA CLI
- **DataService**: Background worker for database seeding
- **Service Discovery**: All services communicate via Aspire service discovery

### 3. Production Deployment

For production deployment, use Azure Container Apps with Aspire's built-in deployment support.

#### Azure Container Apps with Aspire

Aspire provides first-class support for Azure Container Apps deployment:

```bash
# Install Azure Developer CLI
azd init
azd auth login
azd up
```

#### Manual Container Deployment

If you need custom container deployment:

1. **Build container images**:
   ```bash
   # API
   dotnet publish JJBussert.Aspire.Api -c Release

   # DataService
   dotnet publish JJBussert.Aspire.DataService -c Release

   # Web (build React app)
   cd JJBussert.Aspire.Web
   npm run build
   ```

2. **Deploy to your container platform** (Azure Container Apps, AWS ECS, etc.)

#### Kubernetes with Aspire Manifest

Aspire can generate Kubernetes manifests:

```bash
# Generate manifest
dotnet run --project JJBussert.Aspire.AppHost -- --publisher manifest --output-path ./aspire-manifest.json

# Use the manifest with your Kubernetes deployment tools
```

## 🔧 Environment Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development` |
| `ConnectionStrings__aspiredb` | Database connection | See appsettings |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | OpenTelemetry endpoint | Not set |
| `REACT_APP_API_URL` | API base URL for React | `/api` |

### Production Settings

1. **Enable HTTPS**
   ```json
   {
     "Kestrel": {
       "Endpoints": {
         "Https": {
           "Url": "https://localhost:7000"
         }
       }
     }
   }
   ```

2. **Configure Authentication**
   ```json
   {
     "Authentication": {
       "Schemes": {
         "Bearer": {
           "ValidAudiences": ["your-audience"],
           "ValidIssuers": ["your-issuer"]
         }
       }
     }
   }
   ```

3. **Database Connection**
   ```json
   {
     "ConnectionStrings": {
       "aspiredb": "Server=your-server;Database=AspireDb;Integrated Security=true;"
     }
   }
   ```

## 🔐 Security Considerations

### Production Checklist

- [ ] Use HTTPS everywhere
- [ ] Secure database connections
- [ ] Configure proper CORS policies
- [ ] Set up authentication/authorization
- [ ] Enable request rate limiting
- [ ] Configure logging and monitoring
- [ ] Use secrets management (Azure Key Vault, etc.)
- [ ] Enable health checks
- [ ] Set up backup strategies

### Secrets Management

**Azure Key Vault Integration:**
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri("https://your-keyvault.vault.azure.net/"),
    new DefaultAzureCredential());
```

**Environment Variables:**
```bash
export ConnectionStrings__aspiredb="your-secure-connection-string"
export Authentication__JwtBearer__Authority="your-authority"
```

## 📊 Monitoring & Observability

### Application Insights

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### OpenTelemetry

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddUrlGroup(new Uri("https://api.example.com/health"));
```

## 🚨 Troubleshooting

### Common Issues

1. **Database Connection Failures**
   - Check connection string
   - Verify SQL Server is running
   - Check firewall settings

2. **Authentication Issues**
   - Verify JWT configuration
   - Check token expiration
   - Validate issuer/audience

3. **Service Discovery Problems**
   - Check Aspire configuration
   - Verify service names
   - Review network policies

### Debugging

```bash
# Check container logs
docker logs <container-id>

# Check Kubernetes pods
kubectl logs -f deployment/api -n jjbussert-aspire

# Check health endpoints
curl http://localhost:5000/health
```

## 📞 Support

For deployment issues:
1. Check the troubleshooting section
2. Review application logs
3. Create an issue on GitHub
4. Contact the development team

## 🔄 CI/CD Integration

The included GitHub Actions workflow (`ci-workflow.yml`) supports:
- Automated testing
- Docker image building
- Azure deployment
- Environment promotion

Copy the workflow to `.github/workflows/ci.yml` to enable automated deployments.
