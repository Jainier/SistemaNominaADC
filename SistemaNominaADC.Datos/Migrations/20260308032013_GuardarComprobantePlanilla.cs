using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class GuardarComprobantePlanilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ComprobantePdf",
                table: "PlanillaDetalle",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaGeneracionComprobantePdf",
                table: "PlanillaDetalle",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashComprobantePdf",
                table: "PlanillaDetalle",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreComprobantePdf",
                table: "PlanillaDetalle",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComprobantePdf",
                table: "PlanillaDetalle");

            migrationBuilder.DropColumn(
                name: "FechaGeneracionComprobantePdf",
                table: "PlanillaDetalle");

            migrationBuilder.DropColumn(
                name: "HashComprobantePdf",
                table: "PlanillaDetalle");

            migrationBuilder.DropColumn(
                name: "NombreComprobantePdf",
                table: "PlanillaDetalle");
        }
    }
}
