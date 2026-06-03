namespace MarbleWebProject.Helper;

/// <summary>
/// API'den gelen widget listesini ViewComponentName → IsActive sözlüğüne çevirir (DB ViewComponentName ile eşleşir).
/// </summary>
public static class StorefrontFlagHelper
{
    public static Dictionary<string, bool> BuildFlagMap(
        IEnumerable<(string ViewComponentName, bool IsActive)>? widgets)
    {
        var map = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        if (widgets == null)
            return map;

        foreach (var group in widgets
                     .Where(w => !string.IsNullOrWhiteSpace(w.ViewComponentName))
                     .GroupBy(w => w.ViewComponentName.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            map[group.Key] = group.Any(x => x.IsActive);
        }

        return map;
    }

    public static bool IsActive(IReadOnlyDictionary<string, bool>? flags, string viewComponentName)
    {
        if (flags == null || string.IsNullOrWhiteSpace(viewComponentName))
            return false;

        return flags.TryGetValue(viewComponentName.Trim(), out var enabled) && enabled;
    }
}
