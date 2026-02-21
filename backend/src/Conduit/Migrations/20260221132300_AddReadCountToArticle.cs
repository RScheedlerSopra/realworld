using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conduit.Migrations;

/// <inheritdoc />
public partial class AddReadCountToArticle : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ReadCount",
            table: "Articles",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReadCount",
            table: "Articles");
    }
}
