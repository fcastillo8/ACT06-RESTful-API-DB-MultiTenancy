# ACT06 - Actividad Grupal: RESTful API + DB MultiTenancy

## 📋 Descripción

API segura en **ASP.NET Core 6** que implementa **autenticación JWT**, **multitenancy** a nivel de base de datos y configuración para **despliegue en Railway** con **SQL Server**.

## 🏗️ Arquitectura (Clean Architecture)

```
/Solution
 ├── Api/                          # Capa de presentación
 │    ├── Controllers/
 │    │    ├── AuthController.cs    # Endpoints de autenticación
 │    │    └── ProductsController.cs # CRUD de productos (multitenancy)
 │    ├── Models/                   # ViewModels / DTOs
 │    │    ├── AuthModels.cs
 │    │    └── ProductModels.cs
 │    └── Program.cs               # Configuración del pipeline
 │
 ├── Application/                   # Lógica de negocio
 │    ├── Interfaces/               # Contratos
 │    │    ├── IUserRepository.cs
 │    │    ├── IProductRepository.cs
 │    │    ├── ITokenService.cs
 │    │    ├── ITenantService.cs
 │    │    └── IPasswordResetRepository.cs
 │    └── Services/
 │         ├── AuthService.cs       # Lógica de autenticación
 │         └── ProductService.cs    # Lógica de productos
 │
 ├── Domain/                        # Entidades del dominio
 │    ├── Entities/
 │    │    ├── BaseEntity.cs        # Entidad base con TenantId
 │    │    ├── User.cs
 │    │    ├── Product.cs
 │    │    └── PasswordResetRequest.cs
 │    └── ValueObjects/
 │         └── TenantInfo.cs
 │
 ├── Infrastructure/                # Implementaciones
 │    ├── Data/
 │    │    └── ApplicationDbContext.cs  # DbContext con filtros globales
 │    ├── Repositories/
 │    │    ├── UserRepository.cs
 │    │    ├── ProductRepository.cs
 │    │    └── PasswordResetRepository.cs
 │    ├── Logging/
 │    │    └── SerilogConfig.cs     # Configuración de Serilog
 │    └── Security/
 │         ├── TokenService.cs      # Generador de JWT
 │         └── TenantService.cs     # Resolución de tenant
 │
 ├── Tests/                         # Tests unitarios
 │    └── AuthServiceTests.cs
 │
 ├── Dockerfile                     # Despliegue en Railway
 ├── railway.toml                   # Configuración de Railway
 └── MultiTenantApi.sln
```

## 🔐 1. Autenticación JWT

### Endpoints

| Método | Endpoint | Auth | Descripción |
|--------|----------|------|-------------|
| `POST` | `/api/Auth/Login` | ❌ No | Recibe usuario, contraseña y tenantId. Devuelve JWT |
| `POST` | `/api/Auth/CambioDeClave` | ✅ JWT | Cambia la contraseña del usuario autenticado |
| `POST` | `/api/Auth/OlvideMiClave` | ❌ No | Simula envío de correo de recuperación (se registra en Serilog) |
| `POST` | `/api/Auth/Register` | ❌ No | Registra un nuevo usuario en un tenant |

### Claims del JWT Token
```json
{
  "sub": "admin",
  "email": "admin@tenanta.com",
  "tenantId": "tenant-a",
  "username": "admin",
  "role": "Admin"
}
```

### Ejemplo de Login
```json
POST /api/Auth/Login
{
  "username": "admin",
  "password": "Admin123!",
  "tenantId": "tenant-a"
}
```

**Respuesta:**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "message": "Login exitoso."
}
```

## 🏢 2. Multitenancy

### Estrategia: Tabla compartida con `TenantId`

Todas las entidades heredan de `BaseEntity` que incluye un campo `TenantId`. El `DbContext` aplica **filtros globales (Global Query Filters)** para que cada usuario solo acceda a los datos de su tenant.

```csharp
// Filtro global en DbContext
entity.HasQueryFilter(e => e.TenantId == _tenantId);
```

### Tenants de prueba (seed data)

| Tenant | Usuario | Contraseña | Rol |
|--------|---------|------------|-----|
| `tenant-a` | `admin` | `Admin123!` | Admin |
| `tenant-a` | `user1` | `User123!` | User |
| `tenant-b` | `admin` | `Admin123!` | Admin |
| `tenant-b` | `user1` | `User123!` | User |

### Productos CRUD (protegidos por JWT)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| `GET` | `/api/Products` | Lista productos del tenant |
| `GET` | `/api/Products/{id}` | Obtiene un producto |
| `POST` | `/api/Products` | Crea un producto |
| `PUT` | `/api/Products/{id}` | Actualiza un producto |
| `DELETE` | `/api/Products/{id}` | Elimina un producto |

## 📊 Serilog

- Logs en **consola** y **archivos rotativos** (`logs/log-YYYYMMDD.txt`)
- Registra todas las solicitudes HTTP
- Registra intentos de login (exitosos y fallidos)
- Registra solicitudes de restablecimiento de contraseña simuladas

## 🚀 3. Despliegue en Railway

### Variables de entorno requeridas

| Variable | Descripción |
|----------|-------------|
| `JWT_Key` | Clave secreta para firmar tokens JWT (mín. 32 caracteres) |
| `ConnectionStrings_DefaultConnection` | Connection string de SQL Server |

### Pasos para desplegar

1. **Crear proyecto en Railway** → [railway.app](https://railway.app)
2. **Agregar plugin SQL Server** en Railway
3. **Conectar repositorio GitHub**: `fcastillo8/ACT06-RESTful-API-DB-MultiTenancy`
4. **Configurar variables de entorno**:
   - `JWT_Key` = Tu clave secreta
   - `ConnectionStrings_DefaultConnection` = Connection string del plugin SQL Server
5. Railway desplegará automáticamente usando el `Dockerfile`

## ⚙️ Ejecución Local

```bash
# Clonar el repositorio
git clone https://github.com/fcastillo8/ACT06-RESTful-API-DB-MultiTenancy.git
cd ACT06-RESTful-API-DB-MultiTenancy

# Restaurar dependencias
dotnet restore

# Ejecutar la API
dotnet run --project Api

# Acceder a Swagger
# http://localhost:5000
```

### Ejecutar Tests
```bash
dotnet test
```

## 🛠️ Tecnologías

- **ASP.NET Core 6** - Framework web
- **Entity Framework Core** - ORM con SQL Server
- **JWT Bearer** - Autenticación
- **Serilog** - Logging estructurado
- **BCrypt** - Hash de contraseñas
- **Swagger/OpenAPI** - Documentación interactiva
- **xUnit + Moq** - Testing
- **Docker** - Containerización
- **Railway** - Despliegue en la nube

## 👥 Equipo

- Integrante 1
- Integrante 2
- Integrante 3

## 📄 Licencia

Este proyecto es parte de una actividad académica.
