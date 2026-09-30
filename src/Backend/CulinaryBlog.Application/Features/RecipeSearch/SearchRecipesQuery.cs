using System.Globalization;
using System.Text;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public sealed record SearchRecipesQuery(
    string Q,
    Guid? CategoryId = null,
    string? Difficulty = null,
    int? MaxCookTime = null,
    int? MinServings = null,
    string? Sort = null,
    int Page = 1,
    int PageSize = 10)
    : IRequest<PagedResult<RecipeSummaryDto>>;

public sealed class SearchRecipesQueryValidator : AbstractValidator<SearchRecipesQuery>
{
    private static readonly string[] SortFields = ["createdAt", "title", "cookTime"];

    public SearchRecipesQueryValidator()
    {
        RuleFor(query => query.Q)
            .NotEmpty()
            .Must(value => value.Trim().Length >= 2)
            .WithMessage("Search term must contain at least 2 characters.");
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, PagedResult<RecipeSummaryDto>.MaxPageSize);
        RuleFor(query => query.MaxCookTime).GreaterThanOrEqualTo(0).When(query => query.MaxCookTime.HasValue);
        RuleFor(query => query.MinServings).GreaterThan(0).When(query => query.MinServings.HasValue);
        RuleFor(query => query.Difficulty)
            .Must(value => string.IsNullOrWhiteSpace(value) ||
                           (Enum.TryParse<Difficulty>(value, true, out var difficulty) && Enum.IsDefined(difficulty)))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");
        RuleFor(query => query.Sort)
            .Must(value => string.IsNullOrWhiteSpace(value) || IsAllowedSort(value))
            .WithMessage("Sort must be createdAt, title, cookTime, or one of those fields prefixed with '-'.");
    }

    private static bool IsAllowedSort(string sort)
    {
        var field = sort.StartsWith("-", StringComparison.Ordinal) ? sort[1..] : sort;
        return SortFields.Contains(field, StringComparer.OrdinalIgnoreCase);
    }
}

public sealed class SearchRecipesQueryHandler(IRecipeSearchRepository repository)
    : IRequestHandler<SearchRecipesQuery, PagedResult<RecipeSummaryDto>>
{
    public Task<PagedResult<RecipeSummaryDto>> Handle(
        SearchRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var terms = NormalizeTerms(request.Q);
        if (terms.Count == 0)
        {
            return Task.FromResult(new PagedResult<RecipeSummaryDto>([], request.Page, request.PageSize, 0));
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
            prefixQuery,
            request.CategoryId,
            difficulty,
            request.MaxCookTime,
            request.MinServings,
            request.Sort,
            request.Page,
            request.PageSize,
            cancellationToken);
    }

    private static List<string> NormalizeTerms(string value)
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
