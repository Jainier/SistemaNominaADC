IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Activo] bit NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Bitacora] (
    [IdBitacora] int NOT NULL IDENTITY,
    [IdEmpleado] nvarchar(100) NOT NULL,
    [Fecha] datetime2 NOT NULL,
    [Accion] nvarchar(500) NOT NULL,
    [Detalle] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Bitacora] PRIMARY KEY ([IdBitacora])
);
GO

CREATE TABLE [Estado] (
    [IdEstado] int NOT NULL IDENTITY,
    [Codigo] int NULL,
    [Nombre] varchar(100) NULL,
    [Descripcion] nvarchar(max) NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Estado] PRIMARY KEY ([IdEstado])
);
GO

CREATE TABLE [GrupoEstado] (
    [IdGrupoEstado] int NOT NULL IDENTITY,
    [Nombre] nvarchar(max) NULL,
    [Descripcion] nvarchar(max) NULL,
    CONSTRAINT [PK_GrupoEstado] PRIMARY KEY ([IdGrupoEstado])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Departamento] (
    [IdDepartamento] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_Departamento] PRIMARY KEY ([IdDepartamento]),
    CONSTRAINT [FK_Departamento_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE
);
GO

CREATE TABLE [GrupoEstadoDetalle] (
    [IdGrupoEstado] int NOT NULL,
    [IdEstado] int NOT NULL,
    [Orden] int NULL,
    CONSTRAINT [PK_GrupoEstadoDetalle] PRIMARY KEY ([IdGrupoEstado], [IdEstado]),
    CONSTRAINT [FK_GrupoEstadoDetalle_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE,
    CONSTRAINT [FK_GrupoEstadoDetalle_GrupoEstado_IdGrupoEstado] FOREIGN KEY ([IdGrupoEstado]) REFERENCES [GrupoEstado] ([IdGrupoEstado]) ON DELETE CASCADE
);
GO

CREATE TABLE [ObjetoSistema] (
    [IdObjeto] int NOT NULL IDENTITY,
    [NombreEntidad] nvarchar(max) NOT NULL,
    [IdGrupoEstado] int NOT NULL,
    CONSTRAINT [PK_ObjetoSistema] PRIMARY KEY ([IdObjeto]),
    CONSTRAINT [FK_ObjetoSistema_GrupoEstado_IdGrupoEstado] FOREIGN KEY ([IdGrupoEstado]) REFERENCES [GrupoEstado] ([IdGrupoEstado]) ON DELETE CASCADE
);
GO

CREATE TABLE [Puesto] (
    [IdPuesto] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [SalarioBase] decimal(18,2) NOT NULL,
    [IdDepartamento] int NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Puesto] PRIMARY KEY ([IdPuesto]),
    CONSTRAINT [FK_Puesto_Departamento_IdDepartamento] FOREIGN KEY ([IdDepartamento]) REFERENCES [Departamento] ([IdDepartamento]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Empleado] (
    [IdEmpleado] int NOT NULL IDENTITY,
    [IdentityUserId] nvarchar(max) NULL,
    [Cedula] nvarchar(20) NOT NULL,
    [NombreCompleto] nvarchar(200) NOT NULL,
    [FechaIngreso] datetime2 NOT NULL,
    [FechaSalida] datetime2 NULL,
    [IdPuesto] int NOT NULL,
    [SalarioBase] decimal(18,2) NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_Empleado] PRIMARY KEY ([IdEmpleado]),
    CONSTRAINT [FK_Empleado_Puesto_IdPuesto] FOREIGN KEY ([IdPuesto]) REFERENCES [Puesto] ([IdPuesto]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_Departamento_IdEstado] ON [Departamento] ([IdEstado]);
GO

CREATE INDEX [IX_Empleado_IdPuesto] ON [Empleado] ([IdPuesto]);
GO

CREATE INDEX [IX_GrupoEstadoDetalle_IdEstado] ON [GrupoEstadoDetalle] ([IdEstado]);
GO

CREATE INDEX [IX_ObjetoSistema_IdGrupoEstado] ON [ObjetoSistema] ([IdGrupoEstado]);
GO

CREATE INDEX [IX_Puesto_IdDepartamento] ON [Puesto] ([IdDepartamento]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251227233817_AgregarEstadoARoles', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Puesto]') AND [c].[name] = N'Estado');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Puesto] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Puesto] DROP COLUMN [Estado];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Empleado]') AND [c].[name] = N'Estado');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Empleado] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Empleado] DROP COLUMN [Estado];
GO

ALTER TABLE [Puesto] ADD [IdEstado] int NOT NULL DEFAULT 0;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ObjetoSistema]') AND [c].[name] = N'NombreEntidad');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [ObjetoSistema] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [ObjetoSistema] ALTER COLUMN [NombreEntidad] nvarchar(100) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GrupoEstado]') AND [c].[name] = N'Nombre');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [GrupoEstado] DROP CONSTRAINT [' + @var3 + '];');
UPDATE [GrupoEstado] SET [Nombre] = N'' WHERE [Nombre] IS NULL;
ALTER TABLE [GrupoEstado] ALTER COLUMN [Nombre] nvarchar(100) NOT NULL;
ALTER TABLE [GrupoEstado] ADD DEFAULT N'' FOR [Nombre];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[GrupoEstado]') AND [c].[name] = N'Descripcion');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [GrupoEstado] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [GrupoEstado] ALTER COLUMN [Descripcion] nvarchar(250) NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Estado]') AND [c].[name] = N'Nombre');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Estado] DROP CONSTRAINT [' + @var5 + '];');
UPDATE [Estado] SET [Nombre] = '' WHERE [Nombre] IS NULL;
ALTER TABLE [Estado] ALTER COLUMN [Nombre] varchar(100) NOT NULL;
ALTER TABLE [Estado] ADD DEFAULT '' FOR [Nombre];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Estado]') AND [c].[name] = N'Descripcion');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Estado] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Estado] ALTER COLUMN [Descripcion] nvarchar(250) NULL;
GO

ALTER TABLE [Empleado] ADD [IdEstado] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [AspNetRoles] ADD [EsSistema] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

CREATE TABLE [ObjetoSistemaRol] (
    [IdObjeto] int NOT NULL,
    [RoleName] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_ObjetoSistemaRol] PRIMARY KEY ([IdObjeto], [RoleName]),
    CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto] FOREIGN KEY ([IdObjeto]) REFERENCES [ObjetoSistema] ([IdObjeto]) ON DELETE CASCADE
);
GO

CREATE TABLE [TipoHoraExtra] (
    [IdTipoHoraExtra] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_TipoHoraExtra] PRIMARY KEY ([IdTipoHoraExtra])
);
GO

CREATE TABLE [TipoIncapacidad] (
    [IdTipoIncapacidad] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_TipoIncapacidad] PRIMARY KEY ([IdTipoIncapacidad])
);
GO

CREATE TABLE [TipoPermiso] (
    [IdTipoPermiso] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Estado] bit NOT NULL,
    CONSTRAINT [PK_TipoPermiso] PRIMARY KEY ([IdTipoPermiso])
);
GO

CREATE INDEX [IX_Puesto_IdEstado] ON [Puesto] ([IdEstado]);
GO

CREATE INDEX [IX_Empleado_IdEstado] ON [Empleado] ([IdEstado]);
GO

ALTER TABLE [Empleado] ADD CONSTRAINT [FK_Empleado_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE;
GO

ALTER TABLE [Puesto] ADD CONSTRAINT [FK_Puesto_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260222204521_AgregarObjetoSistemaRoles', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [ObjetoSistemaRol] (
    [IdObjeto] int NOT NULL,
    [RoleName] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_ObjetoSistemaRol] PRIMARY KEY ([IdObjeto], [RoleName]),
    CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto] FOREIGN KEY ([IdObjeto]) REFERENCES [ObjetoSistema] ([IdObjeto]) ON DELETE CASCADE
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260222204723_AgregarObjetoSistemaRol', N'8.0.22');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [ObjetoSistemaRol] DROP CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TipoPermiso]') AND [c].[name] = N'Estado');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [TipoPermiso] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [TipoPermiso] DROP COLUMN [Estado];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TipoIncapacidad]') AND [c].[name] = N'Estado');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [TipoIncapacidad] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [TipoIncapacidad] DROP COLUMN [Estado];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TipoHoraExtra]') AND [c].[name] = N'Estado');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [TipoHoraExtra] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [TipoHoraExtra] DROP COLUMN [Estado];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bitacora]') AND [c].[name] = N'Detalle');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Bitacora] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [Bitacora] DROP COLUMN [Detalle];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bitacora]') AND [c].[name] = N'IdEmpleado');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Bitacora] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Bitacora] DROP COLUMN [IdEmpleado];
GO

ALTER TABLE [TipoPermiso] ADD [IdEstado] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TipoIncapacidad] ADD [IdEstado] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TipoHoraExtra] ADD [IdEstado] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [TipoHoraExtra] ADD [PorcentajePago] decimal(5,4) NOT NULL DEFAULT 0.0;
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Puesto]') AND [c].[name] = N'SalarioBase');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Puesto] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Puesto] ALTER COLUMN [SalarioBase] decimal(10,2) NOT NULL;
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Puesto]') AND [c].[name] = N'Nombre');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Puesto] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Puesto] ALTER COLUMN [Nombre] nvarchar(150) NOT NULL;
GO

ALTER TABLE [GrupoEstado] ADD [Activo] bit NOT NULL DEFAULT CAST(1 AS bit);
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Estado]') AND [c].[name] = N'Estado');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Estado] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Estado] ALTER COLUMN [Estado] bit NULL;
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Empleado]') AND [c].[name] = N'SalarioBase');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [Empleado] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [Empleado] ALTER COLUMN [SalarioBase] decimal(10,2) NOT NULL;
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Departamento]') AND [c].[name] = N'Nombre');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Departamento] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [Departamento] ALTER COLUMN [Nombre] nvarchar(150) NOT NULL;
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bitacora]') AND [c].[name] = N'Fecha');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [Bitacora] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [Bitacora] ALTER COLUMN [Fecha] datetime2 NULL;
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Bitacora]') AND [c].[name] = N'Accion');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Bitacora] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [Bitacora] ALTER COLUMN [Accion] varchar(150) NULL;
GO

ALTER TABLE [Bitacora] ADD [Descripcion] text NULL;
GO

ALTER TABLE [Bitacora] ADD [IdEstado] int NULL;
GO

ALTER TABLE [Bitacora] ADD [IdentityUserId] nvarchar(450) NULL;
GO

CREATE TABLE [Asistencia] (
    [IdAsistencia] int NOT NULL IDENTITY,
    [IdEmpleado] int NOT NULL,
    [Fecha] date NOT NULL,
    [HoraEntrada] datetime NULL,
    [HoraSalida] datetime NULL,
    [Ausencia] bit NULL,
    [Justificacion] text NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_Asistencia] PRIMARY KEY ([IdAsistencia]),
    CONSTRAINT [FK_Asistencia_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Asistencia_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [DepartamentoJefatura] (
    [IdDepartamentoJefatura] int NOT NULL IDENTITY,
    [IdDepartamento] int NOT NULL,
    [IdEmpleado] int NOT NULL,
    [TipoJefatura] nvarchar(20) NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [VigenciaDesde] date NULL,
    [VigenciaHasta] date NULL,
    CONSTRAINT [PK_DepartamentoJefatura] PRIMARY KEY ([IdDepartamentoJefatura]),
    CONSTRAINT [FK_DepartamentoJefatura_Departamento_IdDepartamento] FOREIGN KEY ([IdDepartamento]) REFERENCES [Departamento] ([IdDepartamento]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DepartamentoJefatura_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [EmpleadoJerarquia] (
    [IdEmpleadoJerarquia] int NOT NULL IDENTITY,
    [IdEmpleado] int NOT NULL,
    [IdSupervisor] int NOT NULL,
    [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
    [VigenciaDesde] date NULL,
    [VigenciaHasta] date NULL,
    [Observacion] nvarchar(250) NULL,
    CONSTRAINT [PK_EmpleadoJerarquia] PRIMARY KEY ([IdEmpleadoJerarquia]),
    CONSTRAINT [FK_EmpleadoJerarquia_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EmpleadoJerarquia_Empleado_IdSupervisor] FOREIGN KEY ([IdSupervisor]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Incapacidad] (
    [IdIncapacidad] int NOT NULL IDENTITY,
    [IdEmpleado] int NULL,
    [FechaInicio] date NULL,
    [FechaFin] date NULL,
    [IdTipoIncapacidad] int NULL,
    [MontoCubierto] decimal(10,2) NULL,
    [IdEstado] int NULL,
    [NombreDocumento] nvarchar(255) NULL,
    [RutaDocumento] nvarchar(500) NULL,
    [ComentarioRevision] nvarchar(300) NULL,
    [ComentarioSolicitud] nvarchar(300) NULL,
    [ComentarioAprobacion] nvarchar(300) NULL,
    [FechaRegistro] datetime2 NULL,
    CONSTRAINT [PK_Incapacidad] PRIMARY KEY ([IdIncapacidad]),
    CONSTRAINT [FK_Incapacidad_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Incapacidad_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Incapacidad_TipoIncapacidad_IdTipoIncapacidad] FOREIGN KEY ([IdTipoIncapacidad]) REFERENCES [TipoIncapacidad] ([IdTipoIncapacidad]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ModoCalculoConceptoNomina] (
    [IdModoCalculoConceptoNomina] int NOT NULL IDENTITY,
    [Nombre] varchar(100) NOT NULL,
    [Descripcion] varchar(250) NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_ModoCalculoConceptoNomina] PRIMARY KEY ([IdModoCalculoConceptoNomina]),
    CONSTRAINT [FK_ModoCalculoConceptoNomina_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notificacion] (
    [IdNotificacion] int NOT NULL IDENTITY,
    [IdentityUserId] nvarchar(450) NOT NULL,
    [Titulo] nvarchar(150) NOT NULL,
    [Mensaje] nvarchar(500) NOT NULL,
    [UrlDestino] nvarchar(300) NULL,
    [Leida] bit NOT NULL DEFAULT CAST(0 AS bit),
    [FechaCreacion] datetime2 NOT NULL,
    [FechaLectura] datetime2 NULL,
    CONSTRAINT [PK_Notificacion] PRIMARY KEY ([IdNotificacion])
);
GO

CREATE TABLE [Permiso] (
    [IdPermiso] int NOT NULL IDENTITY,
    [IdEmpleado] int NULL,
    [IdTipoPermiso] int NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NOT NULL,
    [Motivo] nvarchar(200) NULL,
    [ComentarioAprobacion] nvarchar(300) NULL,
    [IdEstado] int NULL,
    CONSTRAINT [PK_Permiso] PRIMARY KEY ([IdPermiso]),
    CONSTRAINT [FK_Permiso_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Permiso_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Permiso_TipoPermiso_IdTipoPermiso] FOREIGN KEY ([IdTipoPermiso]) REFERENCES [TipoPermiso] ([IdTipoPermiso]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SolicitudHorasExtra] (
    [IdSolicitudHorasExtra] int NOT NULL IDENTITY,
    [IdEmpleado] int NULL,
    [Fecha] date NOT NULL,
    [CantidadHoras] decimal(10,2) NULL,
    [IdTipoHoraExtra] int NULL,
    [IdEstado] int NULL,
    [Motivo] nvarchar(200) NULL,
    [ComentarioAprobacion] nvarchar(300) NULL,
    CONSTRAINT [PK_SolicitudHorasExtra] PRIMARY KEY ([IdSolicitudHorasExtra]),
    CONSTRAINT [FK_SolicitudHorasExtra_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SolicitudHorasExtra_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SolicitudHorasExtra_TipoHoraExtra_IdTipoHoraExtra] FOREIGN KEY ([IdTipoHoraExtra]) REFERENCES [TipoHoraExtra] ([IdTipoHoraExtra]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SolicitudVacaciones] (
    [IdSolicitudVacaciones] int NOT NULL IDENTITY,
    [IdEmpleado] int NULL,
    [CantidadDias] int NULL,
    [FechaInicio] date NOT NULL,
    [FechaFin] date NOT NULL,
    [IdEstado] int NULL,
    [ComentarioSolicitud] nvarchar(300) NULL,
    [ComentarioAprobacion] nvarchar(300) NULL,
    CONSTRAINT [PK_SolicitudVacaciones] PRIMARY KEY ([IdSolicitudVacaciones]),
    CONSTRAINT [FK_SolicitudVacaciones_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SolicitudVacaciones_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TipoPlanilla] (
    [IdTipoPlanilla] int NOT NULL IDENTITY,
    [Nombre] varchar(100) NOT NULL,
    [Descripcion] varchar(100) NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_TipoPlanilla] PRIMARY KEY ([IdTipoPlanilla]),
    CONSTRAINT [FK_TipoPlanilla_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Vacaciones] (
    [IdVacaciones] int NOT NULL IDENTITY,
    [IdEmpleado] int NULL,
    [DiasRestantes] int NULL,
    [IdEstado] int NULL,
    CONSTRAINT [PK_Vacaciones] PRIMARY KEY ([IdVacaciones]),
    CONSTRAINT [FK_Vacaciones_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Vacaciones_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION
);
GO

CREATE TABLE [TipoConceptoNomina] (
    [IdConceptoNomina] int NOT NULL IDENTITY,
    [Nombre] varchar(150) NOT NULL,
    [IdModoCalculo] int NOT NULL,
    [FormulaCalculo] varchar(1000) NULL,
    [EsIngreso] bit NOT NULL,
    [EsDeduccion] bit NOT NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_TipoConceptoNomina] PRIMARY KEY ([IdConceptoNomina]),
    CONSTRAINT [FK_TipoConceptoNomina_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TipoConceptoNomina_ModoCalculoConceptoNomina_IdModoCalculo] FOREIGN KEY ([IdModoCalculo]) REFERENCES [ModoCalculoConceptoNomina] ([IdModoCalculoConceptoNomina]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PlanillaEncabezado] (
    [IdPlanilla] int NOT NULL IDENTITY,
    [PeriodoInicio] date NOT NULL,
    [PeriodoFin] date NOT NULL,
    [PeriodoAguinaldo] int NULL,
    [FechaPago] date NOT NULL,
    [IdTipoPlanilla] int NOT NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_PlanillaEncabezado] PRIMARY KEY ([IdPlanilla]),
    CONSTRAINT [FK_PlanillaEncabezado_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanillaEncabezado_TipoPlanilla_IdTipoPlanilla] FOREIGN KEY ([IdTipoPlanilla]) REFERENCES [TipoPlanilla] ([IdTipoPlanilla]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PlanillaDetalle] (
    [IdPlanillaDetalle] int NOT NULL IDENTITY,
    [IdPlanilla] int NOT NULL,
    [IdEmpleado] int NOT NULL,
    [SalarioBase] decimal(10,2) NOT NULL,
    [TotalIngresos] decimal(10,2) NOT NULL,
    [TotalDeducciones] decimal(10,2) NOT NULL,
    [SalarioBruto] decimal(10,2) NOT NULL,
    [SalarioNeto] decimal(10,2) NOT NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_PlanillaDetalle] PRIMARY KEY ([IdPlanillaDetalle]),
    CONSTRAINT [FK_PlanillaDetalle_Empleado_IdEmpleado] FOREIGN KEY ([IdEmpleado]) REFERENCES [Empleado] ([IdEmpleado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanillaDetalle_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanillaDetalle_PlanillaEncabezado_IdPlanilla] FOREIGN KEY ([IdPlanilla]) REFERENCES [PlanillaEncabezado] ([IdPlanilla]) ON DELETE NO ACTION
);
GO

CREATE TABLE [PlanillaDetalleConcepto] (
    [IdDetalleConcepto] int NOT NULL IDENTITY,
    [IdPlanillaDetalle] int NOT NULL,
    [IdConceptoNomina] int NOT NULL,
    [Monto] decimal(10,2) NOT NULL,
    [IdEstado] int NOT NULL,
    CONSTRAINT [PK_PlanillaDetalleConcepto] PRIMARY KEY ([IdDetalleConcepto]),
    CONSTRAINT [FK_PlanillaDetalleConcepto_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanillaDetalleConcepto_PlanillaDetalle_IdPlanillaDetalle] FOREIGN KEY ([IdPlanillaDetalle]) REFERENCES [PlanillaDetalle] ([IdPlanillaDetalle]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PlanillaDetalleConcepto_TipoConceptoNomina_IdConceptoNomina] FOREIGN KEY ([IdConceptoNomina]) REFERENCES [TipoConceptoNomina] ([IdConceptoNomina]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_TipoPermiso_IdEstado] ON [TipoPermiso] ([IdEstado]);
GO

CREATE INDEX [IX_TipoIncapacidad_IdEstado] ON [TipoIncapacidad] ([IdEstado]);
GO

CREATE INDEX [IX_TipoHoraExtra_IdEstado] ON [TipoHoraExtra] ([IdEstado]);
GO

CREATE UNIQUE INDEX [IX_ObjetoSistema_NombreEntidad] ON [ObjetoSistema] ([NombreEntidad]);
GO

CREATE INDEX [IX_Asistencia_IdEmpleado] ON [Asistencia] ([IdEmpleado]);
GO

CREATE INDEX [IX_Asistencia_IdEstado] ON [Asistencia] ([IdEstado]);
GO

CREATE INDEX [IX_DepartamentoJefatura_IdDepartamento] ON [DepartamentoJefatura] ([IdDepartamento]);
GO

CREATE INDEX [IX_DepartamentoJefatura_IdEmpleado] ON [DepartamentoJefatura] ([IdEmpleado]);
GO

CREATE INDEX [IX_EmpleadoJerarquia_IdEmpleado] ON [EmpleadoJerarquia] ([IdEmpleado]);
GO

CREATE INDEX [IX_EmpleadoJerarquia_IdSupervisor] ON [EmpleadoJerarquia] ([IdSupervisor]);
GO

CREATE INDEX [IX_Incapacidad_IdEmpleado] ON [Incapacidad] ([IdEmpleado]);
GO

CREATE INDEX [IX_Incapacidad_IdEstado] ON [Incapacidad] ([IdEstado]);
GO

CREATE INDEX [IX_Incapacidad_IdTipoIncapacidad] ON [Incapacidad] ([IdTipoIncapacidad]);
GO

CREATE INDEX [IX_ModoCalculoConceptoNomina_IdEstado] ON [ModoCalculoConceptoNomina] ([IdEstado]);
GO

CREATE INDEX [IX_Permiso_IdEmpleado] ON [Permiso] ([IdEmpleado]);
GO

CREATE INDEX [IX_Permiso_IdEstado] ON [Permiso] ([IdEstado]);
GO

CREATE INDEX [IX_Permiso_IdTipoPermiso] ON [Permiso] ([IdTipoPermiso]);
GO

CREATE INDEX [IX_PlanillaDetalle_IdEmpleado] ON [PlanillaDetalle] ([IdEmpleado]);
GO

CREATE INDEX [IX_PlanillaDetalle_IdEstado] ON [PlanillaDetalle] ([IdEstado]);
GO

CREATE INDEX [IX_PlanillaDetalle_IdPlanilla] ON [PlanillaDetalle] ([IdPlanilla]);
GO

CREATE INDEX [IX_PlanillaDetalleConcepto_IdConceptoNomina] ON [PlanillaDetalleConcepto] ([IdConceptoNomina]);
GO

CREATE INDEX [IX_PlanillaDetalleConcepto_IdEstado] ON [PlanillaDetalleConcepto] ([IdEstado]);
GO

CREATE INDEX [IX_PlanillaDetalleConcepto_IdPlanillaDetalle] ON [PlanillaDetalleConcepto] ([IdPlanillaDetalle]);
GO

CREATE INDEX [IX_PlanillaEncabezado_IdEstado] ON [PlanillaEncabezado] ([IdEstado]);
GO

CREATE INDEX [IX_PlanillaEncabezado_IdTipoPlanilla] ON [PlanillaEncabezado] ([IdTipoPlanilla]);
GO

CREATE INDEX [IX_SolicitudHorasExtra_IdEmpleado] ON [SolicitudHorasExtra] ([IdEmpleado]);
GO

CREATE INDEX [IX_SolicitudHorasExtra_IdEstado] ON [SolicitudHorasExtra] ([IdEstado]);
GO

CREATE INDEX [IX_SolicitudHorasExtra_IdTipoHoraExtra] ON [SolicitudHorasExtra] ([IdTipoHoraExtra]);
GO

CREATE INDEX [IX_SolicitudVacaciones_IdEmpleado] ON [SolicitudVacaciones] ([IdEmpleado]);
GO

CREATE INDEX [IX_SolicitudVacaciones_IdEstado] ON [SolicitudVacaciones] ([IdEstado]);
GO

CREATE INDEX [IX_TipoConceptoNomina_IdEstado] ON [TipoConceptoNomina] ([IdEstado]);
GO

CREATE INDEX [IX_TipoConceptoNomina_IdModoCalculo] ON [TipoConceptoNomina] ([IdModoCalculo]);
GO

CREATE INDEX [IX_TipoPlanilla_IdEstado] ON [TipoPlanilla] ([IdEstado]);
GO

CREATE INDEX [IX_Vacaciones_IdEmpleado] ON [Vacaciones] ([IdEmpleado]);
GO

CREATE INDEX [IX_Vacaciones_IdEstado] ON [Vacaciones] ([IdEstado]);
GO

ALTER TABLE [ObjetoSistemaRol] ADD CONSTRAINT [FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto] FOREIGN KEY ([IdObjeto]) REFERENCES [ObjetoSistema] ([IdObjeto]) ON DELETE NO ACTION;
GO

ALTER TABLE [TipoHoraExtra] ADD CONSTRAINT [FK_TipoHoraExtra_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION;
GO

ALTER TABLE [TipoIncapacidad] ADD CONSTRAINT [FK_TipoIncapacidad_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION;
GO

ALTER TABLE [TipoPermiso] ADD CONSTRAINT [FK_TipoPermiso_Estado_IdEstado] FOREIGN KEY ([IdEstado]) REFERENCES [Estado] ([IdEstado]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260305195044_Sprint1PlanillaBase', N'8.0.22');
GO

COMMIT;
GO

