using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestTemplateId",
                table: "TestSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TestTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SubjectDistributionJson = table.Column<string>(type: "text", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_TestTemplateId",
                table: "TestSessions",
                column: "TestTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions",
                column: "TestTemplateId",
                principalTable: "TestTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions");

            migrationBuilder.DropTable(
                name: "TestTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TestSessions_TestTemplateId",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "TestTemplateId",
                table: "TestSessions");
        }
    }
}
