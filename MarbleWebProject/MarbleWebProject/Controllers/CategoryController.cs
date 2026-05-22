using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class CategoryController : Controller
    {

        public IActionResult Index(int id,int pageNumber = 1, int pageSize = 12)
        {
            var route = Request.Path.Value ?? string.Empty;
            BaseResponse<ProductsByCageoryResponse> routeResponse = new BaseResponse<ProductsByCageoryResponse>();
            var returnData = new ProductsByCageoryResponse();
            TokenResponse loginResponse = new TokenResponse();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var contentRequest = new ProductsByCageoryRequest { pageNumber = pageNumber, pageSize = pageSize, LanguageCode = loginResponse.LanguageCode,ID=id /*Url = route.TrimEnd('/').Split('/').Last()*/ };
                routeResponse = cms.GetProductByCategory(contentRequest, loginResponse.Token);
            }
            if (routeResponse.Status)
            {
                returnData = routeResponse.Data;
                TempData["catUrl"] = route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
                TempData["Title"] = routeResponse.Data.MetaTitle;
                TempData["Keywords"] = routeResponse.Data.MetaKeyword;
                TempData["Description"] = routeResponse.Data.MetaDesc;
                return View(returnData);
            }
            return RedirectToAction("PageNotFound", "Error");
        }
        public IActionResult GetProducts(int pageNumber = 1, int pageSize = 12)
        {
            var url = TempData["catUrl"];
            var routeResponse = new BaseResponse<ProductsByCageoryResponse>();
            var returnData = new ProductsByCageoryResponse();
            TokenResponse loginResponse;

            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var contentRequest = new ProductsByCageoryRequest
                {
                    pageNumber = pageNumber,
                    pageSize = pageSize,
                    LanguageCode = loginResponse.LanguageCode,
                    //Url = url.ToString()
                };

                routeResponse = cms.GetProductByCategory(contentRequest, loginResponse.Token);
            }

            if (routeResponse.Status)
            {
                returnData = routeResponse.Data;

                returnData = routeResponse.Data;
                TempData["catUrl"] = url;
                TempData["Title"] = routeResponse.Data.MetaTitle;
                TempData["Keywords"] = routeResponse.Data.MetaKeyword;
                TempData["Description"] = routeResponse.Data.MetaDesc;
                return PartialView("_CategoryProductListPartial", returnData);
            }

            return NotFound();
        }
    }
}
