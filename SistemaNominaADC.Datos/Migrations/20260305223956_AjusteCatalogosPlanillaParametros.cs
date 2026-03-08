using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AjusteCatalogosPlanillaParametros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoConcepto",
                table: "TipoConceptoNomina",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoFormula",
                table: "TipoConceptoNomina",
                type: "varchar(60)",
                unicode: false,
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrdenCalculo",
                table: "TipoConceptoNomina",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorFijo",
                table: "TipoConceptoNomina",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorPorcentaje",
                table: "TipoConceptoNomina",
                type: "decimal(9,6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipoConceptoNomina_CodigoConcepto",
                table: "TipoConceptoNomina",
                column: "CodigoConcepto",
                unique: true,
                filter: "[CodigoConcepto] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TipoConceptoNomina_CodigoConcepto",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "CodigoConcepto",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "CodigoFormula",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "OrdenCalculo",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "ValorFijo",
                table: "TipoConceptoNomina");

            migrationBuilder.DropColumn(
                name: "ValorPorcentaje",
                table: "TipoConceptoNomina");
        }
    }
}
