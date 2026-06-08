using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.CheckoutDraft;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MarbleWebProject.Controllers;

public class CheckoutController : Controller
{
    private readonly IStoreStorefrontApi _storefront;
    private readonly IStoreCustomerSession _customerSession;
    private readonly IStoreCustomerAddressApi _customerAddresses;
    private readonly IStoreOrderApi _orders;
    private readonly IBasketUserIdProvider _basketUserId;
    private readonly IStoreApiClient _api;
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;
    private readonly IStoreShippingApi _shipping;
    private readonly IConfiguration _configuration;
    private readonly ICheckoutDraftStore _checkoutDraft;

    public CheckoutController(
        IStoreStorefrontApi storefront,
        IStoreCustomerSession customerSession,
        IStoreCustomerAddressApi customerAddresses,
        IStoreOrderApi orders,
        IBasketUserIdProvider basketUserId,
        IStoreApiClient api,
        IStoreBasketApi basket,
        IStoreAuthService auth,
        IStoreShippingApi shipping,
        IConfiguration configuration,
        ICheckoutDraftStore checkoutDraft)
    {
        _storefront = storefront;
        _customerSession = customerSession;
        _customerAddresses = customerAddresses;
        _orders = orders;
        _basketUserId = basketUserId;
        _api = api;
        _basket = basket;
        _auth = auth;
        _shipping = shipping;
        _configuration = configuration;
        _checkoutDraft = checkoutDraft;
    }

    [Route("checkout")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(basketUserId))
            return RedirectToAction("Index", "Cart");

        var session = await _auth.GetSessionAsync(cancellationToken);
        var model = await LoadBasketSetModelAsync(basketUserId, session.LanguageCode, cancellationToken);
        if (model.CartList.Count == 0)
            return RedirectToAction("Index", "Cart");

        await ApplyShippingQuoteAsync(model, cancellationToken);

        ViewBag.LanguageCode = session.LanguageCode ?? "tr";
        ViewBag.IsCustomerLoggedIn = _customerSession.IsLoggedIn();
        ViewBag.CarrierId = model.Info.CarrierId;
        ViewBag.CarrierName = model.Info.CarrierName;
        ViewBag.BankTransferEnabled = IsBankTransferEnabled();

        if (_customerSession.IsLoggedIn())
        {
            try
            {
                var profile = await _customerSession.GetAsync(cancellationToken);
                ViewBag.CustomerEmail = profile?.Customer?.Email;
                ViewBag.CustomerFirstName = profile?.Customer?.FirstName;
                ViewBag.CustomerLastName = profile?.Customer?.LastName;
                ViewBag.CustomerPhone = profile?.Customer?.Phone;
            }
            catch
            {
                // Profil opsiyonel.
            }
        }

        if (IsBankTransferEnabled())
        {
            try
            {
                var bankResp = await _api.GetAsync<BaseResponse<StoreBankTransferInfoModel>>(
                    "/api/store/checkout/bank-transfer-info", null, cancellationToken);
                if (bankResp.Status)
                    ViewBag.BankInfo = bankResp.Data;
            }
            catch
            {
                // Banka bilgisi opsiyonel.
            }
        }

        return View(model);
    }

    private bool IsBankTransferEnabled() =>
        _configuration.GetValue("Checkout:BankTransferEnabled", false);

    [HttpPost]
    [Route("checkout/shipping-options")]
    public async Task<IActionResult> ShippingOptions([FromBody] ShippingCalculateRequest? request, CancellationToken cancellationToken)
    {
        if (request?.CartItems == null || request.CartItems.Count == 0)
        {
            var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(basketUserId))
                return BadRequest(new { message = "Sepet boş." });

            var session = await _auth.GetSessionAsync(cancellationToken);
            var model = await LoadBasketSetModelAsync(basketUserId, session.LanguageCode ?? "tr", cancellationToken);
            request = new ShippingCalculateRequest
            {
                CityId = request?.CityId,
                TownId = request?.TownId,
                PostalCode = request?.PostalCode,
                CarrierId = request?.CarrierId,
                CartItems = model.CartList.Select(x => new ShippingCartItemRequest
                {
                    ProductId = x.ProductID,
                    Quantity = x.CartQuantity is < 1 ? 1 : x.CartQuantity!.Value
                }).ToList()
            };
        }

        var options = await _shipping.CalculateOptionsAsync(request, cancellationToken);
        if (options == null || options.Options.Count == 0)
            return NotFound(new { message = "Bu sepet için kargo seçeneği bulunamadı." });

        return Ok(options);
    }

    [HttpPost]
    [Route("checkout/place-order")]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutPlaceOrderForm form, CancellationToken cancellationToken)
    {
        if (form == null)
            return BadRequest(new { message = "Geçersiz sipariş isteği." });

        if (!form.CarrierId.HasValue || form.CarrierId.Value <= 0)
            return BadRequest(new { message = "Lütfen kargo firması seçin." });

        var session = await _auth.GetSessionAsync(cancellationToken);
        var languageCode = !string.IsNullOrWhiteSpace(form.LanguageCode)
            ? form.LanguageCode.Trim()
            : (session.LanguageCode ?? "tr");

        var paymentMethod = string.IsNullOrWhiteSpace(form.PaymentMethod) ? "CashOnDelivery" : form.PaymentMethod.Trim();
        if (string.Equals(paymentMethod, "CreditCard", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Kredi kartı ödemesi henüz aktif değil." });

        if (string.Equals(paymentMethod, "BankTransfer", StringComparison.OrdinalIgnoreCase))
        {
            if (!IsBankTransferEnabled())
                return BadRequest(new { message = "Şu an yalnızca kapıda ödeme ile sipariş verilebilir." });

            if (!form.BankAccountId.HasValue || form.BankAccountId.Value <= 0)
                return BadRequest(new { message = "Lütfen havale için banka hesabı seçin." });

            try
            {
                var bankResp = await _api.GetAsync<BaseResponse<StoreBankTransferInfoModel>>(
                    "/api/store/checkout/bank-transfer-info", null, cancellationToken);
                var activeIds = bankResp.Data?.Accounts?
                    .Where(x => x.IsActive)
                    .Select(x => x.ID)
                    .ToHashSet() ?? new HashSet<int>();
                if (!activeIds.Contains(form.BankAccountId.Value))
                    return BadRequest(new { message = "Seçilen banka hesabı geçersiz." });
            }
            catch
            {
                return BadRequest(new { message = "Banka hesapları yüklenemedi." });
            }
        }
        else if (!string.Equals(paymentMethod, "CashOnDelivery", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Şu an yalnızca kapıda ödeme ile sipariş verilebilir." });
        }

        var auth = await _customerSession.GetAsync(cancellationToken);
        var customerId = auth?.Customer?.Id;

        if (customerId is > 0)
        {
            var guestId = _basketUserId.GetGuestBasketUserId();
            if (!string.IsNullOrWhiteSpace(guestId) && !string.Equals(guestId, customerId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    await _orders.MergeCartAsync(new MergeCartForm
                    {
                        GuestUserId = guestId,
                        LanguageCode = languageCode
                    }, cancellationToken);
                }
                catch
                {
                    // Birleştirme başarısız olsa da sipariş denenir.
                }
            }
        }

        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(basketUserId))
            return BadRequest(new { message = "Sepet oturumu bulunamadı." });

        var key = _configuration["Checkout:GuestCheckoutKey"] ?? "";
        if (string.IsNullOrWhiteSpace(key))
            return StatusCode(500, new { message = "Checkout anahtarı yapılandırılmamış." });

        CustomerAddressModel? savedAddress = null;
        if (form.CustomerAddressId is > 0 && customerId is > 0)
        {
            try
            {
                var addrResult = await _customerAddresses.GetAsync(form.CustomerAddressId.Value, "tr", cancellationToken);
                if (addrResult.Status && addrResult.Data != null)
                    savedAddress = addrResult.Data;
            }
            catch
            {
                return BadRequest(new { message = "Seçilen adres bulunamadı." });
            }
        }

        var firstName = savedAddress?.ContactFirstName ?? form.FirstName;
        var lastName = savedAddress?.ContactLastName ?? form.LastName;
        var phone = savedAddress?.ContactPhone ?? form.Phone;
        var company = form.Company;
        var country = savedAddress?.CountryName ?? form.Country;
        var street = savedAddress?.AddressLine1 ?? form.Street;
        var street2 = savedAddress?.AddressLine2 ?? form.Street2;
        var city = savedAddress?.CityName ?? form.City;
        var state = savedAddress?.TownName ?? form.State;
        var postcode = savedAddress?.PostalCode ?? form.Postcode;
        var shippingCityId = savedAddress?.CityId.HasValue == true ? (int?)savedAddress.CityId.Value : form.ShippingCityId;
        var shippingTownId = savedAddress?.TownId.HasValue == true ? (int?)savedAddress.TownId.Value : form.ShippingTownId;

        var shipping = JsonSerializer.Serialize(new
        {
            FirstName = firstName,
            LastName = lastName,
            Company = company,
            Country = country,
            Street = street,
            Street2 = street2,
            City = city,
            State = state,
            Postcode = postcode,
            Phone = phone,
            Email = form.Email,
            CustomerAddressId = form.CustomerAddressId
        });

        CustomerAddressModel? billingSavedAddress = null;
        if (!form.BillingSameAsShipping && form.BillingCustomerAddressId is > 0 && customerId is > 0)
        {
            try
            {
                var billingResult = await _customerAddresses.GetAsync(form.BillingCustomerAddressId.Value, "tr", cancellationToken);
                if (billingResult.Status && billingResult.Data != null)
                    billingSavedAddress = billingResult.Data;
            }
            catch
            {
                return BadRequest(new { message = "Seçilen fatura adresi bulunamadı." });
            }
        }

        var billingFirstName = billingSavedAddress?.ContactFirstName ?? form.BillingFirstName ?? firstName;
        var billingLastName = billingSavedAddress?.ContactLastName ?? form.BillingLastName ?? lastName;
        var billingPhone = billingSavedAddress?.ContactPhone ?? form.BillingPhone ?? phone;
        var billingCountry = billingSavedAddress?.CountryName ?? form.BillingCountry ?? country;
        var billingStreet = billingSavedAddress?.AddressLine1 ?? form.BillingStreet ?? street;
        var billingStreet2 = billingSavedAddress?.AddressLine2 ?? form.BillingStreet2 ?? street2;
        var billingCity = billingSavedAddress?.CityName ?? form.BillingCity ?? city;
        var billingState = billingSavedAddress?.TownName ?? form.BillingState ?? state;
        var billingPostcode = billingSavedAddress?.PostalCode ?? form.BillingPostcode ?? postcode;
        var billingAddressId = billingSavedAddress != null ? form.BillingCustomerAddressId : form.BillingCustomerAddressId;

        var isCorporate = string.Equals(form.InvoiceType, "Corporate", StringComparison.OrdinalIgnoreCase);
        var billingCompany = isCorporate ? (form.CompanyName ?? form.Company) : company;
        var billingJson = form.BillingSameAsShipping
            ? shipping
            : JsonSerializer.Serialize(new
            {
                FirstName = billingFirstName,
                LastName = billingLastName,
                Company = billingCompany,
                TaxOffice = form.TaxOffice,
                TaxNumber = form.TaxNumber,
                Country = billingCountry,
                Street = billingStreet,
                Street2 = billingStreet2,
                City = billingCity,
                State = billingState,
                Postcode = billingPostcode,
                Phone = billingPhone,
                Email = form.Email,
                CustomerAddressId = billingAddressId
            });

        var request = new PlaceGuestOrderApiRequest
        {
            GuestUserId = basketUserId,
            Order = new PlaceOrderApiRequest
            {
                LanguageCode = languageCode,
                ShippingAddressJson = shipping,
                BillingAddressJson = billingJson,
                CouponCode = form.CouponCode,
                ShippingMethod = "Standard",
                CarrierID = form.CarrierId,
                ShippingCityID = shippingCityId,
                ShippingTownID = shippingTownId,
                PostalCode = postcode,
                CustomerId = customerId,
                PaymentMethod = paymentMethod,
                BankAccountId = string.Equals(paymentMethod, "BankTransfer", StringComparison.OrdinalIgnoreCase)
                    ? form.BankAccountId
                    : null
            }
        };

        try
        {
            var result = await _storefront.PlaceGuestOrderAsync(request, key, cancellationToken);
            if (!result.Status)
                return BadRequest(new { message = result.ErrorMessage ?? "Sipariş oluşturulamadı." });

            await _checkoutDraft.DeleteAsync(basketUserId, cancellationToken);

            StoreBankTransferInfoModel? bankInfo = null;
            try
            {
                var bankResp = await _api.GetAsync<BaseResponse<StoreBankTransferInfoModel>>(
                    "/api/store/checkout/bank-transfer-info", null, cancellationToken);
                if (bankResp.Status)
                    bankInfo = bankResp.Data;
            }
            catch { /* optional */ }

            var confirmation = new CheckoutConfirmationModel
            {
                OrderId = result.Data?.OrderId ?? 0,
                OrderNumber = result.Data?.OrderNumber,
                GrandTotal = result.Data?.GrandTotal ?? 0,
                CurrencyCode = result.Data?.CurrencyCode ?? "",
                CurrencySymbol = result.Data?.CurrencySymbol ?? "",
                PaymentMethod = paymentMethod,
                CustomerEmail = form.Email,
                PaymentDueAt = result.Data?.PaymentDueAt,
                BankAccountId = result.Data?.BankAccountId,
                BankName = result.Data?.BankName,
                BankAccountHolder = result.Data?.BankAccountHolder,
                BankIban = result.Data?.BankIban,
                BankInstructions = bankInfo?.Instructions
            };
            if (HttpContext.Session != null)
                CheckoutConfirmationSession.Save(HttpContext.Session, confirmation);

            return Ok(new
            {
                orderId = confirmation.OrderId,
                orderNumber = confirmation.OrderNumber,
                grandTotal = confirmation.GrandTotal,
                currencyCode = confirmation.CurrencyCode,
                paymentMethod = confirmation.PaymentMethod
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("checkout/draft")]
    public async Task<IActionResult> GetDraft(CancellationToken cancellationToken)
    {
        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(basketUserId))
            return Ok(null);

        var draft = await _checkoutDraft.GetAsync(basketUserId, cancellationToken);
        return Ok(draft);
    }

    [HttpPut]
    [Route("checkout/draft")]
    public async Task<IActionResult> SaveDraft([FromBody] CheckoutDraftModel? draft, CancellationToken cancellationToken)
    {
        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(basketUserId))
            return BadRequest(new { message = "Sepet oturumu bulunamadı." });

        if (draft == null)
            return BadRequest(new { message = "Taslak verisi boş." });

        await _checkoutDraft.SaveAsync(basketUserId, draft, cancellationToken);
        return Ok();
    }

    [HttpDelete]
    [Route("checkout/draft")]
    public async Task<IActionResult> DeleteDraft(CancellationToken cancellationToken)
    {
        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(basketUserId))
            await _checkoutDraft.DeleteAsync(basketUserId, cancellationToken);

        return Ok();
    }

    [HttpPost]
    [Route("checkout/payment-receipt/{orderId:int}")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> UploadPaymentReceipt(int orderId, IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null)
            return BadRequest(new { message = "Dosya gerekli." });

        var snapshot = HttpContext.Session != null
            ? CheckoutConfirmationSession.Load(HttpContext.Session)
            : null;
        if (snapshot == null || snapshot.OrderId != orderId)
            return BadRequest(new { message = "Geçersiz sipariş oturumu." });

        var saved = await PaymentReceiptUploadHelper.SaveAsync(
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>(),
            _configuration,
            orderId,
            file,
            cancellationToken);
        if (!saved.Ok)
            return BadRequest(new { message = saved.Error ?? "Yükleme başarısız." });

        var basketUserId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var key = _configuration["Checkout:GuestCheckoutKey"] ?? "";
        BaseResponse<ShopOrderDetailModel>? apiResult = null;

        try
        {
            if (_customerSession.IsLoggedIn())
            {
                var token = await _customerSession.GetTokenAsync(cancellationToken);
                apiResult = await _api.PutAsync<BaseResponse<ShopOrderDetailModel>>(
                    $"/api/orders/{orderId}/payment-receipt",
                    new RegisterPaymentReceiptApiRequest { ReceiptPath = saved.Path! },
                    token,
                    cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(basketUserId) && !string.IsNullOrWhiteSpace(key))
            {
                apiResult = await _api.PutAsync<BaseResponse<ShopOrderDetailModel>>(
                    $"/api/orders/guest/{orderId}/payment-receipt",
                    new GuestPaymentReceiptApiRequest { GuestUserId = basketUserId, ReceiptPath = saved.Path! },
                    bearerToken: null,
                    cancellationToken,
                    extraHeaders: new Dictionary<string, string> { ["X-Checkout-Key"] = key });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }

        if (apiResult == null || !apiResult.Status)
            return BadRequest(new { message = apiResult?.ErrorMessage ?? "Dekont kaydedilemedi." });

        return Ok(new
        {
            receiptUrl = MediaUrlHelper.Build(saved.Path),
            bankTransfer = apiResult.Data?.BankTransfer
        });
    }

    [HttpGet]
    [Route("checkout/confirmation/{orderId:int}")]
    public async Task<IActionResult> Confirmation(int orderId, CancellationToken cancellationToken)
    {
        var snapshot = HttpContext.Session != null
            ? CheckoutConfirmationSession.Load(HttpContext.Session)
            : null;

        if (snapshot == null || snapshot.OrderId != orderId)
        {
            snapshot = new CheckoutConfirmationModel
            {
                OrderId = orderId,
                OrderNumber = Request.Query["orderNumber"].ToString(),
                PaymentMethod = Request.Query["payment"].ToString()
            };
        }

        ShopOrderDetailModel? order = null;
        if (_customerSession.IsLoggedIn())
        {
            try
            {
                var detail = await _orders.GetOrderDetailAsync(orderId, cancellationToken);
                if (detail.Status && detail.Data != null)
                    order = detail.Data;
            }
            catch
            {
                // Misafir veya erişim yok.
            }
        }

        StoreBankTransferInfoModel? bankInfo = null;
        try
        {
            var bankResp = await _api.GetAsync<BaseResponse<StoreBankTransferInfoModel>>(
                "/api/store/checkout/bank-transfer-info", null, cancellationToken);
            if (bankResp.Status)
                bankInfo = bankResp.Data;
        }
        catch { /* optional */ }

        if (order != null)
        {
            snapshot.OrderNumber = order.OrderNumber ?? snapshot.OrderNumber;
            snapshot.GrandTotal = order.GrandTotal;
            snapshot.CurrencyCode = order.CurrencyCode;
            snapshot.CurrencySymbol = order.CurrencySymbol;
            snapshot.PaymentMethod = order.PaymentMethod ?? snapshot.PaymentMethod;
            snapshot.PaymentDueAt = order.BankTransfer?.PaymentDueAt ?? snapshot.PaymentDueAt;
            snapshot.BankAccountId = order.BankTransfer?.BankAccountId ?? snapshot.BankAccountId;
            snapshot.BankName = order.BankTransfer?.BankName ?? snapshot.BankName;
            snapshot.BankAccountHolder = order.BankTransfer?.AccountHolder ?? snapshot.BankAccountHolder;
            snapshot.BankIban = order.BankTransfer?.Iban ?? snapshot.BankIban;
        }

        snapshot.BankInstructions ??= bankInfo?.Instructions;

        ViewBag.Confirmation = snapshot;
        ViewBag.Order = order;
        ViewBag.BankInfo = bankInfo;
        ViewBag.IsCustomerLoggedIn = _customerSession.IsLoggedIn();
        return View();
    }

    private async Task<BasketSetModel> LoadBasketSetModelAsync(string userGuid, string languageCode, CancellationToken cancellationToken)
    {
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid, LanguageCode = languageCode }, cancellationToken);
        return routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Data)
            : new BasketSetModel();
    }

    private async Task ApplyShippingQuoteAsync(BasketSetModel model, CancellationToken cancellationToken)
    {
        if (model.CartList.Count == 0)
        {
            model.Info.ShippingPrice = null;
            model.Info.CarrierName = null;
            model.Info.TotalDesi = null;
            model.Info.CarrierId = null;
            model.Info.Subtotal = string.Empty;
            model.Info.GrandTotal = string.Empty;
            model.Info.Total = string.Empty;
            model.Info.CurrencyName = string.Empty;
            return;
        }

        var subtotal = model.CartList.Sum(c => (c.Price ?? 0) * (c.CartQuantity ?? 0));
        var currency = model.CartList.FirstOrDefault()?.CurrencyName ?? string.Empty;
        model.Info.CurrencyName = currency;
        model.Info.Subtotal = subtotal.ToString("N2") + " " + currency;

        var quote = await _shipping.CalculateAsync(new ShippingCalculateRequest
        {
            CartItems = model.CartList.Select(x => new ShippingCartItemRequest
            {
                ProductId = x.ProductID,
                Quantity = x.CartQuantity is < 1 ? 1 : x.CartQuantity!.Value
            }).ToList()
        }, cancellationToken);

        if (quote == null)
        {
            model.Info.GrandTotal = model.Info.Subtotal;
            model.Info.Total = model.Info.Subtotal;
            return;
        }

        model.Info.ShippingPrice = quote.ShippingPrice;
        model.Info.CarrierName = quote.CarrierName;
        model.Info.TotalDesi = quote.TotalDesi;
        model.Info.CarrierId = quote.CarrierId;

        var grandTotal = subtotal + quote.ShippingPrice;
        model.Info.GrandTotal = grandTotal.ToString("N2") + " " + currency;
        model.Info.Total = model.Info.GrandTotal;
    }
}

public sealed class CheckoutPlaceOrderForm
{
    public int? CustomerAddressId { get; set; }
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
    public int? CarrierId { get; set; }
    public int? ShippingCityId { get; set; }
    public int? ShippingTownId { get; set; }
    public string? PaymentMethod { get; set; }
    public int? BankAccountId { get; set; }
    public bool BillingSameAsShipping { get; set; } = true;
    public int? BillingCustomerAddressId { get; set; }
    public string? BillingFirstName { get; set; }
    public string? BillingLastName { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingStreet2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostcode { get; set; }
    public string? BillingPhone { get; set; }
    public string? OrderNote { get; set; }
    public string? InvoiceType { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? CompanyName { get; set; }
    public bool IsEInvoice { get; set; }
}

internal static class CheckoutConfirmationSession
{
    public const string Key = "CheckoutConfirmation";

    public static void Save(ISession session, CheckoutConfirmationModel model) =>
        session.SetString(Key, JsonSerializer.Serialize(model));

    public static CheckoutConfirmationModel? Load(ISession session)
    {
        var json = session.GetString(Key);
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<CheckoutConfirmationModel>(json);
    }
}
