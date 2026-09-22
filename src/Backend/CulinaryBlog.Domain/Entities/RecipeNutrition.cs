namespace CulinaryBlog.Domain.Entities;

public class RecipeNutrition
{
    public int? Calories { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? FatGrams { get; set; }
    public decimal? CarbsGrams { get; set; }
    public decimal? FiberGrams { get; set; }
    public decimal? SugarGrams { get; set; }
}