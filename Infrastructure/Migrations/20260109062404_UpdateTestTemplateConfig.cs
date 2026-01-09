using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTestTemplateConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TestTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_TestTemplates_ClusterNumber",
                table: "TestTemplates",
                column: "ClusterNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions",
                column: "TestTemplateId",
                principalTable: "TestTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions");

            migrationBuilder.DropIndex(
                name: "IX_TestTemplates_ClusterNumber",
                table: "TestTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "TestTemplates",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_TestSessions_TestTemplates_TestTemplateId",
                table: "TestSessions",
                column: "TestTemplateId",
                principalTable: "TestTemplates",
                principalColumn: "Id");
        }
    }
}
