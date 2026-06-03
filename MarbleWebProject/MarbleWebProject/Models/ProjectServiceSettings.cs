namespace MarbleWebProject.Models;

/// <summary>Müşteri kurulumu (appsettings ProjectService) — CMS ile aynı şema.</summary>
public sealed class ProjectServiceSettings
{
    public const string SectionName = "ProjectService";

    public StoreSectorType StoreSectorType { get; set; } = StoreSectorType.Apparel;
    public string SectorCode { get; set; } = "";
    public string ProjectName { get; set; } = "";

    /// <summary>Header ana menüde gösterilecek üst kategori sayısı; fazlası Browse Categories içinde.</summary>
    public int HeaderNavMaxVisibleCategories { get; set; } = 9;

    public bool IsAutoParts => ProjectSector.IsAutoParts(SectorCode);
    public bool IsApparelOrElectronics => !IsAutoParts;

    public static ProjectServiceSettings FromConfiguration(IConfiguration configuration)
    {
        var settings = configuration.GetSection(SectionName).Get<ProjectServiceSettings>()
            ?? new ProjectServiceSettings();
        ProjectSector.ApplyConfigurationDefaults(settings);
        return settings;
    }
}
