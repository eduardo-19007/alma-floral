using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlmaFloral.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAdministracionYPostventa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    HashContrasena = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "CasoPostventa",
                columns: table => new
                {
                    CasoPostventaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    Respuesta = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Acuerdo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasoPostventa", x => x.CasoPostventaId);
                    table.CheckConstraint("CK_Caso_Cierre", "[Estado] <> 'Cerrado' OR ([FechaCierre] IS NOT NULL AND [UsuarioId] IS NOT NULL AND [Respuesta] IS NOT NULL AND LEN(LTRIM(RTRIM([Respuesta]))) > 0 AND [Acuerdo] IS NOT NULL AND LEN(LTRIM(RTRIM([Acuerdo]))) > 0)");
                    table.CheckConstraint("CK_Caso_Estado", "[Estado] IN ('Abierto', 'En atención', 'Cerrado')");
                    table.CheckConstraint("CK_Caso_Tipo", "[Tipo] IN ('Consulta', 'Reclamo', 'Solicitud')");
                    table.ForeignKey(
                        name: "FK_CasoPostventa_Pedido_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedido",
                        principalColumn: "PedidoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CasoPostventa_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CasoPostventa_PedidoId",
                table: "CasoPostventa",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_CasoPostventa_UsuarioId",
                table: "CasoPostventa",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CasoPostventa");

            migrationBuilder.DropTable(
                name: "Usuario");
        }
    }
}
