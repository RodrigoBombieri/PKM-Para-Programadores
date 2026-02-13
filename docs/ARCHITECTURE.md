# 🏗️ Guía de Arquitectura - PKM-Dev

## Tabla de Contenidos
- [Visión General](#visión-general)
- [Principios de Diseño](#principios-de-diseño)
- [Arquitectura en Capas](#arquitectura-en-capas)
- [Patrones de Diseño](#patrones-de-diseño)
- [Estructura de Carpetas](#estructura-de-carpetas)
- [Flujo de Datos](#flujo-de-datos)
- [Seguridad](#seguridad)
- [Escalabilidad](#escalabilidad)

---

## Visión General

PKM-Dev está construido siguiendo los principios de **Clean Architecture** y **Separation of Concerns (SoC)**, lo que permite:

- ✅ **Mantenibilidad**: Código organizado y fácil de mantener
- ✅ **Testabilidad**: Cada capa puede probarse de forma aislada
- ✅ **Escalabilidad**: Fácil agregar nuevas funcionalidades
- ✅ **Flexibilidad**: Cambiar tecnologías sin afectar la lógica de negocio

### Diagrama de Alto Nivel

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENTE (Browser)                     │
│                   HTML/CSS/JavaScript                    │
└─────────────────────────────────────────────────────────┘
                           ↓ HTTPS
┌─────────────────────────────────────────────────────────┐
│              PRESENTATION LAYER (ASP.NET MVC)            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Controllers  │  │    Views     │  │  ViewModels  │  │
│  │              │  │   (Razor)    │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│           BUSINESS LOGIC LAYER (Services)                │
│  ┌──────────────────────────────────────────────────┐   │
│  │  KnowledgeService │ TagService │ RoadmapService  │   │
│  │  UserService      │ AIInsightService             │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │              DTOs & Business Rules               │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│          DATA ACCESS LAYER (Repositories)                │
│  ┌──────────────────────────────────────────────────┐   │
│  │  IKnowledgeRepository │ ITagRepository           │   │
│  │  IRoadmapRepository   │ IUserRepository          │   │
│  └──────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────┐   │
│  │           ApplicationDbContext (EF Core)         │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────┐
│                   DATABASE (SQL Server)                  │
│     Users │ KnowledgeItems │ Tags │ RoadmapItems        │
└─────────────────────────────────────────────────────────┘
```

---

## Principios de Diseño

### 1. Separation of Concerns (SoC)
Cada capa tiene una responsabilidad específica y bien definida.

### 2. Dependency Inversion (DIP)
Las capas superiores no dependen de las inferiores, sino de abstracciones (interfaces).

```csharp
// ❌ MAL - Dependencia directa
public class KnowledgeController
{
    private KnowledgeRepository _repo = new KnowledgeRepository();
}

// ✅ BIEN - Inyección de dependencias
public class KnowledgeController
{
    private readonly IKnowledgeService _service;
    
    public KnowledgeController(IKnowledgeService service)
    {
        _service = service;
    }
}
```

### 3. Single Responsibility Principle (SRP)
Cada clase tiene una única razón para cambiar.

### 4. DRY (Don't Repeat Yourself)
Lógica común centralizada en servicios y helpers.

---

## Arquitectura en Capas

### 1️⃣ Presentation Layer (Capa de Presentación)

**Responsabilidad**: Manejar la interacción con el usuario

**Componentes**:
- **Controllers**: Reciben requests HTTP, validan, llaman servicios
- **Views**: Templates Razor para renderizar HTML
- **ViewModels**: DTOs específicos para la vista

**Ejemplo de Controller**:
```csharp
public class KnowledgeController : Controller
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IMapper _mapper;

    public KnowledgeController(
        IKnowledgeService knowledgeService, 
        IMapper mapper)
    {
        _knowledgeService = knowledgeService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Index(KnowledgeFilterViewModel filter)
    {
        var items = await _knowledgeService.GetFilteredAsync(filter);
        var viewModel = _mapper.Map<List<KnowledgeItemViewModel>>(items);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateKnowledgeViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var dto = _mapper.Map<KnowledgeItemDto>(model);
        await _knowledgeService.CreateAsync(dto);
        
        return RedirectToAction(nameof(Index));
    }
}
```

---

### 2️⃣ Business Logic Layer (Capa de Lógica de Negocio)

**Responsabilidad**: Implementar reglas de negocio y orquestar operaciones

**Componentes**:
- **Services**: Lógica de negocio principal
- **DTOs**: Objetos de transferencia de datos
- **Validators**: Validaciones de negocio
- **Business Rules**: Reglas específicas del dominio

**Ejemplo de Service**:
```csharp
public interface IKnowledgeService
{
    Task<KnowledgeItemDto> GetByIdAsync(int id);
    Task<List<KnowledgeItemDto>> GetFilteredAsync(KnowledgeFilterDto filter);
    Task<KnowledgeItemDto> CreateAsync(KnowledgeItemDto dto);
    Task UpdateAsync(KnowledgeItemDto dto);
    Task DeleteAsync(int id);
    Task<List<string>> SuggestTagsAsync(int knowledgeItemId);
}

public class KnowledgeService : IKnowledgeService
{
    private readonly IKnowledgeRepository _repository;
    private readonly ITagRepository _tagRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<KnowledgeService> _logger;

    public KnowledgeService(
        IKnowledgeRepository repository,
        ITagRepository tagRepository,
        IMapper mapper,
        ILogger<KnowledgeService> logger)
    {
        _repository = repository;
        _tagRepository = tagRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<KnowledgeItemDto> CreateAsync(KnowledgeItemDto dto)
    {
        try
        {
            // Validación de negocio
            await ValidateKnowledgeItemAsync(dto);

            // Mapeo a entidad
            var entity = _mapper.Map<KnowledgeItem>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            // Persistencia
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Log de auditoría
            _logger.LogInformation(
                "Knowledge item created: {Id} by user {UserId}", 
                entity.Id, 
                entity.UserId);

            return _mapper.Map<KnowledgeItemDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating knowledge item");
            throw;
        }
    }

    private async Task ValidateKnowledgeItemAsync(KnowledgeItemDto dto)
    {
        // Validar que el título no esté duplicado para el mismo usuario
        var exists = await _repository.ExistsAsync(
            k => k.Title == dto.Title && k.UserId == dto.UserId);
        
        if (exists)
            throw new BusinessException(
                "Ya existe un item con ese título");

        // Otras validaciones de negocio...
    }
}
```

---

### 3️⃣ Data Access Layer (Capa de Acceso a Datos)

**Responsabilidad**: Interactuar con la base de datos

**Componentes**:
- **Repositories**: Patrón Repository para abstraer acceso a datos
- **DbContext**: Configuración de Entity Framework
- **Entities**: Modelos de base de datos
- **Migrations**: Control de versiones de BD

**Ejemplo de Repository**:
```csharp
public interface IKnowledgeRepository : IRepository<KnowledgeItem>
{
    Task<List<KnowledgeItem>> GetByUserIdAsync(int userId);
    Task<List<KnowledgeItem>> GetByTagAsync(string tagName);
    Task<List<KnowledgeItem>> SearchAsync(string searchTerm);
    Task<bool> ExistsAsync(Expression<Func<KnowledgeItem, bool>> predicate);
}

public class KnowledgeRepository : Repository<KnowledgeItem>, IKnowledgeRepository
{
    public KnowledgeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<KnowledgeItem>> GetByUserIdAsync(int userId)
    {
        return await _context.KnowledgeItems
            .Include(k => k.Tags)
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<KnowledgeItem>> GetByTagAsync(string tagName)
    {
        return await _context.KnowledgeItems
            .Include(k => k.Tags)
            .Where(k => k.Tags.Any(t => t.Name == tagName))
            .ToListAsync();
    }

    public async Task<List<KnowledgeItem>> SearchAsync(string searchTerm)
    {
        return await _context.KnowledgeItems
            .Include(k => k.Tags)
            .Where(k => k.Title.Contains(searchTerm) || 
                       k.Content.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(
        Expression<Func<KnowledgeItem, bool>> predicate)
    {
        return await _context.KnowledgeItems.AnyAsync(predicate);
    }
}
```

**DbContext**:
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<KnowledgeItem> KnowledgeItems { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<RoadmapItem> RoadmapItems { get; set; }
    public DbSet<AIInsight> AIInsights { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuración de relación N:M
        builder.Entity<KnowledgeItemTag>()
            .HasKey(kt => new { kt.KnowledgeItemId, kt.TagId });

        builder.Entity<KnowledgeItemTag>()
            .HasOne(kt => kt.KnowledgeItem)
            .WithMany(k => k.KnowledgeItemTags)
            .HasForeignKey(kt => kt.KnowledgeItemId);

        builder.Entity<KnowledgeItemTag>()
            .HasOne(kt => kt.Tag)
            .WithMany(t => t.KnowledgeItemTags)
            .HasForeignKey(kt => kt.TagId);

        // Índices para mejorar performance
        builder.Entity<KnowledgeItem>()
            .HasIndex(k => k.UserId);

        builder.Entity<KnowledgeItem>()
            .HasIndex(k => k.Type);

        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
}
```

---

### 4️⃣ Cross-Cutting Concerns

**Componentes transversales** que afectan a todas las capas:

#### Logging
```csharp
// Configuración en Program.cs
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddApplicationInsights(); // Azure
});

// Uso en servicios
_logger.LogInformation("User {UserId} created knowledge item {ItemId}", 
    userId, itemId);
_logger.LogError(ex, "Error processing request");
```

#### Mapping (AutoMapper)
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<KnowledgeItem, KnowledgeItemDto>().ReverseMap();
        CreateMap<KnowledgeItemDto, KnowledgeItemViewModel>();
        CreateMap<CreateKnowledgeViewModel, KnowledgeItemDto>();
    }
}
```

#### Exception Handling
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Business rule violation");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(
                new { error = "Internal server error" });
        }
    }
}
```

---

## Patrones de Diseño

### 1. Repository Pattern
Abstrae el acceso a datos y facilita testing.

### 2. Dependency Injection
Configurado en `Program.cs`:
```csharp
// Repositories
builder.Services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Services
builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<ITagService, TagService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
```

### 3. Unit of Work
Para transacciones complejas:
```csharp
public interface IUnitOfWork : IDisposable
{
    IKnowledgeRepository KnowledgeItems { get; }
    ITagRepository Tags { get; }
    IRoadmapRepository Roadmaps { get; }
    Task<int> SaveChangesAsync();
}
```

### 4. DTO Pattern
Separación entre entidades de BD y objetos de transferencia.

---

## Estructura de Carpetas

```
PKM-Dev/
├── src/
│   ├── PKMDev.Web/                    # Presentation Layer
│   │   ├── Controllers/
│   │   ├── Views/
│   │   ├── ViewModels/
│   │   ├── wwwroot/
│   │   └── Program.cs
│   │
│   ├── PKMDev.Application/            # Business Layer
│   │   ├── Services/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   ├── Validators/
│   │   └── Exceptions/
│   │
│   ├── PKMDev.Infrastructure/         # Data Layer
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   └── Configurations/
│   │
│   └── PKMDev.Domain/                 # Domain Layer
│       ├── Entities/
│       ├── Enums/
│       └── ValueObjects/
│
├── tests/
│   ├── PKMDev.UnitTests/
│   └── PKMDev.IntegrationTests/
│
└── docs/
```

---

## Flujo de Datos

### Ejemplo: Crear un Knowledge Item

```
1. Usuario envía POST /Knowledge/Create
   ↓
2. KnowledgeController recibe CreateKnowledgeViewModel
   ↓
3. ModelState.IsValid valida datos
   ↓
4. AutoMapper convierte ViewModel → DTO
   ↓
5. KnowledgeService.CreateAsync(dto)
   ├── Validaciones de negocio
   ├── Mapper DTO → Entity
   └── Repository.AddAsync(entity)
   ↓
6. ApplicationDbContext.SaveChangesAsync()
   ↓
7. SQL Server persiste datos
   ↓
8. Controller redirige a Index
   ↓
9. Usuario ve lista actualizada
```

---

## Seguridad

### Autenticación y Autorización

```csharp
// Configuración en Program.cs
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.User.RequireUniqueEmail = true;
});

// Protección de controladores
[Authorize]
public class KnowledgeController : Controller
{
    // Solo usuarios autenticados pueden acceder
}

// Protección por rol
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Solo administradores
}
```

### Prevención de CSRF
```html
@* En formularios Razor *@
<form asp-action="Create" method="post">
    @Html.AntiForgeryToken()
    ...
</form>
```

### SQL Injection Prevention
Entity Framework parametriza automáticamente las consultas.

---

## Escalabilidad

### Optimizaciones de Performance

1. **Eager Loading vs Lazy Loading**
```csharp
// Eager Loading - carga relacionados en una query
var items = await _context.KnowledgeItems
    .Include(k => k.Tags)
    .Include(k => k.User)
    .ToListAsync();
```

2. **Paginación**
```csharp
public async Task<PagedResult<KnowledgeItemDto>> GetPagedAsync(
    int page, int pageSize)
{
    var total = await _context.KnowledgeItems.CountAsync();
    var items = await _context.KnowledgeItems
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<KnowledgeItemDto>
    {
        Items = _mapper.Map<List<KnowledgeItemDto>>(items),
        TotalCount = total,
        Page = page,
        PageSize = pageSize
    };
}
```

3. **Caching**
```csharp
builder.Services.AddMemoryCache();

// En servicios
private readonly IMemoryCache _cache;

public async Task<List<TagDto>> GetPopularTagsAsync()
{
    return await _cache.GetOrCreateAsync("popular_tags", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
        return await _tagRepository.GetMostUsedAsync(10);
    });
}
```

4. **Asynchronous Operations**
Todo el código usa `async/await` para no bloquear threads.

---

## Consideraciones Futuras

### Microservicios
La arquitectura en capas facilita migrar a microservicios:
- Knowledge Service → Microservicio independiente
- User Service → Microservicio independiente
- Comunicación vía API Gateway o Message Queue

### CQRS (Command Query Responsibility Segregation)
Separar comandos (escritura) de queries (lectura) para mayor escalabilidad.

### Event Sourcing
Almacenar eventos en lugar de estado para auditoría completa.

---

## Conclusión

Esta arquitectura proporciona:
- ✅ **Código limpio y mantenible**
- ✅ **Fácil testing**
- ✅ **Preparado para crecer**
- ✅ **Siguiendo mejores prácticas de la industria**

Es una arquitectura **de nivel empresarial** que demuestra madurez técnica y preparación para proyectos reales.
