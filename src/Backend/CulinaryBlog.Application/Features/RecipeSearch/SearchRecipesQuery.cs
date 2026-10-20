using System.Globalization;
using System.Text;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;
using ValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public sealed record SearchRecipesQuery(
    string Q,
    Guid? CategoryId = null,
    string? Difficulty = null,
    int? MaxCookTime = null,
    int? MinServings = null,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
{
    public SearchRecipesQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PagedResult<RecipeSummaryDto>.MaxPageSize);
        RuleFor(query => query.MaxCookTime).GreaterThanOrEqualTo(0).When(query => query.MaxCookTime.HasValue);
        RuleFor(query => query.MinServings).GreaterThan(0).When(query => query.MinServings.HasValue);
        RuleFor(query => query.Difficulty)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           (Enum.TryParse<Difficulty>(value, true, out var difficulty) && Enum.IsDefined(difficulty)))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");
    }
}

public sealed class SearchRecipesQueryHandler(IRecipeSearchRepository repository)
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var terms = SearchTermNormalizer.NormalizeTerms(request.Q);
        if (terms.Sum(term => term.Length) < 2)
        {
            throw new ValidationException(
                "SEARCH_QUERY_TOO_SHORT",
                "Search query must contain at least two normalized characters.");
        }

        Difficulty? difficulty = null;
        if (!string.IsNullOrWhiteSpace(request.Difficulty))
        {
            Enum.TryParse(request.Difficulty, true, out Difficulty parsedDifficulty);
            difficulty = parsedDifficulty;
        }

        var normalizedTerm = string.Join(' ', terms);
        var prefixQuery = string.Join(" & ", terms.Select(term => $"{term}:*"));

        return repository.SearchAsync(
            normalizedTerm,
            request.CategoryId,
            difficulty,
            request.MaxCookTime,
            request.MinServings,
            request.Page,
            request.PageSize,
            cancellationToken);
    }

}

internal static class SearchTermNormalizer
{
    public static List<string> NormalizeTerms(string value)
    {
        var decomposed = value.Trim()
            .Replace('đ', 'd')
            .Replace('Đ', 'D')
            .Normalize(NormalizationForm.FormD);
        var terms = new List<string>();
        var token = new StringBuilder();

        foreach (var character in decomposed)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                token.Append(char.ToLowerInvariant(character));
                continue;
            }

            AddToken();
        }

        AddToken();
        return terms;

        void AddToken()
        {
            if (token.Length == 0)
            {
                return;
            }

            terms.Add(token.ToString());
            token.Clear();
        }
    }
}
