using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class StaticPageController : Controller
{
    private readonly IStoreContentApi _content;
    private readonly IStoreAuthService _auth;

    public StaticPageController(IStoreContentApi content, IStoreAuthService auth)
    {
        _content = content;
        _auth = auth;
    }

    public async Task<IActionResult> Index(string id, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var contentRequest = new InfoPageRequest
        {
            Type = "INFORMATION",
            LanguageCode = session.LanguageCode,
            Url = id
        };
        var routeResponse = await _content.GetInfoByUrlAsync(contentRequest, cancellationToken);

        if (!routeResponse.Status)
            return NotFound();

        TempData["Title"] = routeResponse.Data.MetaTitle;
        TempData["Keywords"] = routeResponse.Data.MetaKeyword;
        TempData["Description"] = routeResponse.Data.MetaDescription;
        return View(routeResponse.Data);
    }
}
