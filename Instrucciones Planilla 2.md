\# agents.md — Implementación sugerida del motor de nómina (Concept Engine + Recalculo + Auditoría)



\## Propósito

Este bloque describe \*\*cómo implementar\*\* el motor de cálculo de nómina para que:

\- calcule por colaborador y por período,

\- registre \*\*desglose por concepto\*\* (ingresos/deducciones),

\- soporte \*\*recalcular\*\* sin duplicar datos,

\- mantenga \*\*auditoría completa\*\*,

\- permita \*\*parametrización anual\*\* (CCSS, renta, topes, etc.) sin cambiar código.



El diseño se apoya en:

\- Encabezado/Detalle de planilla y detalle por concepto (`PlanillaEncabezado`, `PlanillaDetalle`, `PlanillaDetalleConcepto`). :contentReference\[oaicite:0]{index=0}  

\- Catálogo de conceptos con fórmula y bandera de ingreso/deducción (`TipoConceptoNomina`, `ModosCalculoConceptoNomina`). :contentReference\[oaicite:1]{index=1}  

\- Flujo funcional de “Calcular Nómina” y “Recalcular Nómina”. :contentReference\[oaicite:2]{index=2}  



---



\## 1) Contratos de servicio sugeridos



\### 1.1 Orquestación de planilla

\- `INominaService`

&nbsp; - `CalcularPlanilla(iPlanillaId)`

&nbsp; - `RecalcularPlanilla(iPlanillaId)`

&nbsp; - `AprobarPlanilla(iPlanillaId)` (bloqueo lógico)

&nbsp; - `ObtenerResumenPlanilla(iPlanillaId)`



\### 1.2 Cálculo por colaborador

\- `INominaCalculator`

&nbsp; - `CalcularEmpleado(iPlanillaId, iEmpleadoId) => ResultadoNominaEmpleado`



\### 1.3 Motor de conceptos (reglas dinámicas)

\- `IConceptEngine`

&nbsp; - `EvaluarConceptosIngresos(ctx) => List<ConceptoAplicado>`

&nbsp; - `EvaluarConceptosDeducciones(ctx) => List<ConceptoAplicado>`



\### 1.4 Módulos de apoyo (fuentes)

\- `IAsistenciaProvider` (ausencias, horas trabajadas si aplica)

\- `IHorasExtraProvider` (solo aprobadas)

\- `IPermisosProvider`

\- `IIncapacidadesProvider`

\- `IVacacionesProvider`

\- `IParametrosLegalesProvider` (año/fecha)

\- `IDeduccionesInternasProvider` (préstamos, solidarista, embargos)

\- `IBitacoraService` (auditoría)



---



\## 2) Estados y reglas de bloqueo



\### Estados recomendados de planilla

\- Pendiente de Cálculo

\- Calculada

\- Aprobada

\- Pagada



Reglas:

\- \*\*Calcular\*\*: solo si estado = “Pendiente de Cálculo”. :contentReference\[oaicite:3]{index=3}  

\- \*\*Recalcular\*\*: solo si estado = “Calculada” (o “Pendiente de Cálculo”, según política). :contentReference\[oaicite:4]{index=4}  

\- \*\*Aprobar\*\*: solo si estado = “Calculada”. :contentReference\[oaicite:5]{index=5}  

\- \*\*Pagada\*\*: no se recalcula.



---



\## 3) Modelo de datos mínimo que el motor debe persistir



\### 3.1 Encabezado

`PlanillaEncabezado`:

\- PeriodoInicio

\- PeriodoFin

\- FechaPago

\- TipoPlanilla

\- Estado :contentReference\[oaicite:6]{index=6}  



\### 3.2 Detalle por colaborador

`PlanillaDetalle`:

\- PlanillaId

\- EmpleadoId

\- SalarioBase

\- TotalIngresos

\- TotalDeducciones

\- SalarioBruto

\- SalarioNeto :contentReference\[oaicite:7]{index=7}  



\### 3.3 Desglose por concepto

`PlanillaDetalleConcepto`:

\- PlanillaDetalleId

\- TipoConceptoId

\- Monto :contentReference\[oaicite:8]{index=8}  



\### 3.4 Catálogo de conceptos

`TipoConceptoNomina`:

\- Nombre

\- ModoCalculoId

\- FormulaCalculo

\- EsIngreso

\- EsDeduccion

\- Estado :contentReference\[oaicite:9]{index=9}  



---



\## 4) Recalcular sin duplicar: estrategia obligatoria



\### Regla clave

Antes de recalcular un colaborador, se deben eliminar (o versionar) los conceptos existentes del detalle.



\#### Opción A (simple y recomendada): “Reemplazo total”

1\. Cargar `PlanillaDetalle` del colaborador.

2\. Borrar `PlanillaDetalleConcepto` asociados a ese `PlanillaDetalleId`.

3\. Recalcular todo.

4\. Insertar nuevamente conceptos.

5\. Actualizar totales del `PlanillaDetalle`.



\#### Opción B (auditoría extendida): “Versionado”

\- Agregar columnas:

&nbsp; - `VersionCalculo`

&nbsp; - `EsVigente`

\- Marcar anteriores como no vigentes y crear una nueva versión.



> Si el proyecto tiene alcance de TFG, Opción A suele ser suficiente; la auditoría se cubre con bitácora.



---



\## 5) Contexto de cálculo (DTO interno recomendado)



\### `NominaContext`

\- `PlanillaId`

\- `PeriodoInicio`

\- `PeriodoFin`

\- `FechaPago`

\- `EmpleadoId`

\- `SalarioMensual` / `SalarioHora`

\- `FechaIngreso`, `FechaSalida`

\- `HorasExtraAprobadas\[]`

\- `Permisos\[]`

\- `Incapacidades\[]`

\- `Vacaciones\[]`

\- `Asistencia\[]` (o ausencias calculadas)

\- `ParametrosLegales` (CCSS, renta, créditos)

\- `DeduccionesInternas\[]`

\- `PoliticasEmpresa` (base 30/26, redondeo, prioridad deducciones)



---



\## 6) Orden del cálculo por colaborador (procedimiento exacto)



\### Paso 0 — Validaciones mínimas

\- El colaborador debe estar activo en el período (por fecha ingreso/salida y estado).

\- La planilla debe estar en estado permitido. :contentReference\[oaicite:10]{index=10}  



\### Paso 1 — Determinar salario base del período (SB)

1\. Determinar modalidad:

&nbsp;  - mensual → usar `Empleado.SalarioBase` (o `Puesto.SalarioBase` si aplica). :contentReference\[oaicite:11]{index=11}  

&nbsp;  - hora → calcular con tarifa/hora.

2\. Aplicar prorrateos:

&nbsp;  - ingreso posterior a inicio

&nbsp;  - salida anterior a fin

3\. Aplicar descuentos por tiempo no pagado:

&nbsp;  - ausencias injustificadas

&nbsp;  - permisos sin goce



\*\*Resultado\*\*: `SB` (salario base ya ajustado al período)



\### Paso 2 — Calcular ingresos (TI)

Ingresos deben provenir de:

\- Horas extra aprobadas (por `TipoHoraExtra.PorcentajePago`) :contentReference\[oaicite:12]{index=12}  

\- Bonos / comisiones / ajustes (conceptos configurables)



\*\*Persistencia\*\*: cada ingreso se registra como un `PlanillaDetalleConcepto`.



\### Paso 3 — Calcular salario bruto (BR)

\- `BR = SB + TI`



\### Paso 4 — Calcular deducciones legales

\- CCSS obrera (componentes parametrizados)

\- Banco Popular

\- Impuesto al salario (tramos parametrizados; cálculo progresivo)

\- Créditos fiscales (si aplica)



\*\*Persistencia\*\*: cada deducción legal debe quedar como concepto.



\### Paso 5 — Calcular deducciones internas (empresa)

\- Solidarista

\- Préstamos

\- Embargos

\- Otros



\*\*Importante\*\*: aplicar \*\*prioridad de deducciones\*\* (ver sección 8).



\### Paso 6 — Total deducciones y neto

\- `TD = sum(deducciones)`

\- `NETO = BR - TD`



\### Paso 7 — Guardar totales

Actualizar `PlanillaDetalle`:

\- SalarioBase = SB

\- TotalIngresos = TI

\- TotalDeducciones = TD

\- SalarioBruto = BR

\- SalarioNeto = NETO :contentReference\[oaicite:13]{index=13}  



\### Paso 8 — Bitácora

Registrar:

\- usuario que ejecuta

\- fecha/hora

\- planilla id

\- colaborador id

\- acción: Calcular/Recalcular/Aprobar

\- resumen (bruto, neto, deducciones)



---



\## 7) Concept Engine: modos de cálculo soportados (mínimo)



En `ModosCalculoConceptoNomina` se recomienda soportar al menos:

1\. \*\*Monto fijo\*\*

2\. \*\*Porcentaje del bruto\*\*

3\. \*\*Porcentaje del salario base\*\*

4\. \*\*Fórmula evaluada (expresiones)\*\*

5\. \*\*Regla especializada (plugin)\*\*



El concepto (`TipoConceptoNomina`) define:

\- si es ingreso o deducción,

\- cómo se calcula,

\- fórmula (si aplica). :contentReference\[oaicite:14]{index=14}  



---



\## 8) Prioridad de deducciones (orden de aplicación)



Se recomienda una tabla:

\- `PrioridadDeduccion`

&nbsp; - `TipoConceptoId`

&nbsp; - `Prioridad` (1..N)

&nbsp; - `EsObligatoria` (bool)

&nbsp; - `PermiteParcial` (bool)

&nbsp; - `TopeMaximo` (decimal, nullable)



Reglas:

\- Deducciones \*\*legales\*\* primero (CCSS, renta).

\- Luego embargos/órdenes judiciales (si política lo define).

\- Luego préstamos/solidarista/otros.

\- Si una deducción no cabe por neto mínimo (si se implementa), se aplica parcial si `PermiteParcial=1`.



> Si el alcance no contempla “neto mínimo”, al menos se debe permitir ordenar y aplicar deducciones sin conflicto.



---



\## 9) Parametrización anual (evitar cambios de código)



Se recomienda que el motor consulte parámetros por la \*\*fecha de pago\*\* o por el \*\*año del período\*\*:



\### Tablas sugeridas

\- `ParametrosLegalesAnuales`

&nbsp; - Año

&nbsp; - SEM\_Trabajador

&nbsp; - IVM\_Trabajador

&nbsp; - BancoPopular\_Trabajador

&nbsp; - CreditoHijo

&nbsp; - CreditoConyuge



\- `TramoRentaSalario`

&nbsp; - Año

&nbsp; - Desde

&nbsp; - Hasta (nullable)

&nbsp; - Tasa



Regla:

\- Si el período cruza años, usar la fecha de pago como referencia (o parametrizar esta decisión).



---



\## 10) Cálculo de renta (algoritmo progresivo)



El motor debe calcular así:

1\. Determinar `baseRentaMensual`.

2\. Aplicar tramos en orden:

&nbsp;  - para cada tramo: `montoTramo = min(base, hasta) - desde`

&nbsp;  - `impuestoTramo = montoTramo \* tasa`

3\. Aplicar créditos:

&nbsp;  - `impuesto = max(0, impuesto - creditos)`



Para planillas quincenales/semanales:

\- Convertir base del período a equivalente mensual o parametrizar tramos por período.

\- Recomendación: usar equivalente mensual para retención y luego prorratear, dejando la regla como configurable.



---



\## 11) Incapacidades: implementación recomendada



Para evitar ambigüedades legales en un TFG:

\- Modelar incapacidad como concepto:

&nbsp; - “Descuento por incapacidad (días no pagados por empresa)”

&nbsp; - “Subsidio incapacidad (monto cubierto)”



Si `Incapacidades.MontoCubierto` existe, se usa como entrada directa. :contentReference\[oaicite:15]{index=15}  

Si no existe, el cálculo debe leer una configuración por `TipoIncapacidad` (porcentaje, días, etc.).



---



\## 12) Limpieza de datos al recalcular (procedimiento exacto)



Para cada colaborador:

1\. Leer `PlanillaDetalle` existente; si no existe, crearlo.

2\. Eliminar `PlanillaDetalleConcepto` del detalle. :contentReference\[oaicite:16]{index=16}  

3\. Ejecutar cálculo (sección 6).

4\. Insertar conceptos (ingresos y deducciones).

5\. Actualizar totales.



Para recalcular toda la planilla:

\- repetir por cada colaborador incluido en el encabezado. :contentReference\[oaicite:17]{index=17}  



---



\## 13) Reglas de consistencia (para evitar “sorpresas”)

\- Un concepto nunca debe ser a la vez ingreso y deducción (`EsIngreso` XOR `EsDeduccion`). :contentReference\[oaicite:18]{index=18}  

\- Un concepto inactivo no se aplica.

\- Horas extra solo si están aprobadas. :contentReference\[oaicite:19]{index=19}  

\- Permisos y vacaciones no deben solaparse inválidamente (validación previa).

\- Si faltan datos, el motor debe:

&nbsp; - registrar en bitácora,

&nbsp; - aplicar política: “omitir” / “cero” / “bloquear cálculo”.



---



\## 14) Plantilla de bitácora (mínimo)

Registrar eventos en `Bitacora`:

\- UsuarioId

\- Fecha

\- Acción (CALCULAR\_PLANILLA, RECALCULAR\_PLANILLA, APROBAR\_PLANILLA, CALCULAR\_EMPLEADO)

\- Descripción (JSON breve: bruto, neto, total deducciones, total ingresos) :contentReference\[oaicite:20]{index=20}  



---



\## 15) Checklist de pruebas (unitarias y de integración)

1\. empleado mensual sin novedades.

2\. empleado con horas extra de 50% y 100%.

3\. empleado con ausencia injustificada 1 día.

4\. empleado con permiso sin goce 1 día.

5\. empleado con ingreso a mitad de período.

6\. empleado con salida a mitad de período.

7\. empleado con deducciones internas múltiples con prioridad.

8\. empleado con incapacidad con `MontoCubierto`.

9\. recalcular y validar que no duplica conceptos.

10\. aprobar y validar bloqueo de cambios.



---



\## 16) Resumen operativo para Codex (lo que debe hacer el agente)

\- Implementar servicios `NominaService` y `NominaCalculator`.

\- Implementar `ConceptEngine` con al menos: monto fijo, porcentaje, fórmula.

\- Persistir resultados en:

&nbsp; - `PlanillaDetalle` (totales) :contentReference\[oaicite:21]{index=21}  

&nbsp; - `PlanillaDetalleConcepto` (desglose) :contentReference\[oaicite:22]{index=22}  

\- Implementar “recalcular” con eliminación previa de conceptos.

\- Implementar bitácora detallada.

\- Asegurar validaciones por estado de planilla. :contentReference\[oaicite:23]{index=23}  

