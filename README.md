# ACT06 - Actividad Grupal: RESTful API + DB MultiTenancy

## 📋 Descripción

Proyecto grupal para la creación de una **RESTful API** con soporte de **Base de Datos Multi-Tenancy**. Esta actividad tiene como objetivo implementar una arquitectura donde múltiples inquilinos (tenants) comparten la misma aplicación pero mantienen sus datos aislados.

## 🏗️ Arquitectura Multi-Tenancy

El proyecto implementa una estrategia de multi-tenancy a nivel de base de datos, permitiendo:

- **Aislamiento de datos** entre diferentes tenants
- **Escalabilidad** para múltiples organizaciones/clientes
- **Gestión centralizada** de la API con separación lógica de datos

## 🚀 Tecnologías

- **Backend**: RESTful API
- **Base de Datos**: Multi-Tenancy
- **Autenticación**: JWT / API Keys por tenant

## 📁 Estructura del Proyecto

```
├── src/
│   ├── controllers/       # Controladores de la API
│   ├── models/            # Modelos de datos
│   ├── routes/            # Definición de rutas
│   ├── middleware/         # Middleware (auth, tenant resolver)
│   ├── config/            # Configuración de la aplicación
│   └── database/          # Configuración y migraciones de BD
├── tests/                 # Tests unitarios e integración
├── docs/                  # Documentación adicional
└── README.md
```

## ⚙️ Instalación

```bash
# Clonar el repositorio
git clone https://github.com/<usuario>/ACT06-RESTful-API-DB-MultiTenancy.git

# Instalar dependencias
npm install

# Configurar variables de entorno
cp .env.example .env

# Ejecutar migraciones
npm run migrate

# Iniciar el servidor
npm run dev
```

## 🔑 Conceptos Clave de Multi-Tenancy

| Estrategia | Descripción |
|---|---|
| **Base de datos separada** | Cada tenant tiene su propia base de datos |
| **Esquema separado** | Cada tenant tiene su propio esquema dentro de la misma BD |
| **Tabla compartida** | Todos los tenants comparten tablas con un discriminador (tenant_id) |

## 📚 Endpoints de la API

| Método | Endpoint | Descripción |
|---|---|---|
| `GET` | `/api/v1/tenants` | Listar todos los tenants |
| `POST` | `/api/v1/tenants` | Crear un nuevo tenant |
| `GET` | `/api/v1/tenants/:id` | Obtener un tenant específico |
| `PUT` | `/api/v1/tenants/:id` | Actualizar un tenant |
| `DELETE` | `/api/v1/tenants/:id` | Eliminar un tenant |

## 👥 Equipo

- Integrante 1
- Integrante 2
- Integrante 3

## 📄 Licencia

Este proyecto es parte de una actividad académica.
