using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarObjetoSistemaRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ObjetoSistemaRol",
                columns: table => new
                {
                    IdObjeto = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjetoSistemaRol", x => new { x.IdObjeto, x.RoleName });
                    table.ForeignKey(
                        name: "FK_ObjetoSistemaRol_ObjetoSistema_IdObjeto",
                        column: x => x.IdObjeto,
                        principalTable: "ObjetoSistema",
                        principalColumn: "IdObjeto",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjetoSistemaRol");
        }
    }
}
