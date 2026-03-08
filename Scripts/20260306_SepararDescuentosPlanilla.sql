DECLARE @IdEstadoActivo int =
(
    SELECT TOP 1 IdEstado
    FROM Estado
    WHERE Estado = 1 OR Nombre = 'Activo'
    ORDER BY CASE WHEN Nombre = 'Activo' THEN 0 ELSE 1 END, IdEstado
);

DECLARE @IdModoFormula int =
(
    SELECT TOP 1 IdModoCalculoConceptoNomina
    FROM ModoCalculoConceptoNomina
    WHERE UPPER(Nombre) = 'FORMULA'
    ORDER BY IdModoCalculoConceptoNomina
);

IF @IdEstadoActivo IS NULL OR @IdModoFormula IS NULL
BEGIN
    RAISERROR('No se encontraron IdEstadoActivo o IdModoFormula.', 16, 1);
    RETURN;
END;

MERGE TipoConceptoNomina AS T
USING (
    SELECT
        CodigoConcepto = 'PERMISO_SIN_GOCE',
        Nombre = 'Descuento Permiso sin goce',
        IdModoCalculo = @IdModoFormula,
        CodigoFormula = 'PERMISO_SIN_GOCE',
        FormulaCalculo = 'Dias permiso sin goce * salario diario',
        ValorPorcentaje = CAST(NULL AS decimal(9,6)),
        ValorFijo = CAST(NULL AS decimal(10,2)),
        OrdenCalculo = 121,
        EsIngreso = CAST(0 AS bit),
        EsDeduccion = CAST(1 AS bit),
        IdEstado = @IdEstadoActivo

    UNION ALL

    SELECT
        CodigoConcepto = 'INCAPACIDAD',
        Nombre = 'Descuento Incapacidad',
        IdModoCalculo = @IdModoFormula,
        CodigoFormula = 'INCAPACIDAD',
        FormulaCalculo = 'Dias incapacidad no pagables * salario diario',
        ValorPorcentaje = CAST(NULL AS decimal(9,6)),
        ValorFijo = CAST(NULL AS decimal(10,2)),
        OrdenCalculo = 122,
        EsIngreso = CAST(0 AS bit),
        EsDeduccion = CAST(1 AS bit),
        IdEstado = @IdEstadoActivo
) AS S
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
        T.IdEstado = S.IdEstado
WHEN NOT MATCHED BY TARGET THEN
    INSERT (CodigoConcepto, Nombre, IdModoCalculo, CodigoFormula, FormulaCalculo, ValorPorcentaje, ValorFijo, OrdenCalculo, EsIngreso, EsDeduccion, IdEstado)
    VALUES (S.CodigoConcepto, S.Nombre, S.IdModoCalculo, S.CodigoFormula, S.FormulaCalculo, S.ValorPorcentaje, S.ValorFijo, S.OrdenCalculo, S.EsIngreso, S.EsDeduccion, S.IdEstado);
GO
