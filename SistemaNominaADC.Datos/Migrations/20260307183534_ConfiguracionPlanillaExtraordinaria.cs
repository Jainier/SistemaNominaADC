using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ConfiguracionPlanillaExtraordinaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AportaBaseCcss",
                table: "TipoPlanilla",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AportaBaseRentaMensual",
                table: "TipoPlanilla",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "ModoCalculo",
                table: "TipoPlanilla",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.CreateTable(
                name: "TipoPlanillaConcepto",
                columns: table => new
                {
                    IdTipoPlanilla = table.Column<int>(type: "int", nullable: false),
                    IdConceptoNomina = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Obligatorio = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PermiteMontoManual = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Prioridad = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoPlanillaConcepto", x => new { x.IdTipoPlanilla, x.IdConceptoNomina });
                    table.ForeignKey(
                        name: "FK_TipoPlanillaConcepto_TipoConceptoNomina_IdConceptoNomina",
                        column: x => x.IdConceptoNomina,
                        principalTable: "TipoConceptoNomina",
                        principalColumn: "IdConceptoNomina",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TipoPlanillaConcepto_TipoPlanilla_IdTipoPlanilla",
                        column: x => x.IdTipoPlanilla,
                        principalTable: "TipoPlanilla",
                        principalColumn: "IdTipoPlanilla",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TipoPlanillaConcepto_IdConceptoNomina",
                table: "TipoPlanillaConcepto",
                column: "IdConceptoNomina");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TipoPlanillaConcepto");

            migrationBuilder.DropColumn(
                name: "AportaBaseCcss",
                table: "TipoPlanilla");

            migrationBuilder.DropColumn(
                name: "AportaBaseRentaMensual",
                table: "TipoPlanilla");

            migrationBuilder.DropColumn(
                name: "ModoCalculo",
                table: "TipoPlanilla");
        }
    }
}
