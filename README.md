# 🧠 Personal Knowledge Manager para Desarrolladores (PKM-Dev)

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Azure](https://img.shields.io/badge/Azure-0078D4?style=for-the-badge&logo=microsoft-azure&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

**Una plataforma SaaS de nivel profesional para que los desarrolladores organicen conocimiento técnico, snippets de código y roadmaps de aprendizaje.**

[Demo](#-demo) • [Características](#-características) • [Stack Tecnológico](#-stack-tecnológico) • [Instalación](#-instalación) • [Documentación](#-documentación)

</div>

---

## 📖 Descripción General

**PKM-Dev** es una aplicación web integral construida con ASP.NET MVC que funciona como tu "segundo cerebro" para conocimiento técnico. Diseñada específicamente para desarrolladores, combina gestión de snippets de código, notas técnicas, análisis de aprendizaje e insights asistidos por IA en una plataforma limpia y organizada.

### 🎯 Problema que Resuelve

Los desarrolladores típicamente dispersan su conocimiento en múltiples plataformas:
- 📝 Snippets de código en GitHub Gists
- 📚 Notas en Notion o Google Docs
- 🔖 Marcadores perdidos en el historial del navegador
- 💡 Recursos de aprendizaje en varias herramientas

**PKM-Dev** centraliza todo el conocimiento técnico en una plataforma poderosa y enfocada en desarrolladores.

### 💼 Contexto de Negocio

Este proyecto simula un producto SaaS realista de **DevMemory Inc.**, una startup ficticia dirigida a desarrolladores individuales y equipos pequeños con un modelo de negocio freemium.

---

## ✨ Características

### 🗂️ Gestión de Base de Conocimiento
- **Almacenamiento Organizado**: Gestiona snippets de código, notas técnicas, conceptos y enlaces
- **Soporte Markdown**: Editor markdown completo con resaltado de sintaxis
- **Multi-Lenguaje**: Soporte para C#, JavaScript, Python, SQL y más
- **Etiquetado Inteligente**: Relación N:M para categorización flexible
- **Búsqueda Avanzada**: Filtros por tipo, lenguaje, etiquetas y fechas

### 🗺️ Roadmap Personal de Aprendizaje
- **Tablero Kanban**: Gestión visual de tareas (Planeado → En Progreso → Completado)
- **Sistema de Prioridades**: Organiza objetivos de aprendizaje por importancia
- **Seguimiento de Progreso**: Monitorea tu crecimiento técnico

### 📊 Dashboard de Análisis
- **Métricas de Aprendizaje**: Rastrea el crecimiento de conocimiento en el tiempo
- **Estadísticas de Lenguajes**: Lenguajes de programación más utilizados
- **Tendencias de Actividad**: Gráficos impulsados por Chart.js
- **Insights de Engagement**: Tiempo desde último acceso a temas

### 🤖 Insights Potenciados por IA (Simulado)
- **Detección de Duplicados**: Identifica snippets de código similares
- **Auto-Etiquetado**: Sugerencias inteligentes de etiquetas
- **Repetición Espaciada**: Recordatorios para revisar conceptos antiguos
- **Brechas de Conocimiento**: Identifica áreas de mejora

### 🔐 Seguridad y Autenticación
- **ASP.NET Identity**: Autenticación segura de usuarios
- **Acceso Basado en Roles**: Roles de Admin y Usuario
- **Privacidad de Datos**: Bases de conocimiento aisladas por usuario

### 🌐 Compartir y Colaboración
- **Enlaces Públicos**: Comparte snippets con colegas
- **Opciones de Exportación**: Descarga conocimiento en varios formatos

---

## 🛠️ Stack Tecnológico

### Backend
| Tecnología | Propósito |
|-----------|-----------|
| **ASP.NET Core 10 MVC** | Framework web |
| **Entity Framework Core** | ORM y acceso a datos |
| **SQL Server** | Base de datos (LocalDB + Azure SQL) |
| **AutoMapper** | Mapeo objeto-objeto |
| **ASP.NET Identity** | Autenticación y autorización |

### Frontend
| Tecnología | Propósito |
|-----------|-----------|
| **Razor Views** | Renderizado del lado del servidor |
| **Bootstrap 5** | Framework UI responsive |
| **JavaScript/jQuery** | Interactividad del lado del cliente |
| **Chart.js** | Visualización de datos |
| **Editor Markdown** | Edición de texto enriquecido |

### Testing y Calidad
- **xUnit/NUnit**: Framework de pruebas unitarias
- **Moq**: Librería de mocking
- **Cobertura de Código**: Suite de pruebas completa

### Cloud y DevOps
- **Azure App Service**: Hosting web
- **Azure SQL Database**: Base de datos de producción
- **Docker**: Contenerización (opcional)
- **GitHub Actions**: Pipeline CI/CD

---

## 🏗️ Arquitectura

El proyecto sigue un patrón de **arquitectura limpia en capas** para mantenibilidad y escalabilidad:

```
┌─────────────────────────────────────────┐
│     Capa de Presentación (MVC)          │
│  ┌──────────┐  ┌──────────────────┐    │
│  │Controllers│ │ Views (Razor)    │    │
│  │           │  │ ViewModels       │    │
│  └──────────┘  └──────────────────┘    │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│     Capa de Lógica de Negocio (BLL)     │
│  ┌──────────┐  ┌──────────────────┐    │
│  │ Services │  │ Reglas de Negocio│    │
│  │          │  │ DTOs             │    │
│  └──────────┘  └──────────────────┘    │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│    Capa de Acceso a Datos (DAL)         │
│  ┌──────────────┐  ┌──────────────┐    │
│  │ Repositories │  │ EF Core      │    │
│  │              │  │ DbContext    │    │
│  └──────────────┘  └──────────────┘    │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│       Aspectos Transversales             │
│    Logging • Mapping • Security         │
└─────────────────────────────────────────┘
```

---

## 💾 Esquema de Base de Datos

### Entidades Principales

```sql
-- Gestión de Usuarios
User (Id, Username, Email, PasswordHash, CreatedAt)

-- Base de Conocimiento
KnowledgeItem (Id, Title, Content, Type, Language, CreatedAt, UpdatedAt, UserId)

-- Sistema de Etiquetas
Tag (Id, Name)
KnowledgeItemTag (KnowledgeItemId, TagId)  -- Relación N:M

-- Roadmap de Aprendizaje
RoadmapItem (Id, Title, Status, Priority, UserId)

-- Insights de IA
AIInsight (Id, KnowledgeItemId, SuggestionText, CreatedAt)
```

### Relaciones entre Entidades
- **User** → **KnowledgeItem** (1:N)
- **User** → **RoadmapItem** (1:N)
- **KnowledgeItem** ↔ **Tag** (N:M)
- **KnowledgeItem** → **AIInsight** (1:N)

---

## 🚀 Comenzando

### Requisitos Previos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) o SQL Server Express
- [Visual Studio 2022](https://visualstudio.microsoft.com/) o [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tuusuario/pkm-dev.git
   cd pkm-dev
   ```

2. **Configurar conexión a base de datos**
   
   Actualiza `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PKMDevDb;Trusted_Connection=true;"
     }
   }
   ```

3. **Aplicar migraciones**
   ```bash
   dotnet ef database update
   ```

4. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

5. **Acceder a la app**
   
   Navega a `https://localhost:5001`

### Docker (Opcional)

```bash
# Construir imagen
docker build -t pkm-dev .

# Ejecutar contenedor
docker run -p 8080:80 pkm-dev
```

---

## 📱 Capturas de Pantalla

### Dashboard
![Dashboard](docs/images/dashboard.png)
*Análisis y métricas de aprendizaje de un vistazo*

### Base de Conocimiento
![Base de Conocimiento](docs/images/knowledge-base.png)
*Organiza y busca tu conocimiento técnico*

### Roadmap Kanban
![Roadmap](docs/images/roadmap.png)
*Rastrea tu viaje de aprendizaje visualmente*

---

## 🧪 Testing

Ejecutar la suite de pruebas:

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true
```

---

## 🌐 Despliegue

### Despliegue en Azure

1. **Crear recursos de Azure**
   ```bash
   az group create --name PKMDevRG --location eastus
   az sql server create --name pkm-dev-server --resource-group PKMDevRG
   az webapp create --name pkm-dev-app --resource-group PKMDevRG
   ```

2. **Configurar CI/CD**
   
   El workflow de GitHub Actions está incluido en `.github/workflows/azure-deploy.yml`

3. **Desplegar**
   ```bash
   dotnet publish -c Release
   ```

Consulta la [documentación de despliegue](docs/DEPLOYMENT.md) para instrucciones detalladas.

---

## 📚 Documentación

- [Guía de Arquitectura](docs/ARCHITECTURE.md)
- [Referencia de API](docs/API.md)
- [Guía de Desarrollo](docs/DEVELOPMENT.md)
- [Guía de Despliegue](docs/DEPLOYMENT.md)

---

## 🗓️ Roadmap

### ✅ Completado
- [x] Operaciones CRUD básicas
- [x] Autenticación y Autorización
- [x] Sistema de gestión de etiquetas
- [x] Dashboard de análisis
- [x] Tablero Kanban de roadmap
- [x] Insights de IA simulados

### 🚧 En Progreso
- [ ] Búsqueda de texto completo con Elasticsearch
- [ ] Extensión de navegador para guardado rápido
- [ ] Integración con GitHub Gists

### 📋 Planeado
- [ ] Aplicación móvil (MAUI)
- [ ] Integración real de IA con OpenAI API
- [ ] Características de colaboración en equipo
- [ ] Opciones avanzadas de exportación
- [ ] API para integraciones de terceros

---

## 🤝 Contribuir

¡Las contribuciones son bienvenidas! Por favor sigue estos pasos:

1. Fork el repositorio
2. Crea una rama de feature (`git checkout -b feature/CaracteristicaIncreible`)
3. Commit tus cambios (`git commit -m 'Agrega una CaracteristicaIncreible'`)
4. Push a la rama (`git push origin feature/CaracteristicaIncreible`)
5. Abre un Pull Request

Por favor lee [CONTRIBUTING.md](CONTRIBUTING.md) para detalles sobre nuestro código de conducta y proceso de desarrollo.

---

## 📄 Licencia

Este proyecto está licenciado bajo la Licencia MIT - consulta el archivo [LICENSE](LICENSE) para más detalles.

---

## 👨‍💻 Autor

**Tu Nombre**

- LinkedIn: [tu-perfil](https://linkedin.com/in/tu-perfil)
- GitHub: [@tuusuario](https://github.com/tuusuario)
- Portfolio: [tu-portfolio.com](https://tu-portfolio.com)

---

## 🙏 Agradecimientos

- Inspirado por la necesidad de gestión de conocimiento enfocada en desarrolladores
- Construido como demostración de arquitectura ASP.NET MVC de nivel empresarial
- Gracias a la comunidad .NET por excelentes recursos y apoyo

---

## 📊 Estadísticas del Proyecto

![GitHub stars](https://img.shields.io/github/stars/tuusuario/pkm-dev?style=social)
![GitHub forks](https://img.shields.io/github/forks/tuusuario/pkm-dev?style=social)
![GitHub issues](https://img.shields.io/github/issues/tuusuario/pkm-dev)
![GitHub license](https://img.shields.io/github/license/tuusuario/pkm-dev)

---

<div align="center">

**⭐ ¡Si encuentras útil este proyecto, considera darle una estrella! ⭐**

Hecho con ❤️ y ☕ por un desarrollador apasionado
Brio By Bombieri
LinkedIn: https://www.linkedin.com/in/rodrigobombieri-dev/

</div>

