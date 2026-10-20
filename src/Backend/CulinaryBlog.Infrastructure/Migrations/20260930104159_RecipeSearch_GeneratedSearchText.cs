using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecipeSearch_GeneratedSearchText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_Recipes_SearchDocuments ON \"Recipes\";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.recipes_set_search_documents();");
            migrationBuilder.Sql("DROP TEXT SEARCH CONFIGURATION IF EXISTS public.vietnamese;");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SearchText_Trgm",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_SearchVector",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Recipes");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Recipes",
                type: "text",
                nullable: false,
                computedColumnSql: "lower(public.f_unaccent(\"Title\" || ' ' || \"Description\"))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SearchText_Trgm",
                table: "Recipes",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Recipes_SearchText_Trgm",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "SearchText",
                table: "Recipes");

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                table: "Recipes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Recipes",
                type: "tsvector",
                nullable: true);

            migrationBuilder.Sql("CREATE TEXT SEARCH CONFIGURATION public.vietnamese (COPY = pg_catalog.simple);");
            migrationBuilder.Sql("ALTER TEXT SEARCH CONFIGURATION public.vietnamese ALTER MAPPING FOR asciiword, word, hword, hword_part, hword_asciipart WITH unaccent, simple;");
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION public.recipes_set_search_documents()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    NEW."SearchText" := lower(public.f_unaccent(concat_ws(' ', NEW."Title", NEW."Description")));
                    NEW."SearchVector" := to_tsvector('public.vietnamese'::regconfig, NEW."SearchText");
                    RETURN NEW;
                END;
                $$;

                CREATE TRIGGER TR_Recipes_SearchDocuments
                BEFORE INSERT OR UPDATE OF "Title", "Description" ON "Recipes"
                FOR EACH ROW EXECUTE FUNCTION public.recipes_set_search_documents();

                UPDATE "Recipes"
                SET "SearchText" = lower(public.f_unaccent(concat_ws(' ', "Title", "Description"))),
                    "SearchVector" = to_tsvector('public.vietnamese'::regconfig, lower(public.f_unaccent(concat_ws(' ', "Title", "Description"))));
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SearchText",
                table: "Recipes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Recipes",
                type: "tsvector",
                nullable: false,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SearchText_Trgm",
                table: "Recipes",
                column: "SearchText")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_SearchVector",
                table: "Recipes",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "gin");
        }
    }
}
