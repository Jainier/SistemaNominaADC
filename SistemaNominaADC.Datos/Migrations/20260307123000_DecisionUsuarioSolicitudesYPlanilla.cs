using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SistemaNominaADC.Datos;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260307123000_DecisionUsuarioSolicitudesYPlanilla")]
    public class DecisionUsuarioSolicitudesYPlanilla : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdentityUserIdDecision",
                table: "Permiso",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserIdDecision",
                table: "SolicitudHorasExtra",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserIdDecision",
                table: "SolicitudVacaciones",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserIdDecision",
                table: "Incapacidad",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserIdDecision",
                table: "PlanillaEncabezado",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdentityUserIdDecision",
                table: "Permiso");

            migrationBuilder.DropColumn(
                name: "IdentityUserIdDecision",
                table: "SolicitudHorasExtra");

            migrationBuilder.DropColumn(
                name: "IdentityUserIdDecision",
                table: "SolicitudVacaciones");

            migrationBuilder.DropColumn(
                name: "IdentityUserIdDecision",
                table: "Incapacidad");

            migrationBuilder.DropColumn(
                name: "IdentityUserIdDecision",
                table: "PlanillaEncabezado");
        }
    }
}
