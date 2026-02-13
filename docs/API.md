# 📡 Referencia de API - PKM-Dev

## Tabla de Contenidos
- [Introducción](#introducción)
- [Autenticación](#autenticación)
- [Knowledge Items API](#knowledge-items-api)
- [Tags API](#tags-api)
- [Roadmap API](#roadmap-api)
- [Dashboard API](#dashboard-api)
- [AI Insights API](#ai-insights-api)
- [Códigos de Estado](#códigos-de-estado)
- [Ejemplos de Uso](#ejemplos-de-uso)

---

## Introducción

Esta documentación describe las rutas y endpoints principales de la aplicación PKM-Dev. Aunque es una aplicación MVC tradicional (no REST API pura), documentamos aquí las acciones de los controladores para referencia.

### Base URL
```
Desarrollo: https://localhost:5001
Producción: https://pkm-dev.azurewebsites.net
```

### Formato de Respuesta
La aplicación retorna vistas HTML (Razor), pero puede extenderse para soportar JSON mediante content negotiation.

---

## Autenticación

### Registro de Usuario

**Endpoint**: `POST /Account/Register`

**Descripción**: Registra un nuevo usuario en el sistema.

**Parámetros**:
```csharp
public class RegisterViewModel
{
    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}
```

**Respuesta Exitosa**:
- Redirección a `/Account/Login`
- Usuario creado en base de datos

**Errores**:
- `400 Bad Request`: Validación fallida
- `409 Conflict`: Email o username ya existe

---

### Inicio de Sesión

**Endpoint**: `POST /Account/Login`

**Descripción**: Autentica un usuario existente.

**Parámetros**:
```csharp
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}
```

**Respuesta Exitosa**:
- Cookie de autenticación configurada
- Redirección a `/Dashboard`

**Errores**:
- `401 Unauthorized`: Credenciales inválidas

---

### Cerrar Sesión

**Endpoint**: `POST /Account/Logout`

**Descripción**: Cierra la sesión del usuario actual.

**Autorización**: Usuario autenticado

**Respuesta**: Redirección a `/Home/Index`

---

## Knowledge Items API

### Listar Knowledge Items

**Endpoint**: `GET /Knowledge/Index`

**Descripción**: Lista todos los items de conocimiento del usuario autenticado.

**Autorización**: `[Authorize]`

**Parámetros Query** (opcionales):
```
?type=Snippet              // Filtrar por tipo (Snippet, Note, Link, Concept)
&language=CSharp           // Filtrar por lenguaje
&tag=dotnet               // Filtrar por tag
&search=entity framework   // Búsqueda de texto
&page=1                   // Página (para paginación)
&pageSize=20              // Items por página
```

**Ejemplo**:
```
GET /Knowledge/Index?type=Snippet&language=CSharp&page=1
```

**Respuesta**: Vista con lista de `KnowledgeItemViewModel`

---

### Ver Detalle de Knowledge Item

**Endpoint**: `GET /Knowledge/Details/{id}`

**Descripción**: Muestra el detalle de un item específico.

**Autorización**: `[Authorize]` - Solo el propietario

**Parámetros**:
- `id` (int): ID del knowledge item

**Ejemplo**:
```
GET /Knowledge/Details/123
```

**Respuesta**: Vista con `KnowledgeItemViewModel`

**Errores**:
- `404 Not Found`: Item no existe
- `403 Forbidden`: No es propietario del item

---

### Crear Knowledge Item

**Endpoint**: `POST /Knowledge/Create`

**Descripción**: Crea un nuevo item de conocimiento.

**Autorización**: `[Authorize]`

**Parámetros**:
```csharp
public class CreateKnowledgeViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }  // Markdown

    [Required]
    public KnowledgeType Type { get; set; }  // Snippet, Note, Link, Concept

    public string Language { get; set; }  // CSharp, JavaScript, Python, etc.

    public List<string> Tags { get; set; }  // Lista de tags
}
```

**Ejemplo de datos**:
```json
{
    "title": "Entity Framework Core DbContext Pattern",
    "content": "```csharp\npublic class ApplicationDbContext : DbContext\n{\n    ...\n}\n```",
    "type": "Snippet",
    "language": "CSharp",
    "tags": ["entity-framework", "dotnet", "database"]
}
```

**Respuesta Exitosa**:
- Item creado con ID asignado
- Redirección a `/Knowledge/Index`

**Errores**:
- `400 Bad Request`: Validación fallida
- `409 Conflict`: Título duplicado para el usuario

---

### Actualizar Knowledge Item

**Endpoint**: `POST /Knowledge/Edit/{id}`

**Descripción**: Actualiza un item existente.

**Autorización**: `[Authorize]` - Solo el propietario

**Parámetros**:
```csharp
public class EditKnowledgeViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    public KnowledgeType Type { get; set; }
    public string Language { get; set; }
    public List<string> Tags { get; set; }
}
```

**Respuesta Exitosa**:
- Item actualizado
- Campo `UpdatedAt` modificado
- Redirección a `/Knowledge/Details/{id}`

**Errores**:
- `404 Not Found`: Item no existe
- `403 Forbidden`: No es propietario

---

### Eliminar Knowledge Item

**Endpoint**: `POST /Knowledge/Delete/{id}`

**Descripción**: Elimina un item (soft delete recomendado).

**Autorización**: `[Authorize]` - Solo el propietario

**Parámetros**:
- `id` (int): ID del item a eliminar

**Respuesta Exitosa**:
- Item marcado como eliminado o removido de BD
- Redirección a `/Knowledge/Index`

**Errores**:
- `404 Not Found`: Item no existe
- `403 Forbidden`: No es propietario

---

### Compartir Knowledge Item

**Endpoint**: `GET /Knowledge/Share/{shareToken}`

**Descripción**: Vista pública de un snippet compartido.

**Autorización**: No requiere (público)

**Parámetros**:
- `shareToken` (string): Token único generado al compartir

**Ejemplo**:
```
GET /Knowledge/Share/a8f3k2j9d7s6
```

**Respuesta**: Vista pública de solo lectura

---

## Tags API

### Listar Todos los Tags

**Endpoint**: `GET /Tags/Index`

**Descripción**: Lista todos los tags del usuario con conteo de uso.

**Autorización**: `[Authorize]`

**Respuesta**: Vista con lista de `TagViewModel`

```csharp
public class TagViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int UsageCount { get; set; }  // Cantidad de items con este tag
}
```

---

### Crear Tag

**Endpoint**: `POST /Tags/Create`

**Descripción**: Crea un nuevo tag.

**Autorización**: `[Authorize]`

**Parámetros**:
```csharp
public class CreateTagViewModel
{
    [Required]
    [StringLength(50)]
    [RegularExpression(@"^[a-z0-9-]+$")]  // Solo minúsculas, números y guiones
    public string Name { get; set; }
}
```

**Respuesta Exitosa**: Redirección a `/Tags/Index`

**Errores**:
- `409 Conflict`: Tag ya existe

---

### Eliminar Tag

**Endpoint**: `POST /Tags/Delete/{id}`

**Descripción**: Elimina un tag y sus relaciones.

**Autorización**: `[Authorize]`

**Parámetros**:
- `id` (int): ID del tag

**Respuesta**: Redirección a `/Tags/Index`

---

### Ver Items por Tag

**Endpoint**: `GET /Tags/Items/{tagName}`

**Descripción**: Lista items que tienen un tag específico.

**Autorización**: `[Authorize]`

**Ejemplo**:
```
GET /Tags/Items/entity-framework
```

**Respuesta**: Vista con items filtrados

---

## Roadmap API

### Listar Roadmap Items

**Endpoint**: `GET /Roadmap/Index`

**Descripción**: Muestra el roadmap personal en formato Kanban.

**Autorización**: `[Authorize]`

**Respuesta**: Vista con items agrupados por estado

```csharp
public class RoadmapViewModel
{
    public List<RoadmapItemViewModel> Planned { get; set; }
    public List<RoadmapItemViewModel> InProgress { get; set; }
    public List<RoadmapItemViewModel> Done { get; set; }
}
```

---

### Crear Roadmap Item

**Endpoint**: `POST /Roadmap/Create`

**Descripción**: Agrega un nuevo item al roadmap.

**Autorización**: `[Authorize]`

**Parámetros**:
```csharp
public class CreateRoadmapViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    public RoadmapStatus Status { get; set; }  // Planned, InProgress, Done

    public Priority Priority { get; set; }  // Low, Medium, High
}
```

**Respuesta**: Redirección a `/Roadmap/Index`

---

### Actualizar Estado

**Endpoint**: `POST /Roadmap/UpdateStatus/{id}`

**Descripción**: Cambia el estado de un item (mover en Kanban).

**Autorización**: `[Authorize]`

**Parámetros**:
```csharp
public class UpdateStatusViewModel
{
    public int Id { get; set; }
    public RoadmapStatus NewStatus { get; set; }
}
```

**Ejemplo AJAX**:
```javascript
// Mover item a "In Progress"
fetch('/Roadmap/UpdateStatus/42', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id: 42, newStatus: 1 })
});
```

**Respuesta**: `200 OK` con JSON

---

## Dashboard API

### Vista Principal del Dashboard

**Endpoint**: `GET /Dashboard/Index`

**Descripción**: Muestra estadísticas y métricas del usuario.

**Autorización**: `[Authorize]`

**Respuesta**: Vista con `DashboardViewModel`

```csharp
public class DashboardViewModel
{
    public int TotalKnowledgeItems { get; set; }
    public int TotalTags { get; set; }
    public int ActiveRoadmapItems { get; set; }
    
    public Dictionary<string, int> ItemsByType { get; set; }
    public Dictionary<string, int> ItemsByLanguage { get; set; }
    
    public List<RecentActivityViewModel> RecentActivity { get; set; }
    public List<KnowledgeItemViewModel> MostViewed { get; set; }
}
```

---

### Obtener Datos para Gráficos

**Endpoint**: `GET /Dashboard/GetChartData`

**Descripción**: Retorna datos en JSON para Chart.js.

**Autorización**: `[Authorize]`

**Parámetros Query**:
```
?chartType=growth      // growth, languages, types
&period=30            // Días a incluir
```

**Ejemplo**:
```
GET /Dashboard/GetChartData?chartType=growth&period=90
```

**Respuesta JSON**:
```json
{
    "labels": ["Jan", "Feb", "Mar", "Apr"],
    "datasets": [{
        "label": "Knowledge Items Created",
        "data": [5, 12, 8, 15],
        "backgroundColor": "rgba(75, 192, 192, 0.2)",
        "borderColor": "rgba(75, 192, 192, 1)"
    }]
}
```

---

## AI Insights API

### Obtener Sugerencias

**Endpoint**: `GET /AI/Suggestions/{knowledgeItemId}`

**Descripción**: Obtiene sugerencias de IA para un item.

**Autorización**: `[Authorize]`

**Parámetros**:
- `knowledgeItemId` (int): ID del item

**Respuesta JSON**:
```json
{
    "suggestions": [
        {
            "type": "duplicate_detection",
            "message": "Este snippet es similar a 'LINQ Query Patterns'",
            "relatedItemId": 87
        },
        {
            "type": "tag_suggestion",
            "message": "Considera agregar el tag 'linq'",
            "suggestedTags": ["linq", "collections"]
        },
        {
            "type": "review_reminder",
            "message": "No has revisado este concepto en 45 días",
            "daysSinceLastAccess": 45
        }
    ]
}
```

---

### Detectar Duplicados

**Endpoint**: `POST /AI/DetectDuplicates`

**Descripción**: Analiza y detecta snippets duplicados o similares.

**Autorización**: `[Authorize]`

**Respuesta JSON**:
```json
{
    "duplicates": [
        {
            "item1Id": 23,
            "item1Title": "Repository Pattern Implementation",
            "item2Id": 45,
            "item2Title": "Generic Repository",
            "similarityScore": 0.87
        }
    ]
}
```

---

## Códigos de Estado

### Exitosos
- `200 OK`: Solicitud exitosa
- `201 Created`: Recurso creado exitosamente
- `204 No Content`: Eliminación exitosa

### Redirecciones
- `302 Found`: Redirección temporal (usado en MVC)

### Errores del Cliente
- `400 Bad Request`: Datos inválidos o validación fallida
- `401 Unauthorized`: No autenticado
- `403 Forbidden`: Autenticado pero sin permisos
- `404 Not Found`: Recurso no encontrado
- `409 Conflict`: Conflicto (ej: duplicado)
- `422 Unprocessable Entity`: Regla de negocio violada

### Errores del Servidor
- `500 Internal Server Error`: Error no manejado
- `503 Service Unavailable`: Servicio temporalmente no disponible

---

## Ejemplos de Uso

### Ejemplo 1: Crear un Snippet de Código

**Escenario**: Usuario quiere guardar un snippet de Entity Framework.

```http
POST /Knowledge/Create
Content-Type: application/x-www-form-urlencoded

Title=DbContext+Configuration&
Content=```csharp%0Apublic+class+AppDbContext...&
Type=Snippet&
Language=CSharp&
Tags=entity-framework,dotnet,database
```

**Respuesta**:
```
HTTP/1.1 302 Found
Location: /Knowledge/Index
```

---

### Ejemplo 2: Buscar Snippets por Tag

**Escenario**: Ver todos los snippets relacionados con "async-await".

```http
GET /Knowledge/Index?tag=async-await
```

**Respuesta**: Vista HTML con lista filtrada

---

### Ejemplo 3: Mover Item en Roadmap (AJAX)

**JavaScript**:
```javascript
async function moveToInProgress(itemId) {
    const response = await fetch(`/Roadmap/UpdateStatus/${itemId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({
            id: itemId,
            newStatus: 1  // InProgress
        })
    });

    if (response.ok) {
        // Actualizar UI
        location.reload();
    }
}
```

---

### Ejemplo 4: Obtener Datos del Dashboard

**Fetch con JavaScript**:
```javascript
async function loadGrowthChart() {
    const response = await fetch('/Dashboard/GetChartData?chartType=growth&period=30');
    const data = await response.json();
    
    // Renderizar con Chart.js
    new Chart(ctx, {
        type: 'line',
        data: data,
        options: { /* ... */ }
    });
}
```

---

## Extensión a REST API

Para convertir PKM-Dev en una API REST completa, considera:

### 1. Agregar API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
public class KnowledgeApiController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<KnowledgeItemDto>>> GetAll()
    {
        // Retorna JSON en lugar de Vista
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeItemDto>> Create(
        [FromBody] CreateKnowledgeDto dto)
    {
        // Retorna JSON
    }
}
```

### 2. Configurar Content Negotiation
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

### 3. Documentar con Swagger
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ...

app.UseSwagger();
app.UseSwaggerUI();
```

---

## Conclusión

Esta documentación cubre las rutas y acciones principales de PKM-Dev. Para una API REST completa, considera implementar API Controllers adicionales con documentación Swagger/OpenAPI.

**Recursos adicionales**:
- [Arquitectura del Sistema](ARCHITECTURE.md)
- [Guía de Desarrollo](DEVELOPMENT.md)
- [Guía de Despliegue](DEPLOYMENT.md)
