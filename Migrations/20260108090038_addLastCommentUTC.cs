using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteApp.Migrations
{
    /// <inheritdoc />
    public partial class addLastCommentUTC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "lastCommentUTC",
                table: "Comments",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lastCommentUTC",
                table: "Comments");
        }
    }
}
