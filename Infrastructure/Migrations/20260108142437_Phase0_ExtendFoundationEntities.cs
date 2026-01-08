using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase0_ExtendFoundationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SchoolName",
                table: "UserProfiles",
                newName: "Province");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "UserProfiles",
                newName: "District");

            migrationBuilder.AddColumn<int>(
                name: "CurrentLeagueId",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EloRating",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTestDate",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchoolId",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetMajorId",
                table: "UserProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClusterNumber",
                table: "TestSessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TestSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QuestionIdsJson",
                table: "TestSessions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "TestSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Leagues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "Leagues",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "PromotionThreshold",
                table: "Leagues",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RelegationThreshold",
                table: "Leagues",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Player1AnswersJson",
                table: "DuelMatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Player2AnswersJson",
                table: "DuelMatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionIdsJson",
                table: "DuelMatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DuelMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TimeLimit",
                table: "DuelMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateTable(
                name: "Schools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Province = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    District = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Universities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Universities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniversityId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faculties_Universities_UniversityId",
                        column: x => x.UniversityId,
                        principalTable: "Universities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FacultyId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MinScore2024 = table.Column<int>(type: "integer", nullable: false),
                    MinScore2025 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Majors_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_CurrentLeagueId",
                table: "UserProfiles",
                column: "CurrentLeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_SchoolId",
                table: "UserProfiles",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_TargetMajorId",
                table: "UserProfiles",
                column: "TargetMajorId");

            migrationBuilder.CreateIndex(
                name: "IX_TestSessions_SubjectId",
                table: "TestSessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_UniversityId",
                table: "Faculties",
                column: "UniversityId");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_FacultyId",
                table: "Majors",
                column: "FacultyId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSessions_Subjects_SubjectId",
                table: "TestSessions",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Leagues_CurrentLeagueId",
                table: "UserProfiles",
                column: "CurrentLeagueId",
                principalTable: "Leagues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Majors_TargetMajorId",
                table: "UserProfiles",
                column: "TargetMajorId",
                principalTable: "Majors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfiles_Schools_SchoolId",
                table: "UserProfiles",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSessions_Subjects_SubjectId",
                table: "TestSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Leagues_CurrentLeagueId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Majors_TargetMajorId",
                table: "UserProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_UserProfiles_Schools_SchoolId",
                table: "UserProfiles");

            migrationBuilder.DropTable(
                name: "ClusterDefinitions");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.DropTable(
                name: "Schools");

            migrationBuilder.DropTable(
                name: "Faculties");

            migrationBuilder.DropTable(
                name: "Universities");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_CurrentLeagueId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_SchoolId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_TargetMajorId",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_TestSessions_SubjectId",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "CurrentLeagueId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "EloRating",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "LastTestDate",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "SchoolId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "TargetMajorId",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ClusterNumber",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "QuestionIdsJson",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "TestSessions");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "PromotionThreshold",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "RelegationThreshold",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "Player1AnswersJson",
                table: "DuelMatches");

            migrationBuilder.DropColumn(
                name: "Player2AnswersJson",
                table: "DuelMatches");

            migrationBuilder.DropColumn(
                name: "QuestionIdsJson",
                table: "DuelMatches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DuelMatches");

            migrationBuilder.DropColumn(
                name: "TimeLimit",
                table: "DuelMatches");

            migrationBuilder.RenameColumn(
                name: "Province",
                table: "UserProfiles",
                newName: "SchoolName");

            migrationBuilder.RenameColumn(
                name: "District",
                table: "UserProfiles",
                newName: "City");
        }
    }
}
