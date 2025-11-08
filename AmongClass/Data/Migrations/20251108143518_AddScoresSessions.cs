using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmongClass.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScoresSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Score_IdentityUser<Guid>_StudentId",
                table: "Score");

            migrationBuilder.DropForeignKey(
                name: "FK_Score_Sessions_SessionId",
                table: "Score");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Score",
                table: "Score");

            migrationBuilder.RenameTable(
                name: "Score",
                newName: "Scores");

            migrationBuilder.RenameIndex(
                name: "IX_Score_StudentId",
                table: "Scores",
                newName: "IX_Scores_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Score_SessionId",
                table: "Scores",
                newName: "IX_Scores_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Scores",
                table: "Scores",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_IdentityUser<Guid>_StudentId",
                table: "Scores",
                column: "StudentId",
                principalTable: "IdentityUser<Guid>",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scores_Sessions_SessionId",
                table: "Scores",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Scores_IdentityUser<Guid>_StudentId",
                table: "Scores");

            migrationBuilder.DropForeignKey(
                name: "FK_Scores_Sessions_SessionId",
                table: "Scores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Scores",
                table: "Scores");

            migrationBuilder.RenameTable(
                name: "Scores",
                newName: "Score");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_StudentId",
                table: "Score",
                newName: "IX_Score_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Scores_SessionId",
                table: "Score",
                newName: "IX_Score_SessionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Score",
                table: "Score",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Score_IdentityUser<Guid>_StudentId",
                table: "Score",
                column: "StudentId",
                principalTable: "IdentityUser<Guid>",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Score_Sessions_SessionId",
                table: "Score",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
