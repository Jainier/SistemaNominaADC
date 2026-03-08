SET NOCOUNT ON;

IF COL_LENGTH('Permiso', 'IdentityUserIdDecision') IS NULL
BEGIN
    ALTER TABLE Permiso
    ADD IdentityUserIdDecision NVARCHAR(450) NULL;
END

IF COL_LENGTH('SolicitudHorasExtra', 'IdentityUserIdDecision') IS NULL
BEGIN
    ALTER TABLE SolicitudHorasExtra
    ADD IdentityUserIdDecision NVARCHAR(450) NULL;
END

IF COL_LENGTH('SolicitudVacaciones', 'IdentityUserIdDecision') IS NULL
BEGIN
    ALTER TABLE SolicitudVacaciones
    ADD IdentityUserIdDecision NVARCHAR(450) NULL;
END

IF COL_LENGTH('Incapacidad', 'IdentityUserIdDecision') IS NULL
BEGIN
    ALTER TABLE Incapacidad
    ADD IdentityUserIdDecision NVARCHAR(450) NULL;
END

IF COL_LENGTH('PlanillaEncabezado', 'IdentityUserIdDecision') IS NULL
BEGIN
    ALTER TABLE PlanillaEncabezado
    ADD IdentityUserIdDecision NVARCHAR(450) NULL;
END
