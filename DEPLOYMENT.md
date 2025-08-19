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

### 2. Docker Compose

For local or on-premises deployment.

#### Create docker-compose.yml

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

  api:
    build:
      context: .
      dockerfile: JJBussert.Aspire.Api/Dockerfile
    ports:
      - "5000:8080"
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__aspiredb=Server=sqlserver,1433;Database=AspireDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

  dataservice:
    build:
      context: .
      dockerfile: JJBussert.Aspire.DataService/Dockerfile
    depends_on:
      - sqlserver
    environment:
      - ConnectionStrings__aspiredb=Server=sqlserver,1433;Database=AspireDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;

  web:
    build:
      context: ./JJBussert.Aspire.Web
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    depends_on:
      - api
    environment:
      - REACT_APP_API_URL=http://localhost:5000

volumes:
  sqldata:
```

#### Deploy with Docker Compose

```bash
docker-compose up -d
```

### 3. Kubernetes

For production Kubernetes deployment.

#### Prerequisites
- Kubernetes cluster
- kubectl configured
- Helm (optional)

#### Create Kubernetes Manifests

1. **Namespace**
   ```yaml
   apiVersion: v1
   kind: Namespace
   metadata:
     name: jjbussert-aspire
   ```

2. **SQL Server Deployment**
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: sqlserver
     namespace: jjbussert-aspire
   spec:
     replicas: 1
     selector:
       matchLabels:
         app: sqlserver
     template:
       metadata:
         labels:
           app: sqlserver
       spec:
         containers:
         - name: sqlserver
           image: mcr.microsoft.com/mssql/server:2022-latest
           env:
           - name: SA_PASSWORD
             value: "YourStrong@Passw0rd"
           - name: ACCEPT_EULA
             value: "Y"
           ports:
           - containerPort: 1433
   ```

3. **API Deployment**
   ```yaml
   apiVersion: apps/v1
   kind: Deployment
   metadata:
     name: api
     namespace: jjbussert-aspire
   spec:
     replicas: 2
     selector:
       matchLabels:
         app: api
     template:
       metadata:
         labels:
           app: api
       spec:
         containers:
         - name: api
           image: your-registry/aspire-api:latest
           ports:
           - containerPort: 8080
           env:
           - name: ConnectionStrings__aspiredb
             value: "Server=sqlserver,1433;Database=AspireDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
   ```

#### Deploy to Kubernetes

```bash
kubectl apply -f k8s/
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
