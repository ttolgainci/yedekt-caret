using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductDetailSimilarViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(ProductSimilarModel data)
        {
            BaseResponse<List< ProductList >> routeResponse = new BaseResponse<List<ProductList>>();
            var returnData = new List<ProductList>();
            TokenResponse loginResponse = new TokenResponse();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var contentRequest = new ProductsByCageoryRequest { LanguageCode = loginResponse.LanguageCode,ID= data.ProductID,CategoryID=data.CategoryID /*Url = routeList[1]*/ };
                routeResponse = cms.GetProductDetailSimilar(contentRequest, loginResponse.Token);
            }
            if (routeResponse.Status)
            {
                returnData = routeResponse.Data;
                
                return Task.FromResult<IViewComponentResult>(View(returnData));
            }
            return Task.FromResult<IViewComponentResult>(Content(string.Empty));
        }
    }
}
