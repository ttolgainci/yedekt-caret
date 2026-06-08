using MarbleWebProject.Helpers;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

/// <summary>Storefront araç kataloğu — tarayıcıdan aynı origin (CORS yok).</summary>
[Route("vehicle-catalog")]
public class VehicleCatalogController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly IProductSearchUrlResolver _urlResolver;

    public VehicleCatalogController(
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        IProductSearchUrlResolver urlResolver)
    {
        _catalog = catalog;
        _auth = auth;
        _urlResolver = urlResolver;
    }

    [HttpGet("makes/{makeId:int}/models")]
    public async Task<IActionResult> Models(int makeId, CancellationToken cancellationToken)
    {
        var result = await _catalog.GetVehicleModelsByMakeAsync(makeId, cancellationToken);
        return Json(result);
    }

    [HttpGet("models/{modelId:int}/generations")]
    public async Task<IActionResult> Generations(int modelId, CancellationToken cancellationToken)
    {
        var result = await _catalog.GetVehicleGenerationsByModelAsync(modelId, cancellationToken);
        return Json(result);
    }

    [HttpGet("generations/{generationId:int}/engines")]
    public async Task<IActionResult> Engines(int generationId, CancellationToken cancellationToken)
    {
        var result = await _catalog.GetVehicleEnginesByGenerationAsync(generationId, cancellationToken);
        return Json(result);
    }

    [HttpGet("search-url")]
    public async Task<IActionResult> SearchUrl(
        int makeId,
        int modelId,
        int generationId,
        int engineId,
        CancellationToken cancellationToken)
    {
        var path = await _urlResolver.BuildVehiclePathAsync(
            makeId, modelId, generationId, engineId, cancellationToken);
        if (path == null)
            return NotFound(new { status = false, message = "Vehicle not found." });

        var url = UrlSlugHelper.BuildVehicleSearchPath(
            path.MakeSlug,
            path.ModelSlug,
            path.GenerationSlug,
            path.EngineSlug,
            path.EngineId);

        return Json(new { status = true, url });
    }

    [HttpGet("engines/{engineId:int}/categories")]
    public async Task<IActionResult> Categories(int engineId, CancellationToken cancellationToken)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var result = await _catalog.GetCategoriesByVehicleEngineAsync(
            engineId, session.LanguageCode, cancellationToken: cancellationToken);
        return Json(result);
    }
}
