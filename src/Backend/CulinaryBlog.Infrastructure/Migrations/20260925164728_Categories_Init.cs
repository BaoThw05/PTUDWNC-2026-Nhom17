using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Categories_Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "Name", "OrderIndex", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("0b89ac27-e8d0-4ee2-9e4b-1c0f1a52c001"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món nhẹ dùng trước bữa chính.", null, "Món khai vị", 1, "mon-khai-vi", null },
                    { new Guid("1c89ac27-e8d0-4ee2-9e4b-1c0f1a52c002"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món ăn chính cho bữa cơm hằng ngày.", null, "Món chính", 2, "mon-chinh", null },
                    { new Guid("2d89ac27-e8d0-4ee2-9e4b-1c0f1a52c003"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món ăn kèm cho bữa ăn thêm trọn vẹn.", null, "Món phụ", 3, "mon-phu", null },
                    { new Guid("3e89ac27-e8d0-4ee2-9e4b-1c0f1a52c004"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món canh và súp nóng hổi.", null, "Canh và súp", 4, "canh-va-sup", null },
                    { new Guid("4f89ac27-e8d0-4ee2-9e4b-1c0f1a52c005"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các công thức thuần chay và chay.", null, "Món chay", 5, "mon-chay", null },
                    { new Guid("5a89ac27-e8d0-4ee2-9e4b-1c0f1a52c006"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món ngọt dùng sau bữa ăn.", null, "Món tráng miệng", 6, "mon-trang-mieng", null },
                    { new Guid("6b89ac27-e8d0-4ee2-9e4b-1c0f1a52c007"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các loại thức uống giải khát.", null, "Đồ uống", 7, "do-uong", null },
                    { new Guid("7c89ac27-e8d0-4ee2-9e4b-1c0f1a52c008"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các công thức bánh mặn và bánh ngọt.", null, "Bánh", 8, "banh", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CategoryId",
                table: "Recipes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_Categories_CategoryId",
                table: "Recipes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_Categories_CategoryId",
                table: "Recipes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_CategoryId",
                table: "Recipes");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }
    }
}
