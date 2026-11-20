using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TV4SampleCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "ImageUrl", "Name", "OrderIndex", "Slug", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("8d89ac27-e8d0-4ee2-9e4b-1c0f1a52c009"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món được làm chín bằng hơi nước.", null, "Món hấp", 9, "mon-hap", null },
                    { new Guid("9e89ac27-e8d0-4ee2-9e4b-1c0f1a52c010"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món nướng thơm ngon.", null, "Món nướng", 10, "mon-nuong", null },
                    { new Guid("a089ac27-e8d0-4ee2-9e4b-1c0f1a52c011"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món xào nhanh cho bữa ăn hằng ngày.", null, "Món xào", 11, "mon-xao", null },
                    { new Guid("a689ac27-e8d0-4ee2-9e4b-1c0f1a52c017"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món gỏi và salad.", null, "Gỏi - salad", 17, "goi-salad", null },
                    { new Guid("b189ac27-e8d0-4ee2-9e4b-1c0f1a52c012"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món kho đậm đà.", null, "Món kho", 12, "mon-kho", null },
                    { new Guid("b789ac27-e8d0-4ee2-9e4b-1c0f1a52c018"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món cuốn và trộn.", null, "Món cuốn - trộn", 18, "mon-cuon-tron", null },
                    { new Guid("c289ac27-e8d0-4ee2-9e4b-1c0f1a52c013"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món chiên giòn và áp chảo.", null, "Món chiên", 13, "mon-chien", null },
                    { new Guid("d389ac27-e8d0-4ee2-9e4b-1c0f1a52c014"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món lẩu dùng chung.", null, "Món lẩu", 14, "mon-lau", null },
                    { new Guid("e489ac27-e8d0-4ee2-9e4b-1c0f1a52c015"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món cháo nóng.", null, "Món cháo", 15, "mon-chao", null },
                    { new Guid("f589ac27-e8d0-4ee2-9e4b-1c0f1a52c016"), new DateTimeOffset(new DateTime(2026, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Các món chè ngọt.", null, "Món chè", 16, "mon-che", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8d89ac27-e8d0-4ee2-9e4b-1c0f1a52c009"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("9e89ac27-e8d0-4ee2-9e4b-1c0f1a52c010"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a089ac27-e8d0-4ee2-9e4b-1c0f1a52c011"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a689ac27-e8d0-4ee2-9e4b-1c0f1a52c017"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b189ac27-e8d0-4ee2-9e4b-1c0f1a52c012"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("b789ac27-e8d0-4ee2-9e4b-1c0f1a52c018"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c289ac27-e8d0-4ee2-9e4b-1c0f1a52c013"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("d389ac27-e8d0-4ee2-9e4b-1c0f1a52c014"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e489ac27-e8d0-4ee2-9e4b-1c0f1a52c015"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("f589ac27-e8d0-4ee2-9e4b-1c0f1a52c016"));
        }
    }
}
