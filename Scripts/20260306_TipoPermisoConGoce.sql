IF COL_LENGTH('dbo.TipoPermiso', 'ConGoceSalarial') IS NULL
BEGIN
    ALTER TABLE dbo.TipoPermiso
    ADD ConGoceSalarial bit NOT NULL
        CONSTRAINT DF_TipoPermiso_ConGoceSalarial DEFAULT (1);
END;
GO

-- Opcional: marcar tipos existentes sin goce por nombre.
-- Ajusta estos nombres a los que tengas en tu catalogo.
UPDATE dbo.TipoPermiso
SET ConGoceSalarial = 0
WHERE UPPER(Nombre) IN (
    'PERMISO SIN GOCE',
    'SIN GOCE',
    'LICENCIA SIN GOCE'
);
GO
