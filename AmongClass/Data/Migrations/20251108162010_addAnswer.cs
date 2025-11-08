using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmongClass.Data.Migrations
{
    /// <inheritdoc />
    public partial class addAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_AspNetUsers_VoterId1",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "IsAI",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "VoterId",
                table: "Votes");

            migrationBuilder.RenameColumn(
                name: "VoterId1",
                table: "Votes",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_VoterId1",
                table: "Votes",
                newName: "IX_Votes_UserId");

            migrationBuilder.AddColumn<DateTime>(
                name: "VotedAt",
                table: "Votes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_AspNetUsers_UserId",
                table: "Votes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_AspNetUsers_UserId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "VotedAt",
                table: "Votes");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Votes",
                newName: "VoterId1");

            migrationBuilder.RenameIndex(
                name: "IX_Votes_UserId",
                table: "Votes",
                newName: "IX_Votes_VoterId1");

            migrationBuilder.AddColumn<bool>(
                name: "IsAI",
                table: "Votes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "VoterId",
                table: "Votes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_AspNetUsers_VoterId1",
                table: "Votes",
                column: "VoterId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
