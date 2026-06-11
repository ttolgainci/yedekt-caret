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

        var layoutTask = storefront.GetPublishedLayoutAsync(cancellationToken);
        var generalTask = storefront.GetGeneralSettingsAsync(cancellationToken);

        try
        {
            await Task.WhenAll(layoutTask, generalTask);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Storefront runtime bootstrap failed.");
        }

        try
        {
            var response = await layoutTask;
            if (response.Status && response.Data != null)
            {
                var data = response.Data;
                runtime.LayoutVersion = data.LayoutVersion;

                runtime.Header = StorefrontFlagHelper.BuildFlagMap(
                    data.Header?.Select(w => (w.ViewComponentName, w.IsActive)));

                runtime.Footer = StorefrontFlagHelper.BuildFlagMap(
                    data.Footer?.Select(w => (w.ViewComponentName, w.IsActive)));

                if (data.Home != null)
                    runtime.Home = data.Home;

                runtime.SearchResultLayout = string.IsNullOrWhiteSpace(data.SearchResultLayout)
                    ? "grid4"
                    : data.SearchResultLayout.Trim();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Storefront published layout could not be loaded.");
        }

        try
        {
            var generalResponse = await generalTask;
            if (generalResponse.Status && generalResponse.Data != null)
                runtime.General = generalResponse.Data;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Storefront general settings could not be loaded.");
        }

        return runtime;
    }
}
