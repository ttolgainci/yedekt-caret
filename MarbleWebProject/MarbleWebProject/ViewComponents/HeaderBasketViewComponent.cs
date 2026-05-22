using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HeaderBasketViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync()
        {
            BasketSetModel rtn = new BasketSetModel();
            BaseResponse<List<OrderBasket>> routeResponse = new BaseResponse<List<OrderBasket>>();
            TokenResponse loginResponse = new TokenResponse();
            var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var request = new BasketAllRequest
                {
                    UserID = userGuid,
                    LanguageCode = loginResponse.LanguageCode
                };
                routeResponse = cms.GetBasketAll(request, loginResponse.Token);
            }
            if (routeResponse.Status)
            {
                foreach (var item in routeResponse.Data)
                {
                    rtn.CartList.Add(new CartModel
                    {
                        CartQuantity = item.quantity,
                        CurrencyName = item.Currency,
                        MainImage = item.Image,
                        Price = item.Price,
                        ProductID = item.ProductID,
                        ProductName = item.Name,
                        Url = item.Url,
                        ProductVariantID = item.ProductVariantID
                    });
                }
                var getTotal = routeResponse.Data.Sum(c => (c.Price ?? 0) * (c.quantity ?? 0));
                var currency = routeResponse.Data.FirstOrDefault()?.Currency ?? string.Empty;
                rtn.Info.Total = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
                rtn.Info.TotalQuantity = routeResponse.Data.Sum(c => c.quantity ?? 0);

            }
            return Task.FromResult<IViewComponentResult>(View(rtn));

        }
    }
}
