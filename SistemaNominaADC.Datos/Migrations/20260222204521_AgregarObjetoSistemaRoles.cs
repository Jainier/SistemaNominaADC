using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaNominaADC.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarObjetoSistemaRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Puesto");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Empleado");

            migrationBuilder.AddColumn<int>(
                name: "IdEstado",
                table: "Puesto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "NombreEntidad",
                table: "ObjetoSistema",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "GrupoEstado",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "GrupoEstado",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Estado",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Estado",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdEstado",
                table: "Empleado",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EsSistema",
                table: "AspNetRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.CreateTable(
                name: "TipoHoraExtra",
                columns: table => new
                {
                    IdTipoHoraExtra = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoHoraExtra", x => x.IdTipoHoraExtra);
                });

            migrationBuilder.CreateTable(
                name: "TipoIncapacidad",
                columns: table => new
                {
                    IdTipoIncapacidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoIncapacidad", x => x.IdTipoIncapacidad);
                });

            migrationBuilder.CreateTable(
                name: "TipoPermiso",
                columns: table => new
                {
                    IdTipoPermiso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoPermiso", x => x.IdTipoPermiso);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puesto_IdEstado",
                table: "Puesto",
                column: "IdEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Empleado_IdEstado",
                table: "Empleado",
                column: "IdEstado");

            migrationBuilder.AddForeignKey(
                name: "FK_Empleado_Estado_IdEstado",
                table: "Empleado",
                column: "IdEstado",
                principalTable: "Estado",
                principalColumn: "IdEstado",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Puesto_Estado_IdEstado",
                table: "Puesto",
                column: "IdEstado",
                principalTable: "Estado",
                principalColumn: "IdEstado",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empleado_Estado_IdEstado",
                table: "Empleado");

            migrationBuilder.DropForeignKey(
                name: "FK_Puesto_Estado_IdEstado",
                table: "Puesto");

            migrationBuilder.DropTable(
                name: "ObjetoSistemaRol");

            migrationBuilder.DropTable(
                name: "TipoHoraExtra");

            migrationBuilder.DropTable(
                name: "TipoIncapacidad");

            migrationBuilder.DropTable(
                name: "TipoPermiso");

            migrationBuilder.DropIndex(
                name: "IX_Puesto_IdEstado",
                table: "Puesto");

            migrationBuilder.DropIndex(
                name: "IX_Empleado_IdEstado",
                table: "Empleado");

            migrationBuilder.DropColumn(
                name: "IdEstado",
                table: "Puesto");

            migrationBuilder.DropColumn(
                name: "IdEstado",
                table: "Empleado");

            migrationBuilder.DropColumn(
                name: "EsSistema",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Puesto",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "NombreEntidad",
                table: "ObjetoSistema",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "GrupoEstado",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "GrupoEstado",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Estado",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Estado",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Empleado",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
