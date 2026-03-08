/* ============================================================
   Seed flujo de estados para PlanillaEncabezado
   ============================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @IdPendiente INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Nombre IN ('Pendiente', 'Pendiente de Calculo', 'Pendiente de Cálculo')
        ORDER BY IdEstado
    );

    DECLARE @IdCalculada INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Nombre = 'Calculada'
        ORDER BY IdEstado
    );

    DECLARE @IdAprobada INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Nombre IN ('Aprobada', 'Aprobado')
        ORDER BY IdEstado
    );

    DECLARE @IdInactivo INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Nombre = 'Inactivo'
        ORDER BY IdEstado
    );

    IF @IdPendiente IS NULL
        THROW 50001, 'No se encontro estado Pendiente/Pendiente de Calculo.', 1;

    IF @IdCalculada IS NULL
        THROW 50002, 'No se encontro estado Calculada.', 1;

    IF @IdAprobada IS NULL
        THROW 50003, 'No se encontro estado Aprobada.', 1;

    IF @IdInactivo IS NULL
        THROW 50004, 'No se encontro estado Inactivo.', 1;

    DECLARE @Flujo TABLE
    (
        Entidad       VARCHAR(100) NOT NULL,
        IdEstadoOrigen INT NULL,
        IdEstadoDestino INT NOT NULL,
        Accion        VARCHAR(50) NOT NULL,
        RequiereRol   VARCHAR(256) NULL,
        Activo        BIT NOT NULL
    );

    INSERT INTO @Flujo (Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
    VALUES
    ('PlanillaEncabezado', @IdPendiente, @IdPendiente, 'Crear',      NULL, 1),
    ('PlanillaEncabezado', @IdPendiente, @IdPendiente, 'Editar',     NULL, 1),
    ('PlanillaEncabezado', @IdPendiente, @IdCalculada, 'Calcular',   NULL, 1),
    ('PlanillaEncabezado', @IdCalculada, @IdCalculada, 'Recalcular', NULL, 1),
    ('PlanillaEncabezado', @IdCalculada, @IdAprobada,  'Aprobar',    NULL, 1),
    ('PlanillaEncabezado', @IdPendiente, @IdInactivo,  'Desactivar', NULL, 1),
    ('PlanillaEncabezado', @IdCalculada, @IdInactivo,  'Desactivar', NULL, 1);

    MERGE FlujoEstado AS T
    USING @Flujo AS S
      ON  T.Entidad = S.Entidad
      AND ISNULL(T.IdEstadoOrigen, -1) = ISNULL(S.IdEstadoOrigen, -1)
      AND T.IdEstadoDestino = S.IdEstadoDestino
      AND T.Accion = S.Accion
    WHEN MATCHED THEN
      UPDATE SET
        T.RequiereRol = S.RequiereRol,
        T.Activo = S.Activo
    WHEN NOT MATCHED THEN
      INSERT (Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
      VALUES (S.Entidad, S.IdEstadoOrigen, S.IdEstadoDestino, S.Accion, S.RequiereRol, S.Activo);

    DELETE F
    FROM FlujoEstado F
    WHERE F.Entidad = 'PlanillaEncabezado'
      AND F.IdEstadoOrigen IS NULL
      AND UPPER(LTRIM(RTRIM(F.Accion))) = 'CREAR';

    COMMIT TRAN;

    SELECT 'OK - Flujo de PlanillaEncabezado configurado.' AS Resultado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR('Error seed FlujoEstadoPlanilla: %s', 16, 1, @Err);
END CATCH;
