using System.Linq;
using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class CheckoutController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var vm = LoadCheckoutViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Complete()
        {
            var vm = LoadCheckoutViewModel();

            if (string.IsNullOrWhiteSpace(AppConfig.StorefrontGuestCheckoutKey))
            {
                vm.AlertMessage = "Misafir sipariş kapalı: API ve Web appsettings içinde aynı gizli anahtarı ayarlayın (Orders:GuestCheckoutKey / StorefrontApi:GuestCheckoutKey).";
                vm.AlertSuccess = false;
                return View("Index", vm);
            }

            var guestId = Request.Cookies["UserIDForBasket"];
            if (string.IsNullOrWhiteSpace(guestId))
            {
                vm.AlertMessage = "Sepet tanımlanamadı.";
                vm.AlertSuccess = false;
                return View("Index", vm);
            }

            TokenResponse loginResponse;
            using (var cms = new CmsClient())
            {
                loginResponse = cms.getSession();
                var request = new PlaceGuestOrderRequestDto
                {
                    GuestUserId = guestId.Trim(),
                    Order = new PlaceOrderRequestDto
                    {
                        LanguageCode = loginResponse.LanguageCode ?? string.Empty,
                        ShippingMethod = "Standard"
                    }
                };

                var result = cms.PlaceGuestOrder(request);
                if (result.Status && result.Data != null)
                {
                    vm.AlertMessage = $"Sipariş alındı. No: {result.Data.OrderId}";
                    vm.AlertSuccess = true;
                    vm.LastOrderId = result.Data.OrderId;
                    vm.LastGrandTotal = result.Data.GrandTotal;
                    vm.LastCurrencyCode = result.Data.CurrencyCode;
                    vm.Lines.Clear();
                    vm.TotalQuantity = 0;
                    vm.TotalSummaryHtml = null;
                    return View("Index", vm);
                }

                vm.AlertMessage = string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Sipariş tamamlanamadı." : result.ErrorMessage;
                vm.AlertSuccess = false;
            }

            return View("Index", vm);
        }

        private CheckoutViewModel LoadCheckoutViewModel()
        {
            var vm = new CheckoutViewModel
            {
                GuestCheckoutConfigured = !string.IsNullOrWhiteSpace(AppConfig.StorefrontGuestCheckoutKey)
            };

            using (var cms = new CmsClient())
            {
                var loginResponse = cms.getSession();
                var guestId = Request.Cookies["UserIDForBasket"] ?? string.Empty;
                var request = new BasketAllRequest
                {
                    UserID = guestId,
                    LanguageCode = loginResponse.LanguageCode
                };
                var routeResponse = cms.GetBasketAll(request, loginResponse.Token);
                if (routeResponse.Status && routeResponse.Data != null && routeResponse.Data.Count > 0)
                {
                    foreach (var item in routeResponse.Data)
                    {
                        vm.Lines.Add(new CartModel
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
                    vm.TotalSummaryHtml = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
                    vm.TotalQuantity = routeResponse.Data.Sum(c => c.quantity ?? 0);
                }
            }

            return vm;
        }
    }
}
