# agents.md — Motor de Cálculo de Nómina (Costa Rica · Sector Privado)

## Objetivo
Este documento define, de forma exhaustiva, **todo lo que el motor de nómina debe contemplar** para calcular el salario **bruto, deducciones y salario neto** por colaborador y por período, cubriendo escenarios típicos y excepcionales del sector privado en Costa Rica.

El sistema ya contempla el flujo “Registrar nómina” y “Calcular nómina”, incluyendo recuperación de salario base, deducciones, horas extra y otros rubros. :contentReference[oaicite:0]{index=0}  
La base de datos incluye entidades para almacenar encabezado/detalle de planilla y conceptos por colaborador (ej. `PlanillaDetalle`, `PlanillaDetalleConcepto`, `TipoConceptoNomina`). :contentReference[oaicite:1]{index=1}

---

## Principios obligatorios (no negociables)
1. **Nada “hardcodeado” por año**: porcentajes y tramos deben ser **parametrizables** (ej. por tabla `ParametrosLegalesAnuales`).
2. **Trazabilidad total**: cada cálculo debe generar desglose de ingresos/deducciones por concepto en `PlanillaDetalleConcepto`.
3. **Reglas por estado**: solo se calcula si la planilla está en estado permitido (ej. “Pendiente de Cálculo”). :contentReference[oaicite:2]{index=2}
4. **Precisión financiera**: redondeo a 2 decimales, con reglas consistentes de redondeo.
5. **Reproducibilidad**: recalcular una nómina debe producir exactamente el mismo resultado si los datos de entrada no cambiaron.

---

## Definiciones
- **Salario Base (SB)**: salario contractual del colaborador para el período.
- **Total Ingresos (TI)**: suma de ingresos adicionales (horas extra, bonos, comisiones, ajustes).
- **Salario Bruto (BR)**: `BR = SB + TI`.
- **Total Deducciones (TD)**: suma de deducciones legales + deducciones internas.
- **Salario Neto (NETO)**: `NETO = BR - TD`.

---

## Entradas mínimas del cálculo por colaborador
- Salario base y modalidad (mensual / por hora).
- Fechas de ingreso/salida (si aplica).
- Asistencia/ausencias dentro del período.
- Permisos (con goce / sin goce).
- Vacaciones aprobadas.
- Horas extra aprobadas.
- Incapacidades (tipo, rango de fechas, y si existe “monto cubierto”).
- Otros ingresos (bonos, comisiones, ajustes).
- Deducciones internas (préstamos, solidarista, embargos, etc.).
- Parámetros legales vigentes (por año/fecha del período).

---

## Normalización del período
El motor debe soportar:
- Semanal
- Quincenal
- Mensual

### Cálculos base recomendados (para prorrateos)
- **Salario diario (si salario mensual):** `salarioDiario = salarioMensual / 30`
- **Salario hora (si salario mensual):** `salarioHora = salarioMensual / 240` (30 días * 8 horas)
- **Salario hora (si salario por hora):** provisto por contrato/catálogo.

> Nota: si la empresa maneja 26 días para planilla en algunos casos, se parametriza. El motor debe permitir definir la base (30, 26 u otra) por política interna.

---

## Reglas de ingreso al salario (Ingresos)
### 1) Horas extra (solo aprobadas)
Debe tomarse de la entidad de horas extra en estado “Aprobada”. :contentReference[oaicite:3]{index=3} :contentReference[oaicite:4]{index=4}

- El catálogo `TipoHoraExtra` incluye porcentaje de pago (`PorcentajePago`) por tipo. :contentReference[oaicite:5]{index=5}
- Fórmula genérica:
  - `montoHoraExtra = horas * salarioHora * (1 + porcentajePago)`
  - Ejemplo: 50% extra → factor 1.5

### 2) Bonos / Comisiones / Incentivos
- Deben modelarse como **conceptos de nómina** (`TipoConceptoNomina`), marcados como ingreso (`EsIngreso = 1`). :contentReference[oaicite:6]{index=6}
- Pueden ser:
  - Monto fijo
  - Porcentaje
  - Fórmula configurable

### 3) Ajustes manuales positivos
- Ingresos extraordinarios (ej. reintegros).
- Deben requerir auditoría (bitácora).

---

## Reglas de reducción del salario (Descuentos por tiempo no pagado)
Estas reglas impactan el SB (o generan un concepto “Descuento por ausencia” como deducción).

### 1) Ausencia injustificada
- Descuento: `diasAusentes * salarioDiario` (o por horas si se controla por horas).
- Debe existir evidencia en asistencia (`Ausencia = 1`) y/o marca faltante. :contentReference[oaicite:7]{index=7}

### 2) Permisos sin goce salarial
- Descuento por días/horas del permiso.
- Permisos con goce salarial **no descuentan**.

### 3) Entrada o salida a mitad de período (prorrateo)
- Si fecha ingreso > inicio período: pagar solo días trabajables dentro del período.
- Si fecha salida < fin período: pagar solo hasta la fecha de salida.
- Prorrateo recomendado:
  - `SB_prorrateado = salarioDiario * diasPagables`

> “Días pagables” debe definirse por política: calendario (naturales) o laborables. Parametrizable.

---

## Vacaciones
- Vacaciones aprobadas se pagan como salario normal (no deben reducir el salario).
- Debe reducirse el saldo de vacaciones (módulo correspondiente).

---

## Incapacidades (escenarios)
El motor debe soportar varios enfoques porque la forma exacta puede variar por política y tipo de incapacidad:

### A) Incapacidad con monto explícito (recomendado)
Si la entidad `Incapacidades` trae `MontoCubierto`, el motor:
1. Calcula el salario “normal” del período (o días).
2. Registra un concepto “Subsidio incapacidad” por el `MontoCubierto` cuando aplique.
3. Ajusta el SB por los días no pagados por la empresa, si corresponde.

### B) Incapacidad por reglas (si no hay monto)
Debe permitirse una configuración por `TipoIncapacidad`:
- % pagado por patrono
- % pagado por entidad externa
- días cubiertos por patrono
- topes/reglas

> El motor no debe inventar porcentajes: todo debe ser parametrizable por tipo.

---

## Deducciones legales (Costa Rica, vigentes 2026)
> Estas deducciones se aplican sobre la base definida por ley (en la práctica, suele ser el salario reportado/devengado del período). La base exacta debe parametrizarse por concepto.

### 1) CCSS — Cuota obrera 2026 (trabajador)
- SEM: 5.50%
- IVM: 4.33% (aumento desde 2026)
- Banco Popular (aporte trabajador): 1.00%
- **Total trabajador:** 10.83% :contentReference[oaicite:8]{index=8}

Fórmulas:
- `ded_SEM = baseCCSS * 0.0550`
- `ded_IVM = baseCCSS * 0.0433`
- `ded_BP  = baseCCSS * 0.0100`
- `ded_CCSS_total = ded_SEM + ded_IVM + ded_BP`

> El aporte del 1% al Banco Popular está respaldado en normativa/ley (aporta el trabajador). :contentReference[oaicite:9]{index=9}

### 2) Impuesto al salario (retención mensual) — Tramos 2026
Los tramos oficiales 2026 para asalariados (mensual) incluyen: exento hasta ¢918.000 y escalas 10%, 15%, 20%, 25% por excedentes. :contentReference[oaicite:10]{index=10}

Tramos (mensual):
- Hasta ¢918.000 → 0%
- Exceso ¢918.000 hasta ¢1.347.000 → 10%
- Exceso ¢1.347.000 hasta ¢2.364.000 → 15%
- Exceso ¢2.364.000 hasta ¢4.727.000 → 20%
- Exceso mayor a ¢4.727.000 → 25% :contentReference[oaicite:11]{index=11}

Créditos fiscales mensuales 2026 (si se implementan):
- Por hijo: ¢1.710
- Por cónyuge: ¢2.590 :contentReference[oaicite:12]{index=12}

**Regla**: el impuesto es progresivo por tramo (solo sobre el excedente).

---

## Deducciones internas (no legales, pero comunes en sector privado)
Estas deben existir como `TipoConceptoNomina` (EsDeduccion=1) y ser configurables:
- Asociación solidarista (porcentaje del salario o monto fijo).
- Ahorros voluntarios / cooperativa.
- Préstamos (cuota fija por período).
- Embargos (reglas y prioridades).
- Pólizas / seguros.
- Pensión complementaria voluntaria (si se implementa).
- Comedor / uniformes / adelantos.

**Prioridad de rebajos**: debe ser parametrizable (una tabla de prioridad por concepto). En Costa Rica se menciona que existen órdenes de prioridad en rebajos de planilla; el sistema debe permitir configurar el orden. :contentReference[oaicite:13]{index=13}

---

## Aportes patronales (para costos contables, no para salario neto)
Aunque no rebajan al trabajador, se recomienda calcularlos para reportes/contabilidad:
- CCSS patronal (SEM, IVM, etc.)
- FODESAF, INA, IMAS, INS Riesgos del Trabajo, Banco Popular patronal, FCL, pensión complementaria obligatoria, etc.
Ejemplos de listados y totales de cargas patronales se reportan en fuentes legales/consultoras y varían por componente. :contentReference[oaicite:14]{index=14}

> Estas cargas NO deben restarse del NETO del colaborador. Solo se registran como costo patronal.

---

## Reglas de redondeo y precisión
- Redondear cada concepto a 2 decimales.
- Evitar acumulación de error:
  - calcular con decimal
  - redondear al final por concepto
  - luego sumar

---

## Regla de “congelamiento” por aprobación
- Si una nómina está “Aprobada”, no se recalcula ni se modifica salvo rol especial y bitácora.

---

## Validaciones antes de calcular
Antes de calcular una planilla:
1. La planilla debe existir y estar en estado permitido (“Pendiente de Cálculo”). :contentReference[oaicite:15]{index=15}
2. No debe existir otra nómina para el mismo período. :contentReference[oaicite:16]{index=16}
3. Se deben cargar solo colaboradores activos dentro del período.
4. Horas extra deben estar aprobadas (rechazadas no se incluyen). :contentReference[oaicite:17]{index=17}
5. Permisos/vacaciones/incapacidades no deben solaparse inválidamente (reglas de integridad).

---

## Salidas (persistencia esperada)
Por cada colaborador, persistir:
- `PlanillaDetalle`:
  - SalarioBase
  - TotalIngresos
  - TotalDeducciones
  - SalarioBruto
  - SalarioNeto :contentReference[oaicite:18]{index=18}
- `PlanillaDetalleConcepto` (desglose):
  - TipoConceptoId
  - Monto :contentReference[oaicite:19]{index=19}

---

## Pseudocódigo del cálculo (alto nivel)
Para cada `Empleado` incluido en `PlanillaEncabezado`:

1) Determinar base del período:
- salarioMensual o salarioHora
- prorrateo por ingreso/salida
- descuentos por ausencias/permisos sin goce (según política)

2) Calcular ingresos:
- horas extra aprobadas (por tipo)
- bonos / comisiones / ajustes

3) Calcular BR:
- `BR = SB + TI`

4) Calcular deducciones legales:
- CCSS obrera: SEM + IVM + Banco Popular (según parámetros 2026)
- impuesto al salario: por tramos 2026 + créditos (si aplica)

5) Calcular deducciones internas:
- según conceptos configurados y prioridad

6) NETO:
- `NETO = BR - TD`

7) Persistir detalle y conceptos + bitácora

---

## Parámetros legales recomendados (estructura sugerida)
Tabla: `ParametrosLegalesAnuales`
- Año
- SEM_Trabajador
- IVM_Trabajador
- BancoPopular_Trabajador
- TramosRenta (json o tabla hija)
- CreditoHijo
- CreditoConyuge

Tabla hija: `TramoRentaSalario`
- Año
- Desde
- Hasta (nullable)
- Tasa

> Para 2026, usar: exento hasta ¢918.000; luego 10/15/20/25 por excedentes. :contentReference[oaicite:20]{index=20}

---

## Casos borde que el motor debe cubrir (checklist)
- Colaborador sin marcas de asistencia (ausencia).
- Colaborador con permisos sin goce parciales (por horas).
- Colaborador con múltiples tipos de horas extra en el mismo período.
- Colaborador con ingreso a mitad de período.
- Colaborador con salida a mitad de período.
- Colaborador con incapacidad parcial dentro del período.
- Colaborador con vacaciones y horas extra (si la política lo permite).
- Colaborador con deducciones internas múltiples (préstamo + solidarista + embargo).
- Nómina recalculada: debe regenerar conceptos y bitácora.
- Nómina aprobada: bloquear edición y recalculo salvo rol especial.

---

## Nota de cumplimiento
El cálculo debe alinearse con normativa de Ministerio de Trabajo y parámetros vigentes de CCSS y Hacienda, por lo que el sistema debe permitir actualización anual sin cambios de código. :contentReference[oaicite:21]{index=21}