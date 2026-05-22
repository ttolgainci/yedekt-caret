using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MarbleWebProject.Controllers
{
    public class Cart : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public Cart(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public IActionResult Index()
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
                routeResponse = cms.GetBasketAll(request,loginResponse.Token);
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
            return View(rtn);
 
        }
        //     [HttpPost]
        //     public IActionResult AddToCart(int ProductID,string Url, int? CartQuantity)
        //     {
        //         HttpContextSessionHelper sessionHelper = new HttpContextSessionHelper(_contextAccessor.HttpContext);

        //         var cartSession = sessionHelper.Get<CartFullModel>("Cart");
        //         if (cartSession==null)
        //         {
        //             cartSession = new CartFullModel();
        //         }
        //         string currency = "";
        //var hasProduct = cartSession.CartList.FirstOrDefault(c => c.ProductID == ProductID);
        //if (hasProduct == null)
        //{

        //	BaseResponse<ProductCartInfo> routeResponse = new BaseResponse<ProductCartInfo>();
        //	TokenResponse loginResponse = new TokenResponse();
        //	using (var cms = new CmsClient())
        //	{
        //		loginResponse = cms.getSession();
        //		var contentRequest = new ProductCartRequest { LanguageCode = loginResponse.LanguageCode, ProductID = ProductID };
        //		routeResponse = cms.GetProductForCart(contentRequest, loginResponse.Token);
        //	}
        //	if (routeResponse.Status)
        //	{
        //		currency = routeResponse.Data.CurrencyName;
        //		cartSession.CartList.Add(new CartModel
        //		{
        //			ProductID = routeResponse.Data.ProductID,
        //			CartQuantity = CartQuantity,
        //			CurrencyName = routeResponse.Data.CurrencyName,
        //			MainImage = AppConfig.CDNServices.ContentUploads+ routeResponse.Data.MainImage,
        //			Price = routeResponse.Data.Price,
        //			ProductName = routeResponse.Data.ProductName,
        //			Url = Url
        //		});
        //	}
        //}

        //else {
        //             currency = hasProduct.CurrencyName;
        //	hasProduct.CartQuantity = CartQuantity; 
        //         }







        //         #region setProduct


        //         #endregion
        //         #region Ste Info
        //         var getTotal= cartSession.CartList.Sum(c => c.Price * c.CartQuantity);
        //         cartSession.Info.Total = "<span id='basketTotal'>"+ getTotal + "</span>"+ currency + "";     
        //         cartSession.Info.TotalQuantity = cartSession.CartList.Sum(c=>c.CartQuantity);

        //         #endregion
        //         sessionHelper.SetSession("Cart", Newtonsoft.Json.JsonConvert.SerializeObject(cartSession));
        //         return Json(new { CartList = cartSession.CartList,Info= cartSession.Info });
        //     }
        [HttpPost]
        public IActionResult AddToCart(int ProductID, string Url, int? CartQuantity, int? ProductVariantID)
        {

            BaseResponse<List< OrderBasket >> routeResponse = new BaseResponse<List<OrderBasket>>();
            TokenResponse loginResponse = new TokenResponse();
            BasketReturnModel basketReturnModel = new BasketReturnModel();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
                var contentRequest = new BasketRequest { LanguageCode = loginResponse.LanguageCode, ProductID = ProductID, ProductVariantID = ProductVariantID, CartQuantity= CartQuantity,Url=Url,UserID= userGuid };
                routeResponse = cms.GetByIDBasket(contentRequest, loginResponse.Token);
            }

            if (routeResponse.Status)
            {
                #region Set Info
                var getTotal = routeResponse.Data.Sum(c => (c.Price ?? 0) * (c.quantity ?? 0));
                var currency = routeResponse.Data.FirstOrDefault()?.Currency ?? string.Empty;
                basketReturnModel.TotalPrice = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
                basketReturnModel.TotalQuantity = routeResponse.Data.Sum(c => c.quantity ?? 0);
                #endregion
            }


            
            return Json(new { CartList = routeResponse.Data, Info = basketReturnModel });

        }
        public IActionResult DeleteFromCart(int ProductID, int? ProductVariantID)
        {
            BaseResponse<List<OrderBasket>> routeResponse = new BaseResponse<List<OrderBasket>>();
            TokenResponse loginResponse = new TokenResponse();
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
                var contentRequest = new DeleteBasketRequest
                {
                    LanguageCode = loginResponse.LanguageCode,
                    ProductID = ProductID,
                    ProductVariantID = ProductVariantID,
                    UserID = userGuid
                };
                routeResponse = cms.DeleteProductFromCart(contentRequest, loginResponse.Token);
            }

            return Json(new
            {
                Status = routeResponse.Status,
                Message = routeResponse.ErrorMessage
            });
        }
    }
}
