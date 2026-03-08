using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class TramosRentaSalario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TramoRentaSalario",
                columns: table => new
                {
                    IdTramoRentaSalario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DesdeMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HastaMonto = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Tasa = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    VigenciaDesde = table.Column<DateTime>(type: "date", nullable: false),
                    VigenciaHasta = table.Column<DateTime>(type: "date", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TramoRentaSalario", x => x.IdTramoRentaSalario);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TramoRentaSalario");
        }
    }
}
