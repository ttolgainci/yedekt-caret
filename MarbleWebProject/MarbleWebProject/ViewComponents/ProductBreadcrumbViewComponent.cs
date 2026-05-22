using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductBreadcrumbViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(string name)
        {
            var route = Request.Path.Value ?? string.Empty;
            var routeList = route.Split("/", StringSplitOptions.RemoveEmptyEntries);
            if (routeList.Length < 2)
            {
                return Task.FromResult<IViewComponentResult>(Content(string.Empty));
            }

            var categoryUrl = routeList[^2];
            BaseResponse<List<CategoryBreadcrumbModel>> routeResponse = new BaseResponse<List<CategoryBreadcrumbModel>>();
            var returnData = new ProductBreadcrumbModel();
            TokenResponse loginResponse = new TokenResponse();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var contentRequest = new ProductBreadcrumbRequest { LanguageCode = loginResponse.LanguageCode, CategoryUrl = categoryUrl };
                routeResponse = cms.GetProductBreadcrumbAsync(contentRequest, loginResponse.Token);
            }
            if (routeResponse.Status)
            {
                returnData.CagetoryList = routeResponse.Data;
                returnData.Name = name;
                return Task.FromResult<IViewComponentResult>(View(returnData));
            }
            return Task.FromResult<IViewComponentResult>(Content(string.Empty));
        }
    }
}
