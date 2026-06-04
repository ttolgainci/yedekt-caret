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

    public VehicleCatalogController(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
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

    [HttpGet("engines/{engineId:int}/categories")]
    public async Task<IActionResult> Categories(int engineId, CancellationToken cancellationToken)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var result = await _catalog.GetCategoriesByVehicleEngineAsync(
            engineId, session.LanguageCode, cancellationToken: cancellationToken);
        return Json(result);
    }
}
