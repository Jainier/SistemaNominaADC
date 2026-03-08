/* ============================================================
   Ajuste de estructura para catalogos de planilla
   - Agrega campos parametrizables para motor de nomina
   - Script idempotente (se puede ejecutar varias veces)
   ============================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF COL_LENGTH('TipoConceptoNomina', 'CodigoConcepto') IS NULL
        ALTER TABLE TipoConceptoNomina ADD CodigoConcepto nvarchar(40) NULL;

    IF COL_LENGTH('TipoConceptoNomina', 'CodigoFormula') IS NULL
        ALTER TABLE TipoConceptoNomina ADD CodigoFormula nvarchar(60) NULL;

    IF COL_LENGTH('TipoConceptoNomina', 'ValorPorcentaje') IS NULL
        ALTER TABLE TipoConceptoNomina ADD ValorPorcentaje decimal(9,6) NULL;

    IF COL_LENGTH('TipoConceptoNomina', 'ValorFijo') IS NULL
        ALTER TABLE TipoConceptoNomina ADD ValorFijo decimal(10,2) NULL;

    IF COL_LENGTH('TipoConceptoNomina', 'OrdenCalculo') IS NULL
    BEGIN
        ALTER TABLE TipoConceptoNomina ADD OrdenCalculo int NOT NULL CONSTRAINT DF_TipoConceptoNomina_OrdenCalculo DEFAULT(0);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_TipoConceptoNomina_CodigoConcepto'
          AND object_id = OBJECT_ID('TipoConceptoNomina')
    )
    BEGIN
        CREATE UNIQUE INDEX IX_TipoConceptoNomina_CodigoConcepto
            ON TipoConceptoNomina (CodigoConcepto)
            WHERE CodigoConcepto IS NOT NULL;
    END

    COMMIT TRAN;
    SELECT 'OK - Ajuste de catalogos de planilla aplicado.' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err nvarchar(4000) = ERROR_MESSAGE();
    RAISERROR('Error aplicando ajuste de catalogos de planilla: %s', 16, 1, @Err);
END CATCH;

