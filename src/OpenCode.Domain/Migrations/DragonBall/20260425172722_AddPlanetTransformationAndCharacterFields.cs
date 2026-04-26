using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OpenCode.Domain.Migrations.DragonBall
{
    /// <inheritdoc />
    public partial class AddPlanetTransformationAndCharacterFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "dragonball",
                table: "Characters",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ki",
                schema: "dragonball",
                table: "Characters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaxKi",
                schema: "dragonball",
                table: "Characters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlanetId",
                schema: "dragonball",
                table: "Characters",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Race",
                schema: "dragonball",
                table: "Characters",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Planets",
                schema: "dragonball",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transformations",
                schema: "dragonball",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ki = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CharacterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transformations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transformations_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalSchema: "dragonball",
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Characters_PlanetId",
                schema: "dragonball",
                table: "Characters",
                column: "PlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Transformations_CharacterId",
                schema: "dragonball",
                table: "Transformations",
                column: "CharacterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Planets_PlanetId",
                schema: "dragonball",
                table: "Characters",
                column: "PlanetId",
                principalSchema: "dragonball",
                principalTable: "Planets",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Planets_PlanetId",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropTable(
                name: "Planets",
                schema: "dragonball");

            migrationBuilder.DropTable(
                name: "Transformations",
                schema: "dragonball");

            migrationBuilder.DropIndex(
                name: "IX_Characters_PlanetId",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Ki",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "MaxKi",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "PlanetId",
                schema: "dragonball",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "Race",
                schema: "dragonball",
                table: "Characters");
        }
    }
}
