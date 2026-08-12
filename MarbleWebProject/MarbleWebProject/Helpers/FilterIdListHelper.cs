using System.Globalization;

namespace MarbleWebProject.Helpers;

/// <summary>
/// Querystring filtre ID parse: <c>b=1,5,12</c> / <c>c=4,9</c>.
/// Legacy: <c>brandId</c> / <c>categoryId</c> hâlâ okunur.
/// </summary>
public static class FilterIdListHelper
{
    public const string BrandQueryKey = "b";
    public const string CategoryQueryKey = "c";

    public static IReadOnlyList<int> Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return Array.Empty<int>();

        var set = new HashSet<int>();
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0)
                continue;
            set.Add(id);
        }

        if (set.Count == 0)
            return Array.Empty<int>();

        return set.OrderBy(x => x).ToList();
    }

    /// <summary>Önce kısa key (<c>b</c>/<c>c</c>), yoksa legacy.</summary>
    public static IReadOnlyList<int> ParsePrefer(string? preferred, string? legacy) =>
        !string.IsNullOrWhiteSpace(preferred) ? Parse(preferred) : Parse(legacy);

    public static IReadOnlyList<int> Normalize(IEnumerable<int>? ids)
    {
        if (ids == null)
            return Array.Empty<int>();

        var set = new HashSet<int>();
        foreach (var id in ids)
        {
            if (id > 0)
                set.Add(id);
        }

        if (set.Count == 0)
            return Array.Empty<int>();

        return set.OrderBy(x => x).ToList();
    }

    public static string? ToQueryValue(IReadOnlyList<int>? ids)
    {
        var normalized = Normalize(ids);
        return normalized.Count == 0 ? null : string.Join(",", normalized);
    }

    public static IReadOnlyList<int> Toggle(IReadOnlyList<int>? current, int id)
    {
        if (id <= 0)
            return Normalize(current);

        var set = new HashSet<int>(Normalize(current));
        if (!set.Add(id))
            set.Remove(id);

        return set.Count == 0 ? Array.Empty<int>() : set.OrderBy(x => x).ToList();
    }
}
