using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conduit.Migrations;

/// <inheritdoc />
public partial class AddIsDraftToArticle : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDraft",
            table: "Articles",
            type: "INTEGER",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsDraft",
            table: "Articles");
    }
}
