using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261014120000_AddRecipeImageProcessingStatus")]
public partial class AddRecipeImageProcessingStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsProcessingFailed",
            table: "RecipeImages",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "ProcessingError",
            table: "RecipeImages",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "IsProcessingFailed", table: "RecipeImages");
        migrationBuilder.DropColumn(name: "ProcessingError", table: "RecipeImages");
    }
}
