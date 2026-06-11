namespace MarbleWebProject.Services.Cache;

public interface IGlobalContentCacheVersion
{
    int GetCurrent(string? tenant = null);
}
