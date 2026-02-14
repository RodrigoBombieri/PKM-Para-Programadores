# ☁️ Guía de Despliegue - PKM-Dev

## Tabla de Contenidos
- [Preparación para Producción](#preparación-para-producción)
- [Despliegue en Azure](#despliegue-en-azure)
- [Despliegue con Docker](#despliegue-con-docker)
- [CI/CD con GitHub Actions](#cicd-con-github-actions)
- [Configuración de DNS y SSL](#configuración-de-dns-y-ssl)
- [Monitoreo y Logging](#monitoreo-y-logging)
- [Backup y Recuperación](#backup-y-recuperación)
- [Troubleshooting](#troubleshooting)

---

## Preparación para Producción

### Checklist Pre-Despliegue

- [ ] Todos los tests pasan (`dotnet test`)
- [ ] Código revisado y aprobado (Pull Request)
- [ ] Variables de entorno configuradas
- [ ] Connection strings seguros
- [ ] Logging configurado
- [ ] HTTPS habilitado
- [ ] Secrets no commiteados
- [ ] Documentación actualizada
- [ ] Migraciones de BD preparadas

---

### Configuración de Producción

**1. appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "PKMDev": "Information"
    }
  },
  "AllowedHosts": "pkm-dev.azurewebsites.net",
  "ConnectionStrings": {
    "DefaultConnection": "*** USAR AZURE KEY VAULT ***"
  },
  "ApplicationInsights": {
    "InstrumentationKey": "*** USAR SECRETS ***"
  },
  "Email": {
    "SendGridApiKey": "*** USAR SECRETS ***"
  }
}
```

**2. Habilitar HTTPS**:
```csharp
// Program.cs
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
```

**3. Configurar CORS (si aplica)**:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://pkm-dev.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("Production");
```

---

## Despliegue en Azure

### Opción 1: Despliegue desde Visual Studio

#### Paso 1: Crear Recursos en Azure

**Desde Azure Portal** (https://portal.azure.com):

1. **Crear Resource Group**:
```
Nombre: PKMDev-RG
Región: East US (o la más cercana)
```

2. **Crear SQL Database**:
```
Resource Group: PKMDev-RG
Database name: PKMDevDb
Server: pkm-dev-sql.database.windows.net
Pricing tier: Basic (5 DTU) - para empezar
```

3. **Crear App Service**:
```
Resource Group: PKMDev-RG
Name: pkm-dev-app
Runtime stack: .NET 8 (LTS)
Operating System: Windows o Linux
Region: East US
Plan: B1 (Basic) - para empezar
```

---

#### Paso 2: Configurar Connection String

**En Azure Portal → App Service → Configuration → Connection strings**:

```
Name: DefaultConnection
Value: Server=tcp:pkm-dev-sql.database.windows.net,1433;Initial Catalog=PKMDevDb;Persist Security Info=False;User ID=pkm-admin;Password=***;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
Type: SQLAzure
```

**⚠️ IMPORTANTE**: Agregar IP de App Service al Firewall de SQL Server:
- Azure Portal → SQL Server → Networking
- Add: "Allow Azure services and resources to access this server"

---

#### Paso 3: Publicar desde Visual Studio

1. Click derecho en proyecto → **Publish**
2. Seleccionar **Azure**
3. Seleccionar **Azure App Service (Windows/Linux)**
4. Sign in con tu cuenta Azure
5. Seleccionar tu App Service
6. Click **Publish**

Visual Studio hará:
- ✅ Build del proyecto
- ✅ Aplicar transformaciones de config
- ✅ Subir archivos a Azure
- ✅ Reiniciar App Service

**Verificar despliegue**:
```
https://pkm-dev-app.azurewebsites.net
```

---

#### Paso 4: Aplicar Migraciones en Producción

**Opción A: Desde Package Manager Console**:
```powershell
# Cambiar connection string temporalmente
Update-Database -Connection "tu-connection-string-azure"
```

**Opción B: Generar script SQL y ejecutar en Azure**:
```bash
dotnet ef migrations script -o deploy.sql
```
Luego ejecutar `deploy.sql` en Azure SQL usando SSMS o Azure Portal.

**Opción C: Aplicar en startup (recomendado)**:
```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (app.Environment.IsProduction())
    {
        await context.Database.MigrateAsync();
    }
}
```

---

### Opción 2: Despliegue con Azure CLI

```bash
# 1. Login
az login

# 2. Crear Resource Group
az group create --name PKMDev-RG --location eastus

# 3. Crear SQL Server
az sql server create \
  --name pkm-dev-sql \
  --resource-group PKMDev-RG \
  --location eastus \
  --admin-user pkm-admin \
  --admin-password "YourStrong@Password123"

# 4. Crear SQL Database
az sql db create \
  --resource-group PKMDev-RG \
  --server pkm-dev-sql \
  --name PKMDevDb \
  --service-objective Basic

# 5. Configurar Firewall
az sql server firewall-rule create \
  --resource-group PKMDev-RG \
  --server pkm-dev-sql \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# 6. Crear App Service Plan
az appservice plan create \
  --name PKMDev-Plan \
  --resource-group PKMDev-RG \
  --sku B1 \
  --is-linux

# 7. Crear Web App
az webapp create \
  --resource-group PKMDev-RG \
  --plan PKMDev-Plan \
  --name pkm-dev-app \
  --runtime "DOTNET|8.0"

# 8. Configurar Connection String
az webapp config connection-string set \
  --resource-group PKMDev-RG \
  --name pkm-dev-app \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:pkm-dev-sql.database.windows.net,1433;Initial Catalog=PKMDevDb;User ID=pkm-admin;Password=YourStrong@Password123"

# 9. Desplegar aplicación
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group PKMDev-RG \
  --name pkm-dev-app \
  --src app.zip
```

---

## Despliegue con Docker

### Dockerfile

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/PKMDev.Web/PKMDev.Web.csproj", "PKMDev.Web/"]
COPY ["src/PKMDev.Application/PKMDev.Application.csproj", "PKMDev.Application/"]
COPY ["src/PKMDev.Infrastructure/PKMDev.Infrastructure.csproj", "PKMDev.Infrastructure/"]
COPY ["src/PKMDev.Domain/PKMDev.Domain.csproj", "PKMDev.Domain/"]

# Restaurar dependencias
RUN dotnet restore "PKMDev.Web/PKMDev.Web.csproj"

# Copiar todo el código
COPY src/ .

# Build
WORKDIR "/src/PKMDev.Web"
RUN dotnet build "PKMDev.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "PKMDev.Web.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Exponer puerto
EXPOSE 80
EXPOSE 443

# Entry point
ENTRYPOINT ["dotnet", "PKMDev.Web.dll"]
```

---

### docker-compose.yml

```yaml
version: '3.8'

services:
  web:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:80"
      - "8443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
    volumes:
      - ./https:/https:ro
    depends_on:
      - db
    networks:
      - pkm-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password123
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql
    networks:
      - pkm-network

volumes:
  sqldata:

networks:
  pkm-network:
    driver: bridge
```

---

### Comandos Docker

```bash
# Build imagen
docker build -t pkm-dev:latest .

# Ejecutar container
docker run -d -p 8080:80 --name pkm-dev pkm-dev:latest

# Con docker-compose
docker-compose up -d

# Ver logs
docker logs pkm-dev

# Detener
docker-compose down

# Entrar al container
docker exec -it pkm-dev /bin/bash
```

---

### Desplegar Docker a Azure Container Instances

```bash
# 1. Crear Container Registry
az acr create \
  --resource-group PKMDev-RG \
  --name pkmdevregistry \
  --sku Basic

# 2. Login al registry
az acr login --name pkmdevregistry

# 3. Tag imagen
docker tag pkm-dev:latest pkmdevregistry.azurecr.io/pkm-dev:v1

# 4. Push imagen
docker push pkmdevregistry.azurecr.io/pkm-dev:v1

# 5. Deploy a Container Instance
az container create \
  --resource-group PKMDev-RG \
  --name pkm-dev-container \
  --image pkmdevregistry.azurecr.io/pkm-dev:v1 \
  --dns-name-label pkm-dev \
  --ports 80
```

---

## CI/CD con GitHub Actions

### Workflow File

**.github/workflows/azure-deploy.yml**:

```yaml
name: Build and Deploy to Azure

on:
  push:
    branches:
      - main
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: pkm-dev-app
  DOTNET_VERSION: '8.0.x'

jobs:
  build:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Run tests
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish src/PKMDev.Web/PKMDev.Web.csproj -c Release -o ${{env.DOTNET_ROOT}}/publish
    
    - name: Upload artifact
      uses: actions/upload-artifact@v3
      with:
        name: webapp
        path: ${{env.DOTNET_ROOT}}/publish

  deploy:
    runs-on: ubuntu-latest
    needs: build
    environment:
      name: 'Production'
      url: ${{ steps.deploy.outputs.webapp-url }}
    
    steps:
    - name: Download artifact
      uses: actions/download-artifact@v3
      with:
        name: webapp
    
    - name: Deploy to Azure Web App
      id: deploy
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: .
```

---

### Configurar Secrets en GitHub

1. Ir a **Settings → Secrets and variables → Actions**
2. Agregar secrets:

**AZURE_WEBAPP_PUBLISH_PROFILE**:
- Azure Portal → App Service → Deployment Center
- Download Publish Profile
- Copiar contenido XML completo

**Otros secrets útiles**:
```
AZURE_CREDENTIALS          # Service Principal JSON
SQLSERVER_CONNECTION       # Connection string
SENDGRID_API_KEY          # Email service
```

---

### Workflow con Docker

```yaml
name: Docker Build and Push

on:
  push:
    branches: [ main ]

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Login to Azure Container Registry
      uses: azure/docker-login@v1
      with:
        login-server: pkmdevregistry.azurecr.io
        username: ${{ secrets.ACR_USERNAME }}
        password: ${{ secrets.ACR_PASSWORD }}
    
    - name: Build and push Docker image
      run: |
        docker build -t pkmdevregistry.azurecr.io/pkm-dev:${{ github.sha }} .
        docker push pkmdevregistry.azurecr.io/pkm-dev:${{ github.sha }}
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: pkm-dev-app
        images: pkmdevregistry.azurecr.io/pkm-dev:${{ github.sha }}
```

---

## Configuración de DNS y SSL

### Dominio Personalizado

**1. Agregar dominio en Azure**:
```bash
az webapp config hostname add \
  --webapp-name pkm-dev-app \
  --resource-group PKMDev-RG \
  --hostname www.pkm-dev.com
```

**2. Configurar DNS (en tu proveedor de dominios)**:

| Tipo | Nombre | Valor |
|------|--------|-------|
| CNAME | www | pkm-dev-app.azurewebsites.net |
| A | @ | [IP de App Service] |
| TXT | asuid.www | [Verification ID de Azure] |

**3. Habilitar SSL**:
```bash
az webapp config ssl bind \
  --resource-group PKMDev-RG \
  --name pkm-dev-app \
  --certificate-thumbprint [thumbprint] \
  --ssl-type SNI
```

**O usar SSL gratuito de Azure**:
- Azure Portal → App Service → Custom domains
- Add binding → SSL certificate → Create App Service Managed Certificate

---

### Forzar HTTPS

```csharp
// Program.cs
app.UseHttpsRedirection();

// Configuración adicional
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(365);
    options.IncludeSubDomains = true;
    options.Preload = true;
});
```

---

## Monitoreo y Logging

### Application Insights

**1. Agregar NuGet package**:
```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

**2. Configurar en Program.cs**:
```csharp
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

**3. Usar en código**:
```csharp
public class KnowledgeService
{
    private readonly TelemetryClient _telemetry;
    
    public KnowledgeService(TelemetryClient telemetry)
    {
        _telemetry = telemetry;
    }
    
    public async Task<KnowledgeItemDto> CreateAsync(KnowledgeItemDto dto)
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // ... lógica
            
            _telemetry.TrackEvent("KnowledgeItemCreated", 
                new Dictionary<string, string> {
                    { "UserId", dto.UserId.ToString() },
                    { "Type", dto.Type.ToString() }
                });
            
            return result;
        }
        finally
        {
            timer.Stop();
            _telemetry.TrackMetric("CreateKnowledgeItem_Duration", timer.ElapsedMilliseconds);
        }
    }
}
```

---

### Azure Monitor

**Configurar alertas**:
```bash
az monitor metrics alert create \
  --name HighCPU \
  --resource-group PKMDev-RG \
  --scopes /subscriptions/.../resourceGroups/PKMDev-RG/providers/Microsoft.Web/sites/pkm-dev-app \
  --condition "avg Percentage CPU > 80" \
  --window-size 5m \
  --evaluation-frequency 1m
```

**Dashboard de métricas clave**:
- Response time
- Request rate
- Error rate
- CPU usage
- Memory usage

---

## Backup y Recuperación

### Backup de Base de Datos

**Configurar backup automático en Azure SQL**:
```bash
az sql db ltr-policy set \
  --resource-group PKMDev-RG \
  --server pkm-dev-sql \
  --database PKMDevDb \
  --weekly-retention P4W \
  --monthly-retention P12M \
  --yearly-retention P5Y \
  --week-of-year 1
```

**Backup manual**:
```bash
az sql db export \
  --resource-group PKMDev-RG \
  --server pkm-dev-sql \
  --name PKMDevDb \
  --admin-user pkm-admin \
  --admin-password *** \
  --storage-key-type StorageAccessKey \
  --storage-key *** \
  --storage-uri https://mystorageaccount.blob.core.windows.net/backups/PKMDevDb.bacpac
```

---

### Restaurar Base de Datos

```bash
az sql db import \
  --resource-group PKMDev-RG \
  --server pkm-dev-sql \
  --name PKMDevDb-Restored \
  --admin-user pkm-admin \
  --admin-password *** \
  --storage-key-type StorageAccessKey \
  --storage-key *** \
  --storage-uri https://mystorageaccount.blob.core.windows.net/backups/PKMDevDb.bacpac
```

---

## Troubleshooting

### Problema: App no inicia

**Revisar logs**:
```bash
az webapp log tail --name pkm-dev-app --resource-group PKMDev-RG
```

**Habilitar logging detallado**:
```bash
az webapp log config \
  --name pkm-dev-app \
  --resource-group PKMDev-RG \
  --application-logging filesystem \
  --level verbose
```

---

### Problema: Error de conexión a BD

**Verificar firewall**:
- Azure Portal → SQL Server → Networking
- Verificar que la IP de App Service está permitida

**Verificar connection string**:
```bash
az webapp config connection-string list \
  --name pkm-dev-app \
  --resource-group PKMDev-RG
```

---

### Problema: Aplicación lenta

**Escalar verticalmente**:
```bash
az appservice plan update \
  --name PKMDev-Plan \
  --resource-group PKMDev-RG \
  --sku P1V2
```

**Escalar horizontalmente**:
```bash
az appservice plan update \
  --name PKMDev-Plan \
  --resource-group PKMDev-RG \
  --number-of-workers 3
```

---

### Problema: Migraciones no aplicadas

**Ejecutar manualmente**:
```bash
# Opción 1: Desde Package Manager Console
Update-Database -Connection "Azure-Connection-String"

# Opción 2: Generar script
dotnet ef migrations script -o deploy.sql
# Ejecutar en Azure SQL
```

---

## Costos Estimados (Azure)

### Tier Básico (Desarrollo/Testing)

| Servicio | Tier | Costo Mensual (USD) |
|----------|------|---------------------|
| App Service | B1 Basic | ~$13 |
| SQL Database | Basic | ~$5 |
| **Total** | | **~$18/mes** |

### Tier Producción (Pequeña Escala)

| Servicio | Tier | Costo Mensual (USD) |
|----------|------|---------------------|
| App Service | P1V2 | ~$70 |
| SQL Database | S1 Standard | ~$30 |
| Application Insights | Basic | ~$10 |
| **Total** | | **~$110/mes** |

---

## Checklist Post-Despliegue

- [ ] Aplicación accesible via HTTPS
- [ ] Base de datos poblada con seed data
- [ ] SSL configurado y funcionando
- [ ] Monitoreo configurado (Application Insights)
- [ ] Alertas configuradas
- [ ] Backups automáticos habilitados
- [ ] CI/CD pipeline funcionando
- [ ] DNS apuntando correctamente
- [ ] Tests de humo pasando
- [ ] Documentación actualizada

---

## Recursos Adicionales

- [Azure App Service Docs](https://docs.microsoft.com/azure/app-service/)
- [Azure SQL Database Docs](https://docs.microsoft.com/azure/azure-sql/)
- [GitHub Actions Docs](https://docs.github.com/actions)
- [Docker Docs](https://docs.docker.com/)

---

¡Tu aplicación está lista para producción! 🚀
