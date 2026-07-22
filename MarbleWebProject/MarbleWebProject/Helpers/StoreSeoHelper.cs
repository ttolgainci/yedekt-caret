using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarbleWebProject.Helpers;

public sealed class StoreSeoHead
{
    public string Title { get; init; } = "";
    public string? Keywords { get; init; }
    public string? Description { get; init; }
    public string? CanonicalUrl { get; init; }
    public IReadOnlyList<StoreSeoAlternate>? Alternates { get; init; }
}

public sealed class StoreSeoAlternate
{
    public string Hreflang { get; init; } = "";
    public string Href { get; init; } = "";
}

/// <summary>CMS general SEO — tek noktadan title / meta çözümleme.</summary>
public static class StoreSeoHelper
{
    public static StoreSeoHead Resolve(ViewContext viewContext, bool isHomePage = false)
    {
        var general = AppConfig.Storefront?.General ?? new StoreGeneralSettingsModel();
        var separator = NormalizeSeparator(general.SeoTitleSeparator);

        var pageTitle = FirstNonEmpty(
            viewContext.ViewData["Title"] as string,
            PeekTempData(viewContext, "Title"));
        var pageKeywords = FirstNonEmpty(
            viewContext.ViewData["Keywords"] as string,
            PeekTempData(viewContext, "Keywords"));
        var pageDescription = FirstNonEmpty(
            viewContext.ViewData["Description"] as string,
            PeekTempData(viewContext, "Description"));

        var siteSuffix = FirstNonEmpty(general.DefaultPageTitle, general.StoreName, AppConfig.ProjectName);

        string title;
        if (isHomePage)
        {
            var primary = FirstNonEmpty(general.HomePageTitle, pageTitle);
            title = JoinTitle(primary, siteSuffix, separator) ?? siteSuffix ?? "Mağaza";
        }
        else if (!string.IsNullOrWhiteSpace(pageTitle))
        {
            title = JoinTitle(pageTitle, siteSuffix, separator) ?? pageTitle.Trim();
        }
        else
        {
            title = siteSuffix ?? "Mağaza";
        }

        string? description;
        if (!string.IsNullOrWhiteSpace(pageDescription))
            description = pageDescription.Trim();
        else if (isHomePage && !string.IsNullOrWhiteSpace(general.HomePageMetaDescription))
            description = general.HomePageMetaDescription.Trim();
        else if (!string.IsNullOrWhiteSpace(general.DefaultMetaDescription))
            description = general.DefaultMetaDescription.Trim();
        else
            description = null;

        string? keywords;
        if (!string.IsNullOrWhiteSpace(pageKeywords))
            keywords = pageKeywords.Trim();
        else if (!string.IsNullOrWhiteSpace(general.DefaultMetaKeywords))
            keywords = general.DefaultMetaKeywords.Trim();
        else
            keywords = null;

        string? canonical = null;
        if (viewContext.ViewData["CanonicalUrl"] is string canonicalPath && !string.IsNullOrWhiteSpace(canonicalPath))
        {
            var request = viewContext.HttpContext.Request;
            var path = canonicalPath.StartsWith('/') ? canonicalPath : "/" + canonicalPath;
            canonical = $"{request.Scheme}://{request.Host}{path}";
        }

        IReadOnlyList<StoreSeoAlternate>? alternates = null;
        if (viewContext.ViewData["HreflangAlternates"] is IEnumerable<ProductAlternateUrlModel> altModels)
        {
            var request = viewContext.HttpContext.Request;
            alternates = altModels
                .Where(a => !string.IsNullOrWhiteSpace(a.LanguageCode) && !string.IsNullOrWhiteSpace(a.Path))
                .Select(a =>
                {
                    var path = a.Path.StartsWith('/') ? a.Path : "/" + a.Path;
                    return new StoreSeoAlternate
                    {
                        Hreflang = a.LanguageCode.Trim().ToLowerInvariant(),
                        Href = $"{request.Scheme}://{request.Host}{path}"
                    };
                })
                .ToList();
        }

        return new StoreSeoHead
        {
            Title = title,
            Keywords = keywords,
            Description = description,
            CanonicalUrl = canonical,
            Alternates = alternates
        };
    }

    private static string? PeekTempData(ViewContext viewContext, string key)
        => viewContext.TempData.Peek(key) as string;

    private static string NormalizeSeparator(string? separator)
    {
        var value = separator?.Trim();
        return string.IsNullOrEmpty(value) ? "|" : value;
    }

    /// <summary>{primary} {separator} {suffix} — suffix yoksa veya primary ile aynıysa sadece primary.</summary>
    private static string? JoinTitle(string? primary, string? suffix, string separator)
    {
        var left = primary?.Trim();
        var right = suffix?.Trim();

        if (string.IsNullOrWhiteSpace(left))
            return null;

        if (string.IsNullOrWhiteSpace(right)
            || string.Equals(left, right, StringComparison.OrdinalIgnoreCase))
            return left;

        return $"{left} {separator} {right}";
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }
        return null;
    }
}
