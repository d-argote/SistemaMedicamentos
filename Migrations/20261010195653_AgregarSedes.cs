using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaMedicamentos.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSedes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SedeId",
                table: "Entregas",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1); // Las entregas existentes quedan en la sede 1 (creada por SeedData)

            migrationBuilder.CreateTable(
                name: "Sedes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Ciudad = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sedes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregas_SedeId",
                table: "Entregas",
                column: "SedeId");

            // Sedes base: deben existir antes de crear la FK (las entregas existentes apuntan a la sede 1)
            migrationBuilder.InsertData(
                table: "Sedes",
                columns: new[] { "Id", "Nombre", "Direccion", "Ciudad", "Telefono", "Activa" },
                values: new object[,]
                {
                    { 1, "Sede Central Bogotá", "Calle 45 # 12-30", "Bogotá", "6012345678", true },
                    { 2, "Sede Norte Medellín", "Carrera 70 # 44-15", "Medellín", "6044567890", true },
                    { 3, "Sede Sur Cali", "Av. Roosevelt # 25-40", "Cali", "6023456789", true }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Entregas_Sedes_SedeId",
                table: "Entregas",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entregas_Sedes_SedeId",
                table: "Entregas");

            migrationBuilder.DeleteData(table: "Sedes", keyColumn: "Id", keyValues: new object[] { 1, 2, 3 });

            migrationBuilder.DropTable(
                name: "Sedes");

            migrationBuilder.DropIndex(
                name: "IX_Entregas_SedeId",
                table: "Entregas");

            migrationBuilder.DropColumn(
                name: "SedeId",
                table: "Entregas");
        }
    }
}
