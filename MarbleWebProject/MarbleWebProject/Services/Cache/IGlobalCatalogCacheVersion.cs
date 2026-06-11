namespace MarbleWebProject.Services.Cache;

public interface IGlobalCatalogCacheVersion
{
    int GetCurrent(string? tenant = null);
}
