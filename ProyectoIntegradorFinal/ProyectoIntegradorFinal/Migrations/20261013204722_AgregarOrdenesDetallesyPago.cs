using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoIntegradorFinal.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrdenesDetallesyPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCarrito_Carritos_IdCarrito",
                table: "ItemCarrito");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemCarrito_Productos_IdProducto",
                table: "ItemCarrito");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemCarrito",
                table: "ItemCarrito");

            migrationBuilder.RenameTable(
                name: "ItemCarrito",
                newName: "ItemsCarrito");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCarrito_IdProducto",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_IdProducto");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCarrito_IdCarrito",
                table: "ItemsCarrito",
                newName: "IX_ItemsCarrito_IdCarrito");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemsCarrito",
                table: "ItemsCarrito",
                column: "IdItem");

            migrationBuilder.CreateTable(
                name: "Ordenes",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdDireccion = table.Column<int>(type: "int", nullable: false),
                    FechaOrden = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ordenes", x => x.IdOrden);
                    table.ForeignKey(
                        name: "FK_Ordenes_Direccion_IdDireccion",
                        column: x => x.IdDireccion,
                        principalTable: "Direccion",
                        principalColumn: "IdDireccion",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ordenes_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesOrden",
                columns: table => new
                {
                    IdDetalle = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    IdProducto = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesOrden", x => x.IdDetalle);
                    table.ForeignKey(
                        name: "FK_DetallesOrden_Ordenes_IdOrden",
                        column: x => x.IdOrden,
                        principalTable: "Ordenes",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesOrden_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "IdProducto",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    IdPago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IdOrden = table.Column<int>(type: "int", nullable: false),
                    FechaPago = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EstadoPago = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.IdPago);
                    table.ForeignKey(
                        name: "FK_Pagos_Ordenes_IdOrden",
                        column: x => x.IdOrden,
                        principalTable: "Ordenes",
                        principalColumn: "IdOrden",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrden_IdOrden",
                table: "DetallesOrden",
                column: "IdOrden");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesOrden_IdProducto",
                table: "DetallesOrden",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_IdDireccion",
                table: "Ordenes",
                column: "IdDireccion");

            migrationBuilder.CreateIndex(
                name: "IX_Ordenes_IdUsuario",
                table: "Ordenes",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_IdOrden",
                table: "Pagos",
                column: "IdOrden");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_Carritos_IdCarrito",
                table: "ItemsCarrito",
                column: "IdCarrito",
                principalTable: "Carritos",
                principalColumn: "IdCarrito",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemsCarrito_Productos_IdProducto",
                table: "ItemsCarrito",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_Carritos_IdCarrito",
                table: "ItemsCarrito");

            migrationBuilder.DropForeignKey(
                name: "FK_ItemsCarrito_Productos_IdProducto",
                table: "ItemsCarrito");

            migrationBuilder.DropTable(
                name: "DetallesOrden");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Ordenes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemsCarrito",
                table: "ItemsCarrito");

            migrationBuilder.RenameTable(
                name: "ItemsCarrito",
                newName: "ItemCarrito");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_IdProducto",
                table: "ItemCarrito",
                newName: "IX_ItemCarrito_IdProducto");

            migrationBuilder.RenameIndex(
                name: "IX_ItemsCarrito_IdCarrito",
                table: "ItemCarrito",
                newName: "IX_ItemCarrito_IdCarrito");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemCarrito",
                table: "ItemCarrito",
                column: "IdItem");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCarrito_Carritos_IdCarrito",
                table: "ItemCarrito",
                column: "IdCarrito",
                principalTable: "Carritos",
                principalColumn: "IdCarrito",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCarrito_Productos_IdProducto",
                table: "ItemCarrito",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "IdProducto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
