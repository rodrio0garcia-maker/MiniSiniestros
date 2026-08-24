# MiniSiniestros

Sistema de gestión de siniestros laborales para una ART: Web API REST + Backoffice MVC que comparten la misma capa de negocio. Challenge técnico para Andina ART.

## Stack

| Componente | Tecnología |
|---|---|
| Framework | .NET 8 |
| Web API | ASP.NET Core Web API |
| Backoffice | ASP.NET Core MVC + Razor Views |
| ORM | Entity Framework Core 8 (Code-First + Migrations) |
| Base de datos | SQL Server 2022 (contenedor) |
| Logging | Serilog (consola, JSON) |
| Documentación API | Swagger / Swashbuckle |
| Mapeo | AutoMapper |
| Auth | JWT Bearer (HS256) |
| Testing | xUnit + Moq + coverlet (con EF Core InMemoryDatabase para tests de integración) |
| Contenedores | Docker + Docker Compose |

## Cómo levantar todo

```bash
docker compose up --build
```

No requiere ningún paso manual adicional: SQL Server arranca con healthcheck, la Api aplica migraciones y siembra datos de prueba automáticamente al iniciar.

| Servicio | URL |
|---|---|
| Web API — Swagger | http://localhost:8080/swagger |
| Backoffice MVC | http://localhost:8081/Siniestros |
| SQL Server | `localhost,1434` (usuario `sa`, ver `docker-compose.yml`) |

> El puerto de SQL Server se mapeó a `1434` en vez de `1433` porque mi entorno de desarrollo ya tiene una instancia local escuchando en el puerto estándar. Internamente los contenedores se conectan por `sqlserver,1433` sin cambios.

### Login de prueba (JWT)

```json
POST /api/auth/login
{ "username": "operador", "password": "Operador123!" }
```

El token obtenido va en el header `Authorization: Bearer <token>`. El rol `Operador` es requerido para `POST /api/siniestros` y `PATCH /api/siniestros/{id}/estado`.

## Correr los tests y ver la cobertura

```bash
cd MiniSiniestros.Tests
dotnet test --collect:"XPlat Code Coverage" --settings:"..\coverlet.runsettings"
```

El reporte de cobertura final ya generado está en [`Reportes/index.html`](Reportes/index.html) (61% de líneas al momento de la última corrida completa; las migraciones autogeneradas se excluyen del cálculo por no ser código propio).

## Estructura del proyecto

```
MiniSiniestros.sln
├── MiniSiniestros.Api          → Web API
├── MiniSiniestros.Web          → Backoffice MVC
├── MiniSiniestros.Services     → Lógica de negocio, máquina de estados
├── MiniSiniestros.Data         → EF Core: DbContext, Repository, Unit of Work, Migrations/
├── MiniSiniestros.Entities     → Modelo de dominio (POCOs)
├── MiniSiniestros.Dto          → Contratos de la Web API
├── MiniSiniestros.ViewModels   → Contratos del Backoffice
└── MiniSiniestros.Tests        → xUnit + Moq + tests de integración con InMemory
```

**Desviación de la estructura sugerida**: las migraciones viven en una carpeta `Migrations/` dentro de `MiniSiniestros.Data`, en vez de un proyecto `MiniSiniestros.Data.Migrations` separado. Para el tamaño de este proyecto no justificaba un noveno proyecto solo para eso.

`Web` y `Api` no se conocen entre sí — ambos dependen directo de `Services`, sin pasar por HTTP. El Backoffice inyecta la lógica de negocio en el mismo proceso.

## Decisiones de diseño

- **Máquina de estados como diccionario** (`Dictionary<EstadoSiniestro, EstadoSiniestro[]>`), no una cadena de `if/else` — toda la lógica de transiciones válidas queda declarada en un solo lugar.
- **El historial de cambios de estado no se expone en el DTO de la API** (`GET /api/siniestros/{id}` solo pide "prestadores asignados"); sí se expone en el `ViewModel` del Backoffice, que pide explícitamente el timeline. Cada capa expone solo lo que su consumidor necesita.
- **`Dto` y `ViewModels` referencian `Entities` únicamente para reusar el enum `EstadoSiniestro`** — nunca las clases de entidad completas. Se evaluó un proyecto `Common` separado para evitar incluso esa dependencia mínima, pero no se justificaba para un solo tipo compartido.

## Qué no se implementó, y por qué

- **Notificación SRT + Polly** (retry/timeout/circuit breaker): sección marcada como opcional en el enunciado. Se priorizó el alcance obligatorio completo (Web API, Backoffice, testing, Docker) antes de invertir tiempo en esta parte.
- **Índices sobre `CuitEmpleador`/`CuilTrabajador`**: no se agregaron en la migración inicial. Para el volumen de este challenge no es un problema real; en un escenario con más datos, se agregarían.

## Uso de IA

Se utilizó Claude (Anthropic) como asistente durante el desarrollo, principalmente para:

- **Generación de código guiada** por decisiones de arquitectura tomadas por mí (qué patrón usar, cómo estructurar cada capa).
- **Explicación de conceptos nuevos** para mí en este proyecto: Razor Views con Tag Helpers (MVC, sin experiencia previa fuera de Blazor), multi-stage builds y orquestación con Docker Compose (healthcheck, depends_on con condition service healthy), configuración de Serilog, AutoMapper, y generación/lectura de reportes de cobertura con coverlet + ReportGenerator.
- **Debugging** de errores puntuales de configuración: paquete faltante de AutoMapper según versión, conflicto de puerto 1433 con mi SQL Server local, diferencia de comportamiento entre el proveedor InMemory y SQL Server al ordenar por un enum, entre otros.
