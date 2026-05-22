using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class StaticPageController : Controller
    {
        public IActionResult Index(string id)
        {
       
            BaseResponse<AllInfoResponse> routeResponse = new BaseResponse<AllInfoResponse>();
            var returnData = new AllInfoResponse();
            TokenResponse loginResponse = new TokenResponse();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var contentRequest = new InfoPageRequest {Type= "FAQ", LanguageCode = loginResponse.LanguageCode, Url = id };
                routeResponse = cms.GetInfoByUrl(contentRequest, loginResponse.Token);
            }
            if (routeResponse.Status)
            {
               
                returnData = routeResponse.Data;
                TempData["Title"] = routeResponse.Data.MetaTitle;
                TempData["Keywords"] = routeResponse.Data.MetaKeyword;
                TempData["Description"] = routeResponse.Data.MetaDescription;
                return View(returnData);
            }
            return NotFound();
        }
    }
}
