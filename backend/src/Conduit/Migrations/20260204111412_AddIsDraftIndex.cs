using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Conduit.Migrations;

/// <inheritdoc />
public partial class AddIsDraftIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Articles_IsDraft",
            table: "Articles",
            column: "IsDraft");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Articles_IsDraft",
            table: "Articles");
    }
}
