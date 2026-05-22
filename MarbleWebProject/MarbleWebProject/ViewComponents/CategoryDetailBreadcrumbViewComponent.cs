using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MarbleWebProject.ViewComponents
{

	[ViewComponent]
	public class CategoryDetailBreadcrumbViewComponent : ViewComponent
	{

		public async Task<IViewComponentResult> InvokeAsync(int categoryID)
		{
			BaseResponse<List< CategoryBreadcrumbModel >> routeResponse = new BaseResponse<List<CategoryBreadcrumbModel>>();
			var returnData = new List<CategoryBreadcrumbModel>();
			TokenResponse loginResponse = new TokenResponse();
			CategoryRequest request = new CategoryRequest();
			using (var cms = new CmsClient())
			{
				loginResponse = cms.getSession();
				var contentRequest = new CategoryBreadcrumbRequest { LanguageCode = loginResponse.LanguageCode, CategoryID= categoryID };
				routeResponse = cms.GetCategoryBreadcrumbAsync(contentRequest, loginResponse.Token);
			}
			if (routeResponse.Status)
			{
				returnData = routeResponse.Data;
				return View(returnData);
			}
			return Content(string.Empty);
		}
	}
}