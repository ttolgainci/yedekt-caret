using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

/// <summary>Header arama — tarayıcıdan aynı origin (CORS yok).</summary>
[Route("catalog-search")]
public class CatalogSearchController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public CatalogSearchController(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest(
        [FromQuery] string? q,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var result = await _catalog.SuggestAsync(q ?? "", session.LanguageCode, limit, cancellationToken);
        return Json(result);
    }
}
