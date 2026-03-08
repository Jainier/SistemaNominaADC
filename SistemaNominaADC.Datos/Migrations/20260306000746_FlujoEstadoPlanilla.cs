using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class FlujoEstadoPlanilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlujoEstado",
                columns: table => new
                {
                    IdFlujoEstado = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Entidad = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    IdEstadoOrigen = table.Column<int>(type: "int", nullable: true),
                    IdEstadoDestino = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RequiereRol = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlujoEstado", x => x.IdFlujoEstado);
                    table.ForeignKey(
                        name: "FK_FlujoEstado_Estado_IdEstadoDestino",
                        column: x => x.IdEstadoDestino,
                        principalTable: "Estado",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlujoEstado_Estado_IdEstadoOrigen",
                        column: x => x.IdEstadoOrigen,
                        principalTable: "Estado",
                        principalColumn: "IdEstado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlujoEstado_Entidad_Accion_IdEstadoOrigen_IdEstadoDestino",
                table: "FlujoEstado",
                columns: new[] { "Entidad", "Accion", "IdEstadoOrigen", "IdEstadoDestino" },
                unique: true,
                filter: "[IdEstadoOrigen] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FlujoEstado_IdEstadoDestino",
                table: "FlujoEstado",
                column: "IdEstadoDestino");

            migrationBuilder.CreateIndex(
                name: "IX_FlujoEstado_IdEstadoOrigen",
                table: "FlujoEstado",
                column: "IdEstadoOrigen");

            migrationBuilder.Sql(@"
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

IF @IdPendiente IS NOT NULL AND @IdCalculada IS NOT NULL AND @IdAprobada IS NOT NULL AND @IdInactivo IS NOT NULL
BEGIN
    MERGE FlujoEstado AS T
    USING (VALUES
        ('PlanillaEncabezado', @IdPendiente, @IdPendiente, 'Crear',      NULL, 1),
        ('PlanillaEncabezado', @IdPendiente, @IdPendiente, 'Editar',     NULL, 1),
        ('PlanillaEncabezado', @IdPendiente, @IdCalculada, 'Calcular',   NULL, 1),
        ('PlanillaEncabezado', @IdCalculada, @IdCalculada, 'Recalcular', NULL, 1),
        ('PlanillaEncabezado', @IdCalculada, @IdAprobada,  'Aprobar',    NULL, 1),
        ('PlanillaEncabezado', @IdPendiente, @IdInactivo,  'Desactivar', NULL, 1),
        ('PlanillaEncabezado', @IdCalculada, @IdInactivo,  'Desactivar', NULL, 1)
    ) AS S(Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
      ON  T.Entidad = S.Entidad
      AND ISNULL(T.IdEstadoOrigen, -1) = ISNULL(S.IdEstadoOrigen, -1)
      AND T.IdEstadoDestino = S.IdEstadoDestino
      AND T.Accion = S.Accion
    WHEN MATCHED THEN
      UPDATE SET
        RequiereRol = S.RequiereRol,
        Activo = S.Activo
    WHEN NOT MATCHED THEN
      INSERT (Entidad, IdEstadoOrigen, IdEstadoDestino, Accion, RequiereRol, Activo)
      VALUES (S.Entidad, S.IdEstadoOrigen, S.IdEstadoDestino, S.Accion, S.RequiereRol, S.Activo);
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlujoEstado");
        }
    }
}
