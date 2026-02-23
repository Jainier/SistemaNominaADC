/*
    Script: Alineacion Fase 1 (Saneamiento + Endurecimiento)
    Objetivo:
    - Alinear restricciones de BD con las reglas ya implementadas en codigo para tablas actuales.
    - Sanear datos nulos heredados donde es seguro hacerlo.
    - Agregar/ajustar constraints de estructura faltantes (PK/UNIQUE/NOT NULL).

    Tablas cubiertas:
    - Estado
    - GrupoEstado
    - GrupoEstadoDetalle
    - ObjetoSistema
    - Departamento
    - Puesto
    - Empleado
    - TipoPermiso
    - TipoIncapacidad
    - TipoHoraExtra

    Nota:
    - Bitacora NO se endurece a NOT NULL en esta fase porque su uso suele ser historico/auditoria
      y conviene evitar bloqueos por datos legacy.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @IdEstadoActivo INT;
    SELECT TOP (1) @IdEstadoActivo = IdEstado
    FROM dbo.Estado
    WHERE UPPER(LTRIM(RTRIM(ISNULL(Nombre, '')))) = 'ACTIVO'
    ORDER BY IdEstado;

    IF @IdEstadoActivo IS NULL
    BEGIN
        THROW 51000, 'No existe un registro en dbo.Estado con Nombre = ''Activo''. Cree ese estado antes de ejecutar este script.', 1;
    END

    /* =========================================================
       1) SANEAMIENTO DE DATOS
       ========================================================= */

    /* Estado */
    UPDATE dbo.Estado
    SET Estado = 1
    WHERE Estado IS NULL;

    UPDATE dbo.Estado
    SET Nombre = CONCAT('Estado ', IdEstado)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    /* GrupoEstado */
    UPDATE dbo.GrupoEstado
    SET Nombre = CONCAT('GrupoEstado ', IdGrupoEstado)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    IF COL_LENGTH('dbo.GrupoEstado', 'Activo') IS NULL
    BEGIN
        ALTER TABLE dbo.GrupoEstado ADD Activo bit NULL;
    END

    UPDATE dbo.GrupoEstado
    SET Activo = 1
    WHERE Activo IS NULL;

    /* ObjetoSistema */
    UPDATE dbo.ObjetoSistema
    SET NombreEntidad = CONCAT('ObjetoSistema_', IdObjeto)
    WHERE NombreEntidad IS NULL OR LTRIM(RTRIM(NombreEntidad)) = '';

    UPDATE os
    SET IdGrupoEstado = (
        SELECT TOP (1) ge.IdGrupoEstado
        FROM dbo.GrupoEstado ge
        ORDER BY ge.IdGrupoEstado
    )
    FROM dbo.ObjetoSistema os
    WHERE os.IdGrupoEstado IS NULL;

    /* Departamento */
    UPDATE dbo.Departamento
    SET Nombre = CONCAT('Departamento ', IdDepartamento)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    UPDATE dbo.Departamento
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    /* Puesto */
    UPDATE dbo.Puesto
    SET Nombre = CONCAT('Puesto ', IdPuesto)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    UPDATE dbo.Puesto
    SET SalarioBase = 0
    WHERE SalarioBase IS NULL;

    UPDATE dbo.Puesto
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    /* Empleado */
    UPDATE dbo.Empleado
    SET Cedula = CONCAT('PEND-', IdEmpleado)
    WHERE Cedula IS NULL OR LTRIM(RTRIM(Cedula)) = '';

    UPDATE dbo.Empleado
    SET NombreCompleto = CONCAT('Empleado ', IdEmpleado)
    WHERE NombreCompleto IS NULL OR LTRIM(RTRIM(NombreCompleto)) = '';

    UPDATE dbo.Empleado
    SET FechaIngreso = CAST(GETDATE() AS date)
    WHERE FechaIngreso IS NULL;

    UPDATE dbo.Empleado
    SET SalarioBase = 0
    WHERE SalarioBase IS NULL;

    UPDATE dbo.Empleado
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    /* Tipos */
    UPDATE dbo.TipoPermiso
    SET Nombre = CONCAT('TipoPermiso ', IdTipoPermiso)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    UPDATE dbo.TipoPermiso
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    UPDATE dbo.TipoIncapacidad
    SET Nombre = CONCAT('TipoIncapacidad ', IdTipoIncapacidad)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    UPDATE dbo.TipoIncapacidad
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    UPDATE dbo.TipoHoraExtra
    SET Nombre = CONCAT('TipoHoraExtra ', IdTipoHoraExtra)
    WHERE Nombre IS NULL OR LTRIM(RTRIM(Nombre)) = '';

    UPDATE dbo.TipoHoraExtra
    SET PorcentajePago = 1.0000
    WHERE PorcentajePago IS NULL;

    UPDATE dbo.TipoHoraExtra
    SET IdEstado = @IdEstadoActivo
    WHERE IdEstado IS NULL;

    /* GrupoEstadoDetalle */
    ;WITH Duplicados AS
    (
        SELECT
            IdgrupoEstado,
            IdEstado,
            Orden,
            ROW_NUMBER() OVER (
                PARTITION BY IdgrupoEstado, IdEstado
                ORDER BY CASE WHEN Orden IS NULL THEN 1 ELSE 0 END, Orden, (SELECT 1)
            ) AS rn
        FROM dbo.GrupoEstadoDetalle
        WHERE IdgrupoEstado IS NOT NULL
          AND IdEstado IS NOT NULL
    )
    DELETE FROM Duplicados
    WHERE rn > 1;

    DELETE FROM dbo.GrupoEstadoDetalle
    WHERE IdgrupoEstado IS NULL
       OR IdEstado IS NULL;

    /* =========================================================
       2) VALIDACIONES PREVIAS (FALLAR SI AUN HAY DATOS INVALIDOS)
       ========================================================= */

    IF EXISTS (SELECT 1 FROM dbo.ObjetoSistema GROUP BY NombreEntidad HAVING COUNT(*) > 1)
        THROW 51001, 'Existen valores duplicados en dbo.ObjetoSistema.NombreEntidad. Corrija duplicados antes de aplicar indice unico.', 1;

    IF EXISTS (SELECT 1 FROM dbo.GrupoEstadoDetalle GROUP BY IdgrupoEstado, IdEstado HAVING COUNT(*) > 1)
        THROW 51002, 'Existen duplicados en dbo.GrupoEstadoDetalle (IdGrupoEstado, IdEstado).', 1;

    IF EXISTS (SELECT 1 FROM dbo.Departamento WHERE IdEstado IS NULL OR Nombre IS NULL)
        THROW 51003, 'Persisten nulos en dbo.Departamento.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Puesto WHERE Nombre IS NULL OR SalarioBase IS NULL OR IdDepartamento IS NULL OR IdEstado IS NULL)
        THROW 51004, 'Persisten nulos en dbo.Puesto.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Empleado WHERE Cedula IS NULL OR NombreCompleto IS NULL OR FechaIngreso IS NULL OR IdPuesto IS NULL OR SalarioBase IS NULL OR IdEstado IS NULL)
        THROW 51005, 'Persisten nulos en dbo.Empleado.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TipoPermiso WHERE Nombre IS NULL OR IdEstado IS NULL)
        THROW 51006, 'Persisten nulos en dbo.TipoPermiso.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TipoIncapacidad WHERE Nombre IS NULL OR IdEstado IS NULL)
        THROW 51007, 'Persisten nulos en dbo.TipoIncapacidad.', 1;

    IF EXISTS (SELECT 1 FROM dbo.TipoHoraExtra WHERE Nombre IS NULL OR PorcentajePago IS NULL OR IdEstado IS NULL)
        THROW 51008, 'Persisten nulos en dbo.TipoHoraExtra.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Estado WHERE Nombre IS NULL OR Estado IS NULL)
        THROW 51009, 'Persisten nulos en dbo.Estado (Nombre/Estado).', 1;

    IF EXISTS (SELECT 1 FROM dbo.ObjetoSistema WHERE NombreEntidad IS NULL OR IdGrupoEstado IS NULL)
        THROW 51010, 'Persisten nulos en dbo.ObjetoSistema.', 1;

    IF EXISTS (SELECT 1 FROM dbo.GrupoEstado WHERE Nombre IS NULL OR Activo IS NULL)
        THROW 51011, 'Persisten nulos en dbo.GrupoEstado (Nombre/Activo).', 1;

    IF EXISTS (SELECT 1 FROM dbo.GrupoEstadoDetalle WHERE IdgrupoEstado IS NULL OR IdEstado IS NULL)
        THROW 51012, 'Persisten nulos en dbo.GrupoEstadoDetalle.', 1;

    /* =========================================================
       3) ENDURECIMIENTO DE ESTRUCTURA
       ========================================================= */

    /* Longitudes / precision */
    ALTER TABLE dbo.Departamento ALTER COLUMN Nombre varchar(150) NOT NULL;
    ALTER TABLE dbo.Puesto ALTER COLUMN Nombre varchar(150) NOT NULL;
    ALTER TABLE dbo.Puesto ALTER COLUMN SalarioBase decimal(10,2) NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN Cedula varchar(20) NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN NombreCompleto varchar(200) NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN FechaIngreso date NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN SalarioBase decimal(10,2) NOT NULL;
    ALTER TABLE dbo.TipoPermiso ALTER COLUMN Nombre varchar(100) NOT NULL;
    ALTER TABLE dbo.TipoIncapacidad ALTER COLUMN Nombre varchar(100) NOT NULL;
    ALTER TABLE dbo.TipoHoraExtra ALTER COLUMN Nombre varchar(100) NOT NULL;
    ALTER TABLE dbo.TipoHoraExtra ALTER COLUMN PorcentajePago decimal(5,4) NOT NULL;
    ALTER TABLE dbo.Estado ALTER COLUMN Nombre varchar(100) NOT NULL;
    ALTER TABLE dbo.Estado ALTER COLUMN Estado bit NOT NULL;
    ALTER TABLE dbo.GrupoEstado ALTER COLUMN Nombre varchar(100) NOT NULL;
    ALTER TABLE dbo.GrupoEstado ALTER COLUMN Activo bit NOT NULL;
    ALTER TABLE dbo.ObjetoSistema ALTER COLUMN NombreEntidad varchar(100) NOT NULL;
    ALTER TABLE dbo.ObjetoSistema ALTER COLUMN IdGrupoEstado int NOT NULL;

    /* FKs que la app trata como obligatorias */
    ALTER TABLE dbo.Departamento ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.Puesto ALTER COLUMN IdDepartamento int NOT NULL;
    ALTER TABLE dbo.Puesto ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN IdPuesto int NOT NULL;
    ALTER TABLE dbo.Empleado ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.TipoPermiso ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.TipoIncapacidad ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.TipoHoraExtra ALTER COLUMN IdEstado int NOT NULL;
    ALTER TABLE dbo.GrupoEstadoDetalle ALTER COLUMN IdgrupoEstado int NOT NULL;
    ALTER TABLE dbo.GrupoEstadoDetalle ALTER COLUMN IdEstado int NOT NULL;

    /* PK faltante en GrupoEstadoDetalle */
    IF NOT EXISTS (
        SELECT 1
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID('dbo.GrupoEstadoDetalle')
          AND kc.[type] = 'PK'
    )
    BEGIN
        ALTER TABLE dbo.GrupoEstadoDetalle
        ADD CONSTRAINT PK_GrupoEstadoDetalle PRIMARY KEY CLUSTERED (IdgrupoEstado, IdEstado);
    END

    /* Indice unico faltante/esperado en ObjetoSistema.NombreEntidad */
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes i
        WHERE i.object_id = OBJECT_ID('dbo.ObjetoSistema')
          AND i.is_unique = 1
          AND i.name = 'UX_ObjetoSistema_NombreEntidad'
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_ObjetoSistema_NombreEntidad
            ON dbo.ObjetoSistema (NombreEntidad);
    END

    /* Default para Estado.Estado (si no existe) */
    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        WHERE dc.parent_object_id = OBJECT_ID('dbo.Estado')
          AND dc.name = 'DF_Estado_Estado'
    )
    BEGIN
        ALTER TABLE dbo.Estado
        ADD CONSTRAINT DF_Estado_Estado DEFAULT ((1)) FOR [Estado];
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        WHERE dc.parent_object_id = OBJECT_ID('dbo.GrupoEstado')
          AND dc.name = 'DF_GrupoEstado_Activo'
    )
    BEGIN
        ALTER TABLE dbo.GrupoEstado
        ADD CONSTRAINT DF_GrupoEstado_Activo DEFAULT ((1)) FOR [Activo];
    END

    COMMIT TRAN;
    PRINT 'Alineacion Fase 1 aplicada correctamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    DECLARE @Msg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @Num INT = ERROR_NUMBER();
    DECLARE @State INT = ERROR_STATE();

    RAISERROR('Fallo en Alineacion Fase 1: %s', 16, 1, @Msg);
END CATCH;
