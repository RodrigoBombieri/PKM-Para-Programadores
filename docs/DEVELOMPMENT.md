# 🛠️ Guía de Desarrollo - PKM-Dev

## Tabla de Contenidos
- [Configuración del Entorno](#configuración-del-entorno)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Flujo de Trabajo](#flujo-de-trabajo)
- [Estándares de Código](#estándares-de-código)
- [Testing](#testing)
- [Debugging](#debugging)
- [Base de Datos](#base-de-datos)
- [Frontend](#frontend)
- [Tips y Mejores Prácticas](#tips-y-mejores-prácticas)

---

## Configuración del Entorno

### Requisitos del Sistema

| Software | Versión Mínima | Recomendada |
|----------|----------------|-------------|
| .NET SDK | 8.0 | 8.0.x (última) |
| Visual Studio | 2022 17.0 | 2022 17.8+ |
| SQL Server | 2019 | 2022 |
| Node.js | 18.x | 20.x LTS |
| Git | 2.30+ | Latest |

### Instalación Paso a Paso

#### 1. Instalar .NET 8 SDK

**Windows**:
```powershell
# Descargar desde https://dotnet.microsoft.com/download/dotnet/8.0
# O usar winget
winget install Microsoft.DotNet.SDK.8
```

**macOS**:
```bash
brew install --cask dotnet-sdk
```

**Linux (Ubuntu)**:
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0
```

**Verificar instalación**:
```bash
dotnet --version
# Debe mostrar 8.0.x
```

---

#### 2. Instalar SQL Server

**Windows**:
- Descargar [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads)
- O usar LocalDB (incluido con Visual Studio)

**macOS/Linux**:
```bash
# Docker es la opción más sencilla
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sqlserver \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

**SQL Server Management Studio** (opcional pero recomendado):
- Descargar [SSMS](https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms)

---

#### 3. Instalar Visual Studio 2022

**Workloads necesarios**:
- ✅ ASP.NET and web development
- ✅ .NET desktop development
- ✅ Data storage and processing

**Extensiones recomendadas**:
- [ReSharper](https://www.jetbrains.com/resharper/) (opcional, pago)
- [CodeMaid](https://marketplace.visualstudio.com/items?itemName=SteveCadwallader.CodeMaidVS2022)
- [SonarLint](https://marketplace.visualstudio.com/items?itemName=SonarSource.SonarLintforVisualStudio2022)
- [Markdown Editor](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.MarkdownEditor2)

**Alternativa: Visual Studio Code**
```bash
# Extensiones necesarias
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-mssql.mssql
```

---

#### 4. Clonar y Configurar el Proyecto

```bash
# Clonar repositorio
git clone https://github.com/tuusuario/pkm-dev.git
cd pkm-dev

# Restaurar dependencias
dotnet restore

# Verificar que compila
dotnet build

# Si todo está OK, verás:
# Build succeeded.
```

---

#### 5. Configurar Base de Datos

**Opción A: LocalDB (Desarrollo en Windows)**

`appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PKMDevDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

**Opción B: SQL Server Local**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PKMDevDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

**Opción C: Docker**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=PKMDevDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true"
  }
}
```

**Aplicar migraciones**:
```bash
# Desde el directorio del proyecto
dotnet ef database update

# Si no tienes dotnet-ef, instálalo primero:
dotnet tool install --global dotnet-ef
```

---

#### 6. Configurar Secrets (Opcional para Producción)

```bash
# Inicializar user secrets
dotnet user-secrets init

# Agregar connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "tu-connection-string"

# Agregar otros secrets
dotnet user-secrets set "SendGrid:ApiKey" "tu-api-key"
```

---

#### 7. Ejecutar la Aplicación

**Desde Visual Studio**:
- Presionar `F5` o `Ctrl+F5`

**Desde línea de comandos**:
```bash
dotnet run --project src/PKMDev.Web

# Con hot reload
dotnet watch run --project src/PKMDev.Web
```

**Acceder a la aplicación**:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

---

## Estructura del Proyecto

### Vista General

```
PKM-Dev/
│
├── src/
│   ├── PKMDev.Web/                      # 🎨 Presentation Layer
│   │   ├── Controllers/                 # MVC Controllers
│   │   │   ├── AccountController.cs
│   │   │   ├── KnowledgeController.cs
│   │   │   ├── TagsController.cs
│   │   │   └── RoadmapController.cs
│   │   │
│   │   ├── Views/                       # Razor Views
│   │   │   ├── Shared/
│   │   │   │   ├── _Layout.cshtml
│   │   │   │   └── _ValidationScriptsPartial.cshtml
│   │   │   ├── Knowledge/
│   │   │   │   ├── Index.cshtml
│   │   │   │   ├── Create.cshtml
│   │   │   │   └── Details.cshtml
│   │   │   └── ...
│   │   │
│   │   ├── ViewModels/                  # DTOs para vistas
│   │   │   ├── KnowledgeItemViewModel.cs
│   │   │   ├── CreateKnowledgeViewModel.cs
│   │   │   └── DashboardViewModel.cs
│   │   │
│   │   ├── wwwroot/                     # Static files
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   ├── lib/
│   │   │   └── images/
│   │   │
│   │   ├── Program.cs                   # Entry point
│   │   └── appsettings.json            # Configuration
│   │
│   ├── PKMDev.Application/              # 💼 Business Layer
│   │   ├── Services/
│   │   │   ├── KnowledgeService.cs
│   │   │   ├── TagService.cs
│   │   │   └── AIInsightService.cs
│   │   │
│   │   ├── Interfaces/
│   │   │   ├── IKnowledgeService.cs
│   │   │   └── ITagService.cs
│   │   │
│   │   ├── DTOs/
│   │   │   ├── KnowledgeItemDto.cs
│   │   │   └── TagDto.cs
│   │   │
│   │   ├── Validators/
│   │   │   └── KnowledgeItemValidator.cs
│   │   │
│   │   ├── Mapping/
│   │   │   └── MappingProfile.cs
│   │   │
│   │   └── Exceptions/
│   │       ├── BusinessException.cs
│   │       └── NotFoundException.cs
│   │
│   ├── PKMDev.Infrastructure/           # 🗄️ Data Layer
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── DbInitializer.cs
│   │   │   └── Migrations/
│   │   │
│   │   ├── Repositories/
│   │   │   ├── Repository.cs
│   │   │   ├── KnowledgeRepository.cs
│   │   │   └── TagRepository.cs
│   │   │
│   │   └── Configurations/              # Fluent API configs
│   │       ├── KnowledgeItemConfiguration.cs
│   │       └── TagConfiguration.cs
│   │
│   └── PKMDev.Domain/                   # 🏛️ Domain Layer
│       ├── Entities/
│       │   ├── KnowledgeItem.cs
│       │   ├── Tag.cs
│       │   ├── RoadmapItem.cs
│       │   └── ApplicationUser.cs
│       │
│       ├── Enums/
│       │   ├── KnowledgeType.cs
│       │   ├── RoadmapStatus.cs
│       │   └── Priority.cs
│       │
│       └── Common/
│           └── BaseEntity.cs
│
├── tests/
│   ├── PKMDev.UnitTests/
│   │   ├── Services/
│   │   └── Repositories/
│   │
│   └── PKMDev.IntegrationTests/
│       └── Controllers/
│
├── docs/
│   ├── ARCHITECTURE.md
│   ├── API.md
│   ├── DEVELOPMENT.md
│   └── DEPLOYMENT.md
│
├── .github/
│   └── workflows/
│       └── azure-deploy.yml
│
├── .gitignore
├── README.md
└── PKMDev.sln
```

---

## Flujo de Trabajo

### Branching Strategy (Git Flow)

```
main (producción)
  └── develop (desarrollo)
       ├── feature/nueva-funcionalidad
       ├── bugfix/corregir-error
       └── hotfix/error-critico
```

**Comandos típicos**:
```bash
# Crear feature branch
git checkout develop
git pull origin develop
git checkout -b feature/add-export-functionality

# Trabajar y commitear
git add .
git commit -m "feat: add export to PDF functionality"

# Push y crear PR
git push origin feature/add-export-functionality
# Crear Pull Request en GitHub
```

---

### Convención de Commits (Conventional Commits)

```bash
# Formato
<type>(<scope>): <subject>

# Tipos
feat:     Nueva funcionalidad
fix:      Corrección de bug
docs:     Documentación
style:    Formato de código
refactor: Refactorización
test:     Tests
chore:    Tareas de mantenimiento

# Ejemplos
git commit -m "feat(knowledge): add markdown preview"
git commit -m "fix(auth): resolve login redirect issue"
git commit -m "docs(readme): update installation instructions"
git commit -m "refactor(service): extract validation logic"
```

---

### Workflow Diario Típico

**1. Actualizar tu branch**:
```bash
git checkout develop
git pull origin develop
```

**2. Crear feature branch**:
```bash
git checkout -b feature/nombre-funcionalidad
```

**3. Desarrollar y testear**:
```bash
# Hacer cambios...
dotnet build
dotnet test
```

**4. Commit frecuentemente**:
```bash
git add .
git commit -m "feat: implement initial structure"
```

**5. Push y crear PR**:
```bash
git push origin feature/nombre-funcionalidad
# Crear PR en GitHub targeting 'develop'
```

**6. Code Review y Merge**:
- Esperar aprobación
- Resolver conflictos si hay
- Merge a develop

---

## Estándares de Código

### Naming Conventions

**C# (PascalCase)**:
```csharp
// Clases, Métodos, Propiedades
public class KnowledgeService { }
public void CreateItem() { }
public string Title { get; set; }

// Interfaces (prefijo I)
public interface IKnowledgeRepository { }

// Campos privados (camelCase con _)
private readonly IMapper _mapper;

// Parámetros y variables locales (camelCase)
public void Process(int itemId, string userName)
{
    var result = DoSomething();
}

// Constantes (PascalCase)
public const string DefaultLanguage = "CSharp";
```

---

### Organización de Código

**Orden de miembros en una clase**:
```csharp
public class KnowledgeService
{
    // 1. Campos privados
    private readonly IKnowledgeRepository _repository;
    private readonly ILogger<KnowledgeService> _logger;

    // 2. Constructor
    public KnowledgeService(
        IKnowledgeRepository repository,
        ILogger<KnowledgeService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // 3. Propiedades públicas
    public int MaxItemsPerPage { get; set; } = 20;

    // 4. Métodos públicos
    public async Task<KnowledgeItemDto> CreateAsync(KnowledgeItemDto dto)
    {
        // ...
    }

    // 5. Métodos privados
    private async Task ValidateAsync(KnowledgeItemDto dto)
    {
        // ...
    }
}
```

---

### Buenas Prácticas

**✅ DO**:
```csharp
// Usar async/await para operaciones I/O
public async Task<List<KnowledgeItem>> GetAllAsync()
{
    return await _context.KnowledgeItems.ToListAsync();
}

// Usar using para recursos IDisposable
using var connection = new SqlConnection(connectionString);

// Validar parámetros
public void Process(KnowledgeItem item)
{
    if (item == null)
        throw new ArgumentNullException(nameof(item));
    
    // ...
}

// Usar null-conditional y null-coalescing
var title = item?.Title ?? "Untitled";

// LINQ para consultas legibles
var items = await _context.KnowledgeItems
    .Where(k => k.UserId == userId)
    .OrderByDescending(k => k.CreatedAt)
    .Take(10)
    .ToListAsync();
```

**❌ DON'T**:
```csharp
// NO bloquear llamadas async
var result = GetDataAsync().Result;  // ❌ Puede causar deadlock

// NO ignorar excepciones
try { } catch { }  // ❌ Nunca hacer esto

// NO usar magic numbers
if (count > 100) { }  // ❌ Usar constante

// NO hardcodear strings
var sql = "SELECT * FROM Users";  // ❌ Usar repositorio

// NO crear dependencias circulares
// ServiceA → ServiceB → ServiceA  ❌
```

---

### Code Comments

```csharp
/// <summary>
/// Crea un nuevo item de conocimiento para el usuario autenticado.
/// </summary>
/// <param name="dto">Datos del item a crear</param>
/// <returns>El item creado con ID asignado</returns>
/// <exception cref="BusinessException">
/// Si ya existe un item con el mismo título para el usuario
/// </exception>
public async Task<KnowledgeItemDto> CreateAsync(KnowledgeItemDto dto)
{
    // Comentarios inline solo para lógica compleja
    // La mayoría del código debe ser auto-explicativo
    
    // Validar duplicados antes de insertar
    var exists = await CheckDuplicateAsync(dto);
    if (exists)
        throw new BusinessException("Título duplicado");
    
    // ... resto del código
}
```

---

## Testing

### Estructura de Tests

```
tests/
├── PKMDev.UnitTests/
│   ├── Services/
│   │   └── KnowledgeServiceTests.cs
│   └── Repositories/
│       └── KnowledgeRepositoryTests.cs
│
└── PKMDev.IntegrationTests/
    └── Controllers/
        └── KnowledgeControllerTests.cs
```

---

### Unit Tests (xUnit + Moq)

```csharp
public class KnowledgeServiceTests
{
    private readonly Mock<IKnowledgeRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<KnowledgeService>> _mockLogger;
    private readonly KnowledgeService _service;

    public KnowledgeServiceTests()
    {
        _mockRepository = new Mock<IKnowledgeRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<KnowledgeService>>();
        
        _service = new KnowledgeService(
            _mockRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedItem()
    {
        // Arrange
        var dto = new KnowledgeItemDto
        {
            Title = "Test Item",
            Content = "Test content",
            UserId = 1
        };

        var entity = new KnowledgeItem
        {
            Id = 1,
            Title = dto.Title,
            Content = dto.Content
        };

        _mockMapper
            .Setup(m => m.Map<KnowledgeItem>(dto))
            .Returns(entity);

        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<KnowledgeItem>()))
            .Returns(Task.CompletedTask);

        _mockMapper
            .Setup(m => m.Map<KnowledgeItemDto>(entity))
            .Returns(dto);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Item", result.Title);
        
        _mockRepository.Verify(
            r => r.AddAsync(It.IsAny<KnowledgeItem>()), 
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTitle_ThrowsException()
    {
        // Arrange
        var dto = new KnowledgeItemDto { Title = "Duplicate", UserId = 1 };
        
        _mockRepository
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<KnowledgeItem, bool>>>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessException>(
            () => _service.CreateAsync(dto));
    }
}
```

---

### Integration Tests

```csharp
public class KnowledgeControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public KnowledgeControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Index_ReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        // (autenticación si es necesario)

        // Act
        var response = await _client.GetAsync("/Knowledge/Index");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/html; charset=utf-8", 
            response.Content.Headers.ContentType.ToString());
    }
}
```

---

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Tests específicos
dotnet test --filter "FullyQualifiedName~KnowledgeServiceTests"

# Con verbosidad
dotnet test --logger "console;verbosity=detailed"
```

---

## Debugging

### Visual Studio

**Breakpoints**:
- `F9`: Toggle breakpoint
- `F5`: Start debugging
- `F10`: Step over
- `F11`: Step into
- `Shift+F11`: Step out

**Ventanas útiles**:
- Locals (variables locales)
- Watch (expresiones personalizadas)
- Call Stack (pila de llamadas)
- Immediate Window (ejecutar código)

---

### Logging

**Configuración en `appsettings.Development.json`**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "PKMDev": "Debug"
    }
  }
}
```

**Uso en código**:
```csharp
_logger.LogDebug("Processing item {ItemId}", itemId);
_logger.LogInformation("User {UserId} created item {ItemId}", userId, itemId);
_logger.LogWarning("Duplicate title detected: {Title}", title);
_logger.LogError(ex, "Error creating knowledge item");
```

---

## Base de Datos

### Migrations

**Crear migración**:
```bash
dotnet ef migrations add AddKnowledgeItemEntity
```

**Aplicar migración**:
```bash
dotnet ef database update
```

**Revertir migración**:
```bash
dotnet ef database update PreviousMigrationName
```

**Eliminar última migración** (si no aplicada):
```bash
dotnet ef migrations remove
```

**Generar script SQL**:
```bash
dotnet ef migrations script -o migration.sql
```

---

### Seed Data

```csharp
public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.KnowledgeItems.AnyAsync())
            return;  // Ya tiene datos

        var tags = new List<Tag>
        {
            new Tag { Name = "csharp" },
            new Tag { Name = "dotnet" },
            new Tag { Name = "sql" }
        };

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
    }
}

// Llamar desde Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}
```

---

## Frontend

### Estructura CSS/JS

```
wwwroot/
├── css/
│   ├── site.css           # Estilos globales
│   └── pages/
│       ├── dashboard.css
│       └── knowledge.css
│
├── js/
│   ├── site.js            # Scripts globales
│   └── pages/
│       ├── knowledge.js   # Scripts específicos
│       └── roadmap.js
│
└── lib/                   # Librerías (CDN o npm)
    ├── bootstrap/
    ├── jquery/
    └── chart.js/
```

---

### Razor Best Practices

```cshtml
@* Usar strongly-typed models *@
@model KnowledgeItemViewModel

@* Tag Helpers en lugar de HTML helpers *@
<form asp-action="Create" asp-controller="Knowledge" method="post">
    <div class="form-group">
        <label asp-for="Title"></label>
        <input asp-for="Title" class="form-control" />
        <span asp-validation-for="Title" class="text-danger"></span>
    </div>
    
    <button type="submit" class="btn btn-primary">Crear</button>
</form>

@* Partial views para reutilización *@
@await Html.PartialAsync("_KnowledgeItemCard", Model)

@* View components para lógica compleja *@
@await Component.InvokeAsync("RecentActivity")

@* Scripts al final del body *@
@section Scripts {
    <script src="~/js/pages/knowledge.js"></script>
}
```

---

## Tips y Mejores Prácticas

### Performance

1. **Usar AsNoTracking para queries de solo lectura**:
```csharp
var items = await _context.KnowledgeItems
    .AsNoTracking()
    .ToListAsync();
```

2. **Evitar N+1 queries con Include**:
```csharp
// ❌ Malo - N+1 query
var items = await _context.KnowledgeItems.ToListAsync();
foreach (var item in items)
{
    var tags = item.Tags;  // Query adicional por cada item
}

// ✅ Bueno - Single query
var items = await _context.KnowledgeItems
    .Include(k => k.Tags)
    .ToListAsync();
```

3. **Paginación**:
```csharp
var pageSize = 20;
var page = 1;

var items = await _context.KnowledgeItems
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

---

### Seguridad

1. **Siempre validar entrada del usuario**
2. **Usar [ValidateAntiForgeryToken] en POST**
3. **Sanitizar HTML/Markdown antes de renderizar**
4. **No exponer información sensible en logs**
5. **Usar HTTPS siempre**

---

### Productividad

**Snippets útiles en Visual Studio**:
- `ctor`: Constructor
- `prop`: Property
- `cw`: Console.WriteLine
- `for`: For loop
- `foreach`: Foreach loop

**Atajos de teclado**:
- `Ctrl+K, Ctrl+D`: Format document
- `Ctrl+K, Ctrl+C`: Comment selection
- `Ctrl+.`: Quick actions
- `F12`: Go to definition
- `Ctrl+F12`: Go to implementation

---

## Recursos Adicionales

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Docs](https://docs.microsoft.com/ef/core)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit Documentation](https://xunit.net/)

---

## Preguntas Frecuentes

**P: ¿Cómo actualizo Entity Framework Core?**
```bash
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.x
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.x
```

**P: ¿Cómo reseteo la base de datos?**
```bash
dotnet ef database drop
dotnet ef database update
```

**P: ¿Cómo agrego una nueva dependencia?**
```bash
dotnet add package NombreDelPaquete
```

---

¡Listo para desarrollar! 🚀
