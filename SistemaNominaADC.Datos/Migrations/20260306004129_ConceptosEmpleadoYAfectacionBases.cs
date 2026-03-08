using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ConceptosEmpleadoYAfectacionBases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AfectaCcss",
                table: "TipoConceptoNomina",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AfectaRenta",
                table: "TipoConceptoNomina",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "EmpleadoConceptoNomina",
                columns: table => new
                {
                    IdEmpleadoConceptoNomina = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEmpleado = table.Column<int>(type: "int", nullable: false),
                    IdConceptoNomina = table.Column<int>(type: "int", nullable: false),
                    MontoFijo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Porcentaje = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    SaldoPendiente = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Prioridad = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    VigenciaDesde = table.Column<DateTime>(type: "date", nullable: true),
                    VigenciaHasta = table.Column<DateTime>(type: "date", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpleadoConceptoNomina", x => x.IdEmpleadoConceptoNomina);
                    table.ForeignKey(
                        name: "FK_EmpleadoConceptoNomina_Empleado_IdEmpleado",
                        column: x => x.IdEmpleado,
                        principalTable: "Empleado",
                        principalColumn: "IdEmpleado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmpleadoConceptoNomina_TipoConceptoNomina_IdConceptoNomina",
                        column: x => x.IdConceptoNomina,
                        principalTable: "TipoConceptoNomina",
                        principalColumn: "IdConceptoNomina",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoConceptoNomina_IdConceptoNomina",
                table: "EmpleadoConceptoNomina",
                column: "IdConceptoNomina");

            migrationBuilder.CreateIndex(
                name: "IX_EmpleadoConceptoNomina_IdEmpleado_IdConceptoNomina_VigenciaDesde_VigenciaHasta",
                table: "EmpleadoConceptoNomina",
                columns: new[] { "IdEmpleado", "IdConceptoNomina", "VigenciaDesde", "VigenciaHasta" },
                unique: true,
                filter: "[Activo] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmpleadoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "AfectaCcss",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "AfectaRenta",
                table: "TipoConceptoNomina");
        }
    }
}
