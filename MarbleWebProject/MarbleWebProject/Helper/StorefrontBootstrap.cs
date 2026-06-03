using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;

namespace MarbleWebProject.Helper;

public static class StorefrontBootstrap
{
    public static async Task<StorefrontRuntimeConfig> LoadAsync(
        IStoreStorefrontApi storefront,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var runtime = new StorefrontRuntimeConfig();

        try
        {
            var response = await storefront.GetPublishedLayoutAsync(cancellationToken);
            if (!response.Status || response.Data == null)
                return runtime;

            var data = response.Data;
            runtime.LayoutVersion = data.LayoutVersion;

            runtime.Header = StorefrontFlagHelper.BuildFlagMap(
                data.Header?.Select(w => (w.ViewComponentName, w.IsActive)));

            runtime.Footer = StorefrontFlagHelper.BuildFlagMap(
                data.Footer?.Select(w => (w.ViewComponentName, w.IsActive)));

            if (data.Home != null)
                runtime.Home = data.Home;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Storefront published layout could not be loaded.");
        }

        return runtime;
    }
}
