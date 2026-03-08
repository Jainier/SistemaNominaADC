/* ============================================================
   Seed/Ajuste catalogos de Planilla (idempotente)
   Requiere columnas nuevas en TipoConceptoNomina:
   - CodigoConcepto, CodigoFormula, ValorPorcentaje, ValorFijo, OrdenCalculo
   ============================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @IdEstadoActivo INT;

    SELECT TOP (1) @IdEstadoActivo = e.IdEstado
    FROM Estado e
    WHERE e.Nombre = 'Activo'
       OR e.Estado = 1
    ORDER BY CASE WHEN e.Nombre = 'Activo' THEN 0 ELSE 1 END, e.IdEstado;

    IF @IdEstadoActivo IS NULL
        THROW 50001, 'No se encontro el estado Activo.', 1;

    /* ========= 1) Modos de calculo ========= */
    DECLARE @Modos TABLE
    (
        Nombre      nvarchar(100) NOT NULL PRIMARY KEY,
        Descripcion nvarchar(250) NULL
    );

    INSERT INTO @Modos (Nombre, Descripcion)
    VALUES
    ('Fijo',       'Monto fijo definido para el concepto'),
    ('Porcentaje', 'Porcentaje sobre una base de calculo'),
    ('Formula',    'Calculado por el motor segun codigo formula'),
    ('Manual',     'Monto digitado por usuario'),
    ('Acumulador', 'Suma de otros conceptos');

    MERGE ModoCalculoConceptoNomina AS T
    USING @Modos AS S
      ON T.Nombre = S.Nombre
    WHEN MATCHED THEN
      UPDATE SET
        T.Descripcion = S.Descripcion,
        T.IdEstado = @IdEstadoActivo
    WHEN NOT MATCHED THEN
      INSERT (Nombre, Descripcion, IdEstado)
      VALUES (S.Nombre, S.Descripcion, @IdEstadoActivo);

    DECLARE
        @IdModoFijo INT = (SELECT IdModoCalculoConceptoNomina FROM ModoCalculoConceptoNomina WHERE Nombre = 'Fijo'),
        @IdModoPorcentaje INT = (SELECT IdModoCalculoConceptoNomina FROM ModoCalculoConceptoNomina WHERE Nombre = 'Porcentaje'),
        @IdModoFormula INT = (SELECT IdModoCalculoConceptoNomina FROM ModoCalculoConceptoNomina WHERE Nombre = 'Formula'),
        @IdModoManual INT = (SELECT IdModoCalculoConceptoNomina FROM ModoCalculoConceptoNomina WHERE Nombre = 'Manual'),
        @IdModoAcumulador INT = (SELECT IdModoCalculoConceptoNomina FROM ModoCalculoConceptoNomina WHERE Nombre = 'Acumulador');

    /* ========= 2) Tipos de planilla ========= */
    DECLARE @TiposPlanilla TABLE
    (
        Nombre      nvarchar(100) NOT NULL PRIMARY KEY,
        Descripcion nvarchar(100) NULL
    );

    INSERT INTO @TiposPlanilla (Nombre, Descripcion)
    VALUES
    ('Semanal', 'Planilla de pago semanal'),
    ('Quincenal', 'Planilla de pago quincenal'),
    ('Mensual', 'Planilla de pago mensual'),
    ('Extraordinaria', 'Planilla extraordinaria');

    MERGE TipoPlanilla AS T
    USING @TiposPlanilla AS S
      ON T.Nombre = S.Nombre
    WHEN MATCHED THEN
      UPDATE SET
        T.Descripcion = S.Descripcion,
        T.IdEstado = @IdEstadoActivo
    WHEN NOT MATCHED THEN
      INSERT (Nombre, Descripcion, IdEstado)
      VALUES (S.Nombre, S.Descripcion, @IdEstadoActivo);

    /* ========= 3) Conceptos de nomina ========= */
    DECLARE @Conceptos TABLE
    (
        CodigoConcepto nvarchar(40) NOT NULL PRIMARY KEY,
        Nombre nvarchar(150) NOT NULL,
        IdModoCalculo int NOT NULL,
        CodigoFormula nvarchar(60) NULL,
        FormulaCalculo nvarchar(1000) NULL,
        ValorPorcentaje decimal(9,6) NULL,
        ValorFijo decimal(10,2) NULL,
        OrdenCalculo int NOT NULL,
        EsIngreso bit NOT NULL,
        EsDeduccion bit NOT NULL
    );

    INSERT INTO @Conceptos
    (
        CodigoConcepto, Nombre, IdModoCalculo, CodigoFormula, FormulaCalculo,
        ValorPorcentaje, ValorFijo, OrdenCalculo, EsIngreso, EsDeduccion
    )
    VALUES
    ('SB',            'Salario Base',       @IdModoFormula,    'SB',          'Calculado por periodo y asistencias',        NULL,       NULL,  10, 1, 0),
    ('HE',            'Hora Extra',         @IdModoFormula,    'HORA_EXTRA',  'Horas * valor hora * factor',                NULL,       NULL,  20, 1, 0),
    ('BONO',          'Bonificacion',       @IdModoManual,     NULL,          NULL,                                          NULL,       NULL,  30, 1, 0),
    ('COMISION',      'Comision',           @IdModoManual,     NULL,          NULL,                                          NULL,       NULL,  40, 1, 0),
    ('AJUSTE_ING',    'Ajuste Ingreso',     @IdModoManual,     NULL,          NULL,                                          NULL,       NULL,  50, 1, 0),
    ('TI',            'Total Ingresos',     @IdModoAcumulador, 'TI',          'Suma de ingresos variables',                  NULL,       NULL,  60, 1, 0),
    ('BR',            'Salario Bruto',      @IdModoFormula,    'BR',          'Br = Sb + Ti',                                NULL,       NULL,  70, 1, 0),

    ('CCSS_SEM',      'CCSS Sem',           @IdModoPorcentaje, 'CCSS_SEM',    'Base Ccss * porcentaje',                      0.055000,   NULL,  80, 0, 1),
    ('CCSS_IVM',      'CCSS Ivm',           @IdModoPorcentaje, 'CCSS_IVM',    'Base Ccss * porcentaje',                      0.043300,   NULL,  90, 0, 1),
    ('CCSS_BP',       'CCSS Bp',            @IdModoPorcentaje, 'CCSS_BP',     'Base Ccss * porcentaje',                      0.010000,   NULL, 100, 0, 1),
    ('RENTA',         'Impuesto Renta',     @IdModoFormula,    'RENTA',       'Calculo progresivo por tramos vigentes',     NULL,       NULL, 110, 0, 1),
    ('AUSENCIA',      'Descuento Ausencia', @IdModoFormula,    'AUSENCIA',    'Dias no pagables * salario diario',          NULL,       NULL, 120, 0, 1),
    ('PRESTAMO',      'Prestamo Interno',   @IdModoManual,     NULL,          NULL,                                          NULL,       NULL, 130, 0, 1),
    ('AJUSTE_DED',    'Ajuste Deduccion',   @IdModoManual,     NULL,          NULL,                                          NULL,       NULL, 140, 0, 1),
    ('TD',            'Total Deducciones',  @IdModoAcumulador, 'TD',          'Suma de deducciones',                         NULL,       NULL, 150, 0, 1),

    ('NETO',          'Salario Neto',       @IdModoFormula,    'NETO',        'Neto = Br - Td',                              NULL,       NULL, 160, 1, 0);

    MERGE TipoConceptoNomina AS T
    USING @Conceptos AS S
      ON T.CodigoConcepto = S.CodigoConcepto
    WHEN MATCHED THEN
      UPDATE SET
        T.Nombre = S.Nombre,
        T.IdModoCalculo = S.IdModoCalculo,
        T.CodigoFormula = S.CodigoFormula,
        T.FormulaCalculo = S.FormulaCalculo,
        T.ValorPorcentaje = S.ValorPorcentaje,
        T.ValorFijo = S.ValorFijo,
        T.OrdenCalculo = S.OrdenCalculo,
        T.EsIngreso = S.EsIngreso,
        T.EsDeduccion = S.EsDeduccion,
        T.IdEstado = @IdEstadoActivo
    WHEN NOT MATCHED THEN
      INSERT
      (
        CodigoConcepto, Nombre, IdModoCalculo, CodigoFormula, FormulaCalculo,
        ValorPorcentaje, ValorFijo, OrdenCalculo, EsIngreso, EsDeduccion, IdEstado
      )
      VALUES
      (
        S.CodigoConcepto, S.Nombre, S.IdModoCalculo, S.CodigoFormula, S.FormulaCalculo,
        S.ValorPorcentaje, S.ValorFijo, S.OrdenCalculo, S.EsIngreso, S.EsDeduccion, @IdEstadoActivo
      );

    /* Si habia conceptos viejos sin codigo, se pueden conservar.
       Si quieres inactivar los no incluidos en seed, descomenta:
       UPDATE T
       SET T.IdEstado = @IdEstadoActivo
       FROM TipoConceptoNomina T
       WHERE T.CodigoConcepto IS NULL;
    */

    COMMIT TRAN;

    SELECT 'OK - Catalogos de planilla cargados y ajustados.' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err nvarchar(4000) = ERROR_MESSAGE();
    RAISERROR('Error en seed de catalogos de planilla: %s', 16, 1, @Err);
END CATCH;

