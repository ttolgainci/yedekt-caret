using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    /// <summary>
    /// SEO indeksinden ürün URL'lerini çözer; startup'ta binlerce ürün route'u kaydı gerekmez.
    /// </summary>
    public class CatalogDispatchController : Controller
    {
        public IActionResult Index()
        {
            var raw = (Request.Path.Value ?? string.Empty).TrimStart('/');
            if (string.IsNullOrEmpty(raw))
            {
                return NotFound();
            }

            if (LooksLikeStaticFileRequest(raw))
            {
                return NotFound();
            }

            TokenResponse loginResponse;
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
            }

            BaseResponse<ResolveSeoPathResponse> resolveResponse;
            using (var cms = new CmsClient())
            {
                resolveResponse = cms.ResolveSeoPath(
                    new ResolveSeoPathRequest
                    {
                        LanguageCode = loginResponse.LanguageCode,
                        Path = raw
                    },
                    loginResponse.Token);
            }

            if (!resolveResponse.Status || resolveResponse.Data == null
                || resolveResponse.Data.PageType != "PRODUCT")
            {
                return NotFound();
            }

            var id = resolveResponse.Data.EntityId;
            var catID = resolveResponse.Data.CatId ?? 0;

            BaseResponse<ProductDetailResponse> routeResponse = new BaseResponse<ProductDetailResponse>();
            var returnData = new ProductDetailResponse();
            using (var cms = new CmsClient())
            {
                var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
                var contentRequest = new ProductDetailRequest
                {
                    Language = loginResponse.LanguageCode,
                    ID = id,
                    UserID = userGuid
                };
                routeResponse = cms.GetProductDetail(contentRequest, loginResponse.Token);
                if (routeResponse.Status)
                {
                    cms.AttachProductCatalogExtras(routeResponse.Data, id, loginResponse.Token);
                }
            }

            if (!routeResponse.Status)
            {
                return NotFound();
            }

            var route = Request.Path.Value ?? string.Empty;
            routeResponse.Data.ProductSimilarData = new ProductSimilarModel { CategoryID = catID, ProductID = id };
            routeResponse.Data.ProductDetails.Url = route;
            returnData = routeResponse.Data;
            TempData["Title"] = routeResponse.Data.ProductDetails.MetaTitle;
            TempData["Keywords"] = routeResponse.Data.ProductDetails.MetaKeyword;
            TempData["Description"] = routeResponse.Data.ProductDetails.MetaDescription;
            return View("~/Views/ProductDetail/Index.cshtml", returnData);
        }

        private static bool LooksLikeStaticFileRequest(string path)
        {
            var last = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? path;
            return last.Contains('.', StringComparison.Ordinal) && !last.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        }
    }
}
