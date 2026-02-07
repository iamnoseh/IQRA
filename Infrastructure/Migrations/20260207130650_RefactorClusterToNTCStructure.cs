using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClusterToNTCStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClusterDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_TestTemplates_ClusterNumber",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "SubjectDistributionJson",
                table: "TestTemplates");

            migrationBuilder.RenameColumn(
                name: "TotalQuestions",
                table: "TestTemplates",
                newName: "SingleChoiceCount");

            migrationBuilder.RenameColumn(
                name: "ClusterNumber",
                table: "TestTemplates",
                newName: "MatchingCount");

            migrationBuilder.AddColumn<int>(
                name: "ClosedAnswerCount",
                table: "TestTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "TestTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComponentType",
                table: "TestTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TestTemplates",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ClusterId",
                table: "TestSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComponentType",
                table: "TestSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterNumber = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClusterSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterId = table.Column<int>(type: "integer", nullable: false),
                    SubjectId = table.Column<int>(type: "integer", nullable: false),
                    ComponentType = table.Column<int>(type: "integer", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClusterSubjects_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClusterSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplates_ClusterId_ComponentType",
                table: "TestTemplates",
                columns: new[] { "ClusterId", "ComponentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_ClusterNumber",
                table: "Clusters",
                column: "ClusterNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClusterSubjects_ClusterId_SubjectId_ComponentType",
                table: "ClusterSubjects",
                columns: new[] { "ClusterId", "SubjectId", "ComponentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClusterSubjects_SubjectId",
                table: "ClusterSubjects",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestTemplates_Clusters_ClusterId",
                table: "TestTemplates",
                column: "ClusterId",
                principalTable: "Clusters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestTemplates_Clusters_ClusterId",
                table: "TestTemplates");

            migrationBuilder.DropTable(
                name: "ClusterSubjects");

            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.DropIndex(
                name: "IX_TestTemplates_ClusterId_ComponentType",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "ClosedAnswerCount",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "ComponentType",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TestTemplates");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "ComponentType",
                table: "TestSessions");

            migrationBuilder.RenameColumn(
                name: "SingleChoiceCount",
                table: "TestTemplates",
                newName: "TotalQuestions");

            migrationBuilder.RenameColumn(
                name: "MatchingCount",
                table: "TestTemplates",
                newName: "ClusterNumber");

            migrationBuilder.AddColumn<string>(
                name: "SubjectDistributionJson",
                table: "TestTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ClusterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClusterNumber = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SubjectIdsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterDefinitions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplates_ClusterNumber",
                table: "TestTemplates",
                column: "ClusterNumber");
        }
    }
}
