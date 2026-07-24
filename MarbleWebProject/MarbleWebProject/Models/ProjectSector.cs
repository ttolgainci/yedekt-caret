using MarbleWebProject.Models.Options;

namespace MarbleWebProject.Models;

public static class ProjectSector
{
    public static string ResolveSectorCode(string? sectorCode) =>
        string.IsNullOrWhiteSpace(sectorCode)
            ? AppConfig.Storefront.StoreAuth.SectorCode
            : Normalize(sectorCode, AppConfig.Storefront.StoreAuth.SectorCode);

    public static bool IsAutoParts(string? sectorCode) =>
        string.Equals(Normalize(sectorCode), StoreSectorCodes.AutoParts, StringComparison.Ordinal);

    public static bool IsAutoParts(StoreSectorType sectorType) =>
        sectorType == StoreSectorType.AutoParts;

    public static string ToSectorCode(StoreSectorType sectorType) => sectorType switch
    {
        StoreSectorType.AutoParts => StoreSectorCodes.AutoParts,
        StoreSectorType.Electronics => StoreSectorCodes.Electronics,
        _ => StoreSectorCodes.Apparel
    };

    public static StoreSectorType ToSectorType(string? sectorCode) =>
        Normalize(sectorCode) switch
        {
            StoreSectorCodes.AutoParts => StoreSectorType.AutoParts,
            StoreSectorCodes.Electronics => StoreSectorType.Electronics,
            _ => StoreSectorType.Apparel
        };

    public static StoreSectorType ParseSectorType(int value, StoreSectorType fallback = StoreSectorType.Apparel) =>
        Enum.IsDefined(typeof(StoreSectorType), value) ? (StoreSectorType)value : fallback;

    public static string Normalize(string? sectorCode, string? defaultCode = null)
    {
        if (string.IsNullOrWhiteSpace(sectorCode))
            return string.IsNullOrWhiteSpace(defaultCode) ? StoreSectorCodes.Apparel : Normalize(defaultCode);

        var s = sectorCode.Trim().ToUpperInvariant();
        return s switch
        {
            StoreSectorCodes.AutoParts => StoreSectorCodes.AutoParts,
            StoreSectorCodes.Electronics => StoreSectorCodes.Electronics,
            StoreSectorCodes.Apparel => StoreSectorCodes.Apparel,
            _ => string.IsNullOrWhiteSpace(defaultCode) ? StoreSectorCodes.Apparel : Normalize(defaultCode)
        };
    }

    public static void ApplyConfigurationDefaults(StoreAuthOptions settings)
    {
        settings.StoreSectorType = ParseSectorType((int)settings.StoreSectorType, StoreSectorType.Apparel);

        if (string.IsNullOrWhiteSpace(settings.SectorCode))
            settings.SectorCode = ToSectorCode(settings.StoreSectorType);
        else
            settings.SectorCode = Normalize(settings.SectorCode, ToSectorCode(settings.StoreSectorType));

        settings.StoreSectorType = ToSectorType(settings.SectorCode);
        settings.ProjectName = string.IsNullOrWhiteSpace(settings.ProjectName)
            ? "default"
            : settings.ProjectName.Trim();
    }
}
