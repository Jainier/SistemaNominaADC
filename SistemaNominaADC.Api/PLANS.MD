# PLANS.md — SistemaNominaADC (Plan ejecutable para Codex)

## Objetivo general
Implementar el sistema de Nómina y RRHH por iteraciones pequeñas, preservando la arquitectura real:
Presentación (Blazor + Radzen) → API Controllers → Negocio (Services) → Datos (EF Core).

## Regla clave de este plan
Antes de construir un módulo nuevo, Codex debe:
1) Buscar si ya existe (Entidad/DbSet/Service/Controller/UI/Cliente HTTP).
2) Si existe, revisarlo y aplicar mejoras puntuales sin romper compatibilidad.
3) Si NO existe, implementarlo siguiendo el patrón actual del proyecto.

---

# Iteración 0 — Auditoría de base (obligatoria, sin features nuevas)

## Propósito
Estabilizar y alinear contratos entre Presentación y API para que las siguientes iteraciones avancen sin fricción.

## Trabajo requerido
1) Inventario de módulos existentes
   - API: Controllers existentes y rutas reales.
   - Negocio: Services existentes.
   - Datos: DbSets y relaciones existentes en ApplicationDbContext.
   - Presentación: Pages + clientes HTTP + componentes compartidos.
2) Identificar inconsistencias FE/BE:
   - Login: qué espera el frontend vs qué devuelve la API (campos como UserName/Roles).
   - Roles: DTOs y endpoints (crear/actualizar).
   - Endpoints mixtos (REST vs verbales): mapear los usados por UI.
3) Mejoras puntuales aplicables (sin refactor masivo):
   - Ajustes mínimos de DTO/response para que UI reciba lo que necesita.
   - Alinear nombres de propiedades y validaciones visibles en UI.
   - Consolidar/estandarizar respuestas ProblemDetails.
   - Agregar endpoint /health o /ping (si no existe) para smoke test.

## Criterio de aceptación
- Se puede iniciar Api + Presentación, loguear y usar módulos existentes sin errores.
- No hay 401 inesperados por token/handler en flujos existentes.

---

# Iteración 1 — Mantenimientos base para Nómina (crear o mejorar)

## Propósito
Dejar listos todos los catálogos necesarios para módulos operativos (Asistencia, Solicitudes, Nómina).

## Mantenimientos objetivo (según DER/TFG)
- Departamentos (ya existe → revisar/mejorar)
- Puestos (puede existir o no)
- Empleados (puede existir o no)
- Tipos:
  - TipoPermiso
  - TipoIncapacidad
  - TipoHoraExtra
- (Opcional si ya se modela) Horarios/Turnos

## Instrucciones a Codex (obligatorias)
1) Buscar primero: ¿ya existe cada mantenimiento?
   - Entidad/DTOs en Entidades
   - DbSet en ApplicationDbContext
   - Service en Negocio
   - Controller en Api
   - Page/Componentes/Cliente HTTP en Presentación
2) Si existe:
   - Revisar endpoints usados por la UI.
   - Aplicar mejoras puntuales (validaciones, coherencia de respuestas, duplicidad, dependencias).
   - NO romper contratos actuales.
3) Si no existe:
   - Implementar siguiendo el patrón actual (por capas) y reusar componentes (TablaMantenimiento/FormularioMantenimiento) si aplica.
4) Mantener validaciones de request en Controllers (ModelState + reglas).

## Reglas sugeridas por mantenimiento
- Departamentos:
  - No duplicidad por Nombre.
  - Desactivación lógica: impedir desactivar si hay Puestos activos asociados.
- Puestos:
  - No duplicidad por Nombre dentro del mismo Departamento si aplica.
  - Impedir desactivar si hay Empleados activos asociados.
- Empleados:
  - No duplicidad por Cédula.
  - Estado activo/inactivo.
- Tipos:
  - No duplicidad por Nombre.
  - Impedir desactivar si hay registros dependientes pendientes (según se implemente luego).

## Entregables
- CRUD completo por mantenimiento (Create/Update/Deactivate/List/ById).
- UI Radzen lista + formulario + confirmación de desactivar.
- Bitácora en create/update/deactivate si ya se tiene infraestructura lista.
- Migraciones EF Core solo si faltan tablas/columnas.

## Criterio de aceptación
- Todos los catálogos funcionan en UI y no rompen los mantenimientos actuales.
- Validaciones muestran mensajes claros (UI) y errores vuelven como ProblemDetails (API).

---

# Iteración 2 — Asistencia (marca entrada/salida)

## Propósito
Permitir registro de entrada/salida por colaborador con reglas anti-duplicidad y coherencia temporal.

## Trabajo
- Revisar si ya existe entidad Asistencia, endpoints o UI.
- Si existe: mejorar reglas y alineación.
- Si no existe: implementar:
  - Registrar entrada si no existe marca de entrada para el día.
  - Registrar salida si existe entrada y no existe salida.
  - Historial por colaborador y filtros por fecha.

## Criterio de aceptación
- Un empleado puede marcar entrada y luego salida el mismo día.
- No permite duplicidad inmediata.

---

# Iteración 3 — Solicitudes con aprobación (Horas Extra, Vacaciones, Permisos)

## Propósito
Flujos: Colaborador solicita → Jefatura aprueba/rechaza → RRHH visualiza si aplica.

## Trabajo
- Para cada módulo: revisar si ya existe algo; si existe, mejorar; si no, crear.
- Estados mínimos: Pendiente, Aprobada, Rechazada.
- Reglas:
  - Solapamientos (Vacaciones/Permisos/Incapacidades).
  - Vacaciones: validar saldo.
  - Horas extra: validar fecha razonable y motivo.
- UI:
  - Colaborador: crear y ver estado.
  - Jefatura: ver pendientes y aprobar/rechazar (motivo obligatorio al rechazar).
  - RRHH: vista consolidada si aplica.

## Criterio de aceptación
- Flujo completo en UI para los 3 módulos.

---

# Iteración 4 — Incapacidades (con adjunto + validación RRHH)

## Propósito
Registrar incapacidad con adjunto, validar o rechazar por RRHH.

## Trabajo
- Revisar si ya existe entidad/incapacidad UI.
- Definir almacenamiento de adjuntos (FileSystem o DB) sin romper despliegue.
- Estados: Registrada, Validada, Rechazada.
- UI: registro (colaborador) + validación/rechazo (RRHH).

## Criterio de aceptación
- Subida y consulta de documento funcionan.
- Cambios de estado quedan registrados.

---

# Iteración 5 — Nómina (Registrar / Calcular / Recalcular / Aprobar)

## Propósito
Implementar planilla por período con flujo completo.

## Trabajo
- Registrar planilla: impedir duplicidad por período.
- Calcular:
  - salario base
  - horas extra aprobadas
  - deducciones/otros conceptos si están modelados
- Recalcular
- Aprobar (congelar)

## Criterio de aceptación
- Se puede registrar, calcular y aprobar una planilla desde UI.

---

# Iteración 6 — Aguinaldo
- Cálculo masivo por período
- Revisión por colaborador
- Recalcular si hubo correcciones

---

# Iteración 7 — Liquidaciones
- Registrar proceso
- Calcular
- Aprobar (Contabilidad)
- Bloqueo posterior

---

# Iteración 8 — Evaluación de rendimiento
- Registro por Jefatura
- Consulta por RRHH/Jefatura

---

# Iteración 9 — Reportes/Consultas
- Reportes principales (planilla, vacaciones, horas extra, incapacidades, liquidaciones)
- Filtros por período/empleado/estado
- Exportación si está contemplada

---

## Definition of Done (para cada iteración)
1) dotnet build OK (Api + Presentacion + Negocio + Datos + Entidades).
2) Endpoints probados desde UI o verificación mínima por request.
3) Errores se devuelven como ProblemDetails.
4) No se rompen contratos existentes del frontend.
5) Cambios pequeños y justificados (sin refactor masivo).
