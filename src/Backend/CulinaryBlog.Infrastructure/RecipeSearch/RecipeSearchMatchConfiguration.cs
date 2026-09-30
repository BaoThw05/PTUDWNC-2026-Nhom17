using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeSearchMatchConfiguration : IEntityTypeConfiguration<RecipeSearchMatch>
{
    public void Configure(EntityTypeBuilder<RecipeSearchMatch> builder)
    {
        builder.HasNoKey();
        builder.ToView(null);
    }
}
