using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class EliminarRolConceptoManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RolConcepto",
                table: "TipoConceptoNomina");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RolConcepto",
                table: "TipoConceptoNomina",
                type: "varchar(40)",
                unicode: false,
                maxLength: 40,
                nullable: true);
        }
    }
}
