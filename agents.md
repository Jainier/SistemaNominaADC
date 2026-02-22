# AGENTS.md — SistemaNominaADC

## Contexto del repositorio
Solución por capas/proyectos:
- SistemaNominaADC.Api (ASP.NET Core Web API)
- SistemaNominaADC.Presentacion (Blazor Server + Radzen)
- SistemaNominaADC.Negocio (servicios de negocio, reglas, excepciones)
- SistemaNominaADC.Datos (EF Core + ApplicationDbContext + migraciones)
- SistemaNominaADC.Entidades (entidades de dominio + DTOs)
- SistemaNominaADC.Utilidades (auxiliar)

Flujo real:
Presentación (Blazor) → HttpClients → API Controllers → Servicios Negocio → EF Core DbContext/SQL Server

Base técnica existente:
- Identity + JWT en API
- Token en Presentación usando ProtectedLocalStorage (SessionService)
- AuthorizationMessageHandler adjunta Bearer token a HttpClient
- GlobalExceptionHandler produce ProblemDetails
- UI con Radzen y componentes reutilizables (FormularioMantenimiento/TablaMantenimiento/SelectorEstado)
- Router configurado en Routes.razor

## Reglas NO negociables
1) Mantener compatibilidad explícita con .NET 8.0.
2) Mantener Router en Routes.razor (NO migrar a App.razor).
3) UI en Blazor Server usando Radzen (no cambiar librería UI).
4) Mantener el patrón de capas:
   - Validaciones de entrada en Controllers (ModelState + reglas de request).
   - Servicios de negocio para reglas y operaciones; excepciones BusinessException/NotFoundException.
5) Mantener GlobalExceptionHandler y respuestas en formato ProblemDetails.
6) No realizar refactors masivos ni renombrar namespaces/proyectos. Cambios pequeños y verificables.
7) Si un mantenimiento/módulo ya existe:
   - PRIMERO leerlo, entender contratos FE/BE,
   - proponer mejoras puntuales y aplicarlas sin romper compatibilidad.
   - evitar duplicar entidades/servicios/controladores existentes.
8) No romper contratos actuales del frontend:
   - Si se requiere cambiar contratos, preferir endpoints nuevos o mantener compatibilidad (overloads/DTOs).
9) Toda operación crítica debe registrar Bitácora cuando el diseño ya lo contemple (usuario, fecha, acción, descripción).
10) Mantener el estilo del proyecto aunque existan endpoints mixtos (REST puros vs acciones verbales); priorizar consistencia por módulo.

## Estándares de implementación (prácticos)
- Antes de escribir código:
  1) Enumerar archivos que se tocarán por proyecto.
  2) Explicar endpoints/contratos (request/response) y cambios de UI.
- Implementación por pasos:
  1) Datos (Entidades + DbContext + migración)
  2) Negocio (Service)
  3) API (Controller + validaciones)
  4) Presentación (Pages + Clientes HTTP + componentes Radzen)
- Al finalizar cada iteración:
  - Ejecutar dotnet build.
  - Proveer checklist de verificación manual en UI.
  - Confirmar que no se rompieron módulos existentes.

## Mejora continua de mantenimientos existentes (criterios)
Cuando se revise un mantenimiento existente, aplicar mejoras SOLO si:
- Corrige inconsistencia FE/BE (DTOs, nombres de campos, rutas).
- Elimina duplicidad de lógica o valida mejor (sin mover validaciones fuera del Controller).
- Mejora la coherencia de respuestas (ProblemDetails / códigos HTTP).
- Agrega validación faltante crítica (duplicidad, dependencias activas, estados).
- Mejora UX en Radzen (mensajes, loading, confirmaciones).
Evitar cambios cosméticos o reestructuración amplia.
