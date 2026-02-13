# 🧠 Personal Knowledge Manager para Desarrolladores (PKM-Dev)

Aplicación web diseñada para que los desarrolladores organicen conocimiento técnico, fragmentos de código, notas, conceptos y roadmaps de aprendizaje en una plataforma centralizada.  
Este proyecto fue desarrollado como parte de mi portfolio profesional para demostrar arquitectura empresarial, diseño de bases de datos, buenas prácticas de desarrollo y despliegue en la nube con .NET.

---

## 🚀 Demo
> *(Agregar URL cuando esté desplegado en Azure)*  
Ejemplo: https://pkm-dev.azurewebsites.net

---

## 📌 Descripción del Proyecto

Los desarrolladores suelen guardar conocimiento en múltiples lugares (Notion, Google Docs, GitHub Gists, archivos locales, etc.), lo que genera fragmentación y pérdida de información.  
**PKM-Dev** propone una solución centralizada enfocada exclusivamente en conocimiento técnico, permitiendo construir un “segundo cerebro” digital para programadores.

---

## ✨ Funcionalidades Principales

### 📚 Base de Conocimiento
- Almacenamiento de notas técnicas y snippets de código  
- Editor Markdown  
- Sistema de etiquetas (relación N:M)  
- Búsqueda y filtrado avanzado  

---

### 🗺️ Roadmap Personal de Aprendizaje
- Tablero estilo Kanban (Planificado / En Progreso / Completado)  
- Priorización de tareas  
- Seguimiento del progreso de aprendizaje  

---

### 📊 Panel de Analíticas
- Crecimiento del conocimiento a lo largo del tiempo  
- Lenguajes de programación más utilizados  
- Etiquetas más frecuentes  
- Estadísticas de actividad del usuario  

---

### 🤖 Insights Inspirados en IA (Simulado)
- Sugerencia automática de etiquetas  
- Detección de snippets duplicados o similares  
- Recordatorios de temas no revisados (repetición espaciada)  

---

### 🔐 Autenticación y Seguridad
- Registro e inicio de sesión de usuarios  
- Autorización basada en roles (Usuario / Administrador)  
- Hash seguro de contraseñas con ASP.NET Identity  

---

## 🏗️ Arquitectura del Sistema

El proyecto sigue una **arquitectura en capas**:

- **Capa de Presentación (ASP.NET MVC)**  
- **Capa de Lógica de Negocio (Servicios, DTOs, Validaciones)**  
- **Capa de Acceso a Datos (Entity Framework Core, Repositorios)**  
- **Aspectos Transversales (Logging, Mapeo, Seguridad)**  

La arquitectura está diseñada para ser escalable, mantenible y alineada con prácticas empresariales.

---

## 🧰 Stack Tecnológico

### Backend
- C# .NET 8  
- ASP.NET MVC  
- Entity Framework Core  
- SQL Server  

### Frontend
- Razor Views  
- Bootstrap 5 / Tailwind CSS  
- JavaScript + jQuery  
- Chart.js  

### Nube y DevOps
- Azure App Service  
- Azure SQL Database  
- GitHub Actions (CI/CD)  
- Docker (opcional)  

---

## 🗄️ Modelo de Base de Datos (Simplificado)

- **User**  
- **KnowledgeItem**  
- **Tag**  
- **KnowledgeItemTag** (relación N:M)  
- **RoadmapItem**  
- **AIInsight** (simulado)  

---

## 📐 Diagrama de Arquitectura (Simplificado)

UI (ASP.NET MVC)
↓
Business Layer (Services, DTOs, Validation)
↓
Data Access Layer (EF Core, Repositories)
↓
SQL Server Database

---

## 🛠️ Instalación y Configuración

### 1️⃣ Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/pkm-dev.git
cd pkm-dev

👨‍💻 Autor
Brio By Bombieri
Software Developer | C# .NET | Backend & Web Development
LinkedIn: https://www.linkedin.com/in/rodrigobombieri-dev/
GitHub: https://github.com/RodrigoBombieri
