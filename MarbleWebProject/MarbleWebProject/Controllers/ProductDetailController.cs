using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class ProductDetailController : Controller
    {
        public IActionResult Index(int id,int catID)
        {
            var route = Request.Path.Value ?? string.Empty;
            var routeList = route.Split("/", StringSplitOptions.RemoveEmptyEntries);
            BaseResponse<ProductDetailResponse> routeResponse = new BaseResponse<ProductDetailResponse>();
            var returnData = new ProductDetailResponse();
            if (routeList.Length >= 2)
            {               
                TokenResponse loginResponse = new TokenResponse();
                using (var cms = new CmsClient())
                {
                    loginResponse = cms.getSession();
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
                if (routeResponse.Status)
                {
                    routeResponse.Data.ProductSimilarData = new ProductSimilarModel { CategoryID = catID, ProductID = id };
                    routeResponse.Data.ProductDetails.Url = route;
                    returnData = routeResponse.Data;
                    TempData["Title"] = routeResponse.Data.ProductDetails.MetaTitle;
                    TempData["Keywords"] = routeResponse.Data.ProductDetails.MetaKeyword;
                    TempData["Description"] = routeResponse.Data.ProductDetails.MetaDescription;
                    return View(returnData);
                }
            }
            return NotFound();
        }
    }
}
