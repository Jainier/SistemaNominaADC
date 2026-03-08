/* ============================================================
   Seed base de flujos para transacciones operativas
   Reglas:
   - 0  -> 10 : Crear
   - 10 -> 50 : Rechazar
   - 10 -> 40 : Aprobar
   - 10 -> 10 : Editar
   Entidades:
   - Permiso
   - SolicitudVacaciones
   - SolicitudHorasExtra
   - Incapacidad
   ============================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @IdEstadoNulo INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Codigo = 0
        ORDER BY IdEstado
    );

    DECLARE @IdEstadoPendiente INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Codigo = 10
        ORDER BY IdEstado
    );

    DECLARE @IdEstadoAprobado INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Codigo = 40
        ORDER BY IdEstado
    );

    DECLARE @IdEstadoRechazado INT = (
        SELECT TOP (1) IdEstado
        FROM Estado
        WHERE Codigo = 50
        ORDER BY IdEstado
    );

    IF @IdEstadoNulo IS NULL
        THROW 50001, 'No se encontro el estado con codigo 0 (Nulo).', 1;
    IF @IdEstadoPendiente IS NULL
        THROW 50002, 'No se encontro el estado con codigo 10 (Pendiente).', 1;
    IF @IdEstadoAprobado IS NULL
        THROW 50003, 'No se encontro el estado con codigo 40 (Aprobado).', 1;
    IF @IdEstadoRechazado IS NULL
        THROW 50004, 'No se encontro el estado con codigo 50 (Rechazado).', 1;

    DECLARE @Entidades TABLE (Entidad VARCHAR(100) NOT NULL PRIMARY KEY);
    INSERT INTO @Entidades (Entidad)
    VALUES
    ('Permiso'),
    ('SolicitudVacaciones'),
    ('SolicitudHorasExtra'),
    ('Incapacidad');

    DECLARE @DefinicionFlujo TABLE
    (
        IdEstadoOrigen INT NOT NULL,
        IdEstadoDestino INT NOT NULL,
        Accion VARCHAR(50) NOT NULL
    );

    INSERT INTO @DefinicionFlujo (IdEstadoOrigen, IdEstadoDestino, Accion)
    VALUES
    (@IdEstadoNulo, @IdEstadoPendiente, 'Crear'),
    (@IdEstadoPendiente, @IdEstadoRechazado, 'Rechazar'),
    (@IdEstadoPendiente, @IdEstadoAprobado, 'Aprobar'),
    (@IdEstadoPendiente, @IdEstadoPendiente, 'Editar');

    DECLARE @FlujoObjetivo TABLE
    (
        Entidad VARCHAR(100) NOT NULL,
        IdEstadoOrigen INT NOT NULL,
        IdEstadoDestino INT NOT NULL,
        Accion VARCHAR(50) NOT NULL,
        RequiereRol VARCHAR(256) NULL,
        Activo BIT NOT NULL
    );

    INSERT INTO @FlujoObjetivo (Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
    SELECT
        E.Entidad,
        D.IdEstadoOrigen,
        D.IdEstadoDestino,
        D.Accion,
        NULL,
        1
    FROM @Entidades E
    CROSS JOIN @DefinicionFlujo D;

    MERGE FlujoEstado AS T
    USING @FlujoObjetivo AS S
      ON UPPER(LTRIM(RTRIM(T.Entidad))) = UPPER(LTRIM(RTRIM(S.Entidad)))
     AND T.IdEstadoOrigen = S.IdEstadoOrigen
     AND T.IdEstadoDestino = S.IdEstadoDestino
     AND UPPER(LTRIM(RTRIM(T.Accion))) = UPPER(LTRIM(RTRIM(S.Accion)))
    WHEN MATCHED THEN
      UPDATE SET
        T.RequiereRol = S.RequiereRol,
        T.Activo = S.Activo
    WHEN NOT MATCHED THEN
      INSERT (Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
      VALUES (S.Entidad, S.IdEstadoOrigen, S.IdEstadoDestino, S.Accion, S.RequiereRol, S.Activo);

    COMMIT TRAN;

    SELECT
        'OK - Flujos base transaccionales sembrados.' AS Resultado,
        (SELECT COUNT(1) FROM @Entidades) AS EntidadesIncluidas,
        (SELECT COUNT(1) FROM @FlujoObjetivo) AS TransicionesObjetivo;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR('Error al sembrar flujos transaccionales base: %s', 16, 1, @Err);
END CATCH;

