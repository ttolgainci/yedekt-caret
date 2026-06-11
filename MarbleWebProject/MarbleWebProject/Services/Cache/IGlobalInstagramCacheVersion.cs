namespace MarbleWebProject.Services.Cache;

public interface IGlobalInstagramCacheVersion
{
    int GetCurrent(string? tenant = null);
}
