using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MarbleWebProject.Controllers;

public class CheckoutController : Controller
{
    private readonly IStoreStorefrontApi _storefront;
    private readonly IConfiguration _configuration;

    public CheckoutController(IStoreStorefrontApi storefront, IConfiguration configuration)
    {
        _storefront = storefront;
        _configuration = configuration;
    }

    public IActionResult Index() => View();

    [HttpPost]
    [Route("checkout/place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutPlaceOrderForm form, CancellationToken cancellationToken)
    {
        var guestId = Request.Cookies["UserIDForBasket"];
        if (string.IsNullOrWhiteSpace(guestId))
            return BadRequest(new { message = "Sepet oturumu bulunamadı." });

        var key = _configuration["Checkout:GuestCheckoutKey"] ?? "";
        if (string.IsNullOrWhiteSpace(key))
            return StatusCode(500, new { message = "Checkout anahtarı yapılandırılmamış." });

        var shipping = JsonSerializer.Serialize(new
        {
            form.FirstName,
            form.LastName,
            form.Company,
            form.Country,
            form.Street,
            form.Street2,
            form.City,
            form.State,
            form.Postcode,
            form.Phone,
            form.Email
        });

        var request = new PlaceGuestOrderApiRequest
        {
            GuestUserId = guestId,
            Order = new PlaceOrderApiRequest
            {
                LanguageCode = form.LanguageCode ?? "tr",
                ShippingAddressJson = shipping,
                BillingAddressJson = shipping,
                CouponCode = form.CouponCode,
                ShippingMethod = "Standard"
            }
        };

        try
        {
            var result = await _storefront.PlaceGuestOrderAsync(request, key, cancellationToken);
            if (!result.Status)
                return BadRequest(new { message = result.ErrorMessage ?? "Sipariş oluşturulamadı." });

            return Ok(new
            {
                orderId = result.Data?.OrderId,
                grandTotal = result.Data?.GrandTotal,
                currencyCode = result.Data?.CurrencyCode
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}

public sealed class CheckoutPlaceOrderForm
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Company { get; set; }
    public string? Country { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? CouponCode { get; set; }
    public string? LanguageCode { get; set; }
}
