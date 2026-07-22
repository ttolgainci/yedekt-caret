using System.Text.RegularExpressions;

using MarbleWebProject.Models;

using MarbleWebProject.Helpers;

using MarbleWebProject.Services;

using MarbleWebProject.Services.Api;

using Microsoft.AspNetCore.Mvc;



namespace MarbleWebProject.Controllers;



public class AccountController : Controller

{

    private readonly IStoreCustomerAuthApi _customerAuth;

    private readonly IStoreCustomerSession _customerSession;

    private readonly IStoreCustomerAddressApi _customerAddresses;

    private readonly IStoreLocationApi _locations;

    private readonly IStoreAuthService _storeAuth;

    private readonly IStoreOrderApi _orders;

    private readonly IStoreInvoiceApi _invoices;

    private readonly IStoreReturnApi _returns;

    private readonly IBasketUserIdProvider _basketUserId;

    private readonly IStoreApiClient _api;

    private readonly IConfiguration _configuration;



    public AccountController(

        IStoreCustomerAuthApi customerAuth,

        IStoreCustomerSession customerSession,

        IStoreCustomerAddressApi customerAddresses,

        IStoreLocationApi locations,

        IStoreAuthService storeAuth,

        IStoreOrderApi orders,

        IStoreInvoiceApi invoices,

        IStoreReturnApi returns,

        IBasketUserIdProvider basketUserId,

        IStoreApiClient api,

        IConfiguration configuration)

    {

        _customerAuth = customerAuth;

        _customerSession = customerSession;

        _customerAddresses = customerAddresses;

        _locations = locations;

        _storeAuth = storeAuth;

        _orders = orders;

        _invoices = invoices;

        _returns = returns;

        _basketUserId = basketUserId;

        _api = api;

        _configuration = configuration;

    }



    [HttpGet]

    [Route("account")]

    public IActionResult Index()

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        return RedirectToAction(nameof(Profile));

    }



    [HttpGet]

    [Route("account/profile")]

    public async Task<IActionResult> Profile(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("profile", cancellationToken);

        return View();

    }



    [HttpGet]

    [Route("account/profile/data")]

    public async Task<IActionResult> ProfileData(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        var token = await _customerSession.GetTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(token))

            return Unauthorized();

        try

        {

            var result = await _customerAuth.GetProfileAsync(token, cancellationToken);

            if (!result.Status || result.Data == null)

                return BadRequest(new { message = result.ErrorMessage ?? "Profil yüklenemedi." });

            return Ok(result.Data);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpPut]

    [Route("account/profile")]

    public async Task<IActionResult> UpdateProfile([FromBody] CustomerProfileUpdateForm? form, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        if (form == null)

            return BadRequest(new { message = "Geçersiz istek." });

        var token = await _customerSession.GetTokenAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(token))

            return Unauthorized();

        try

        {

            var result = await _customerAuth.UpdateProfileAsync(token, form, cancellationToken);

            if (!result.Status || result.Data == null)

                return BadRequest(new { message = result.ErrorMessage ?? "Profil güncellenemedi." });

            var auth = await _customerSession.GetAsync(cancellationToken);

            if (auth != null)

            {

                auth.Customer = result.Data;

                await _customerSession.SaveAsync(auth, cancellationToken);

            }

            return Ok(new

            {

                customer = result.Data,

                displayName = BuildDisplayName(result.Data)

            });

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpGet]

    [Route("account/password")]

    public async Task<IActionResult> Password(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("password", cancellationToken);

        return View();

    }



    [HttpGet]

    [Route("account/orders")]

    public async Task<IActionResult> Orders(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("orders", cancellationToken);

        return View();

    }



    [HttpGet]

    [Route("account/orders/list")]

    public async Task<IActionResult> OrderList(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        try

        {

            var result = await _orders.GetMyOrdersAsync(cancellationToken);

            if (!result.Status)

                return BadRequest(new { message = result.ErrorMessage ?? "Siparişler yüklenemedi." });

            var items = result.Data ?? new List<ShopOrderListItemModel>();
            EnrichOrderListMedia(items);
            return Ok(items);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpPost]
    [Route("account/orders/{id:int}/payment-receipt")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> UploadOrderPaymentReceipt(int id, IFormFile? file, CancellationToken cancellationToken)
    {
        if (!_customerSession.IsLoggedIn())
            return Unauthorized();

        if (file == null)
            return BadRequest(new { message = "Dosya gerekli." });

        var saved = await PaymentReceiptUploadHelper.SaveAsync(
            HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>(),
            _configuration,
            id,
            file,
            cancellationToken);
        if (!saved.Ok)
            return BadRequest(new { message = saved.Error ?? "Yükleme başarısız." });

        try
        {
            var token = await _customerSession.GetTokenAsync(cancellationToken);
            var apiResult = await _api.PutAsync<BaseResponse<ShopOrderDetailModel>>(
                $"/api/orders/{id}/payment-receipt",
                new RegisterPaymentReceiptApiRequest { ReceiptPath = saved.Path! },
                token,
                cancellationToken);
            if (!apiResult.Status)
                return BadRequest(new { message = apiResult.ErrorMessage ?? "Dekont kaydedilemedi." });

            return Ok(new
            {
                receiptUrl = MediaUrlHelper.Build(saved.Path),
                bankTransfer = apiResult.Data?.BankTransfer
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]

    [Route("account/orders/{id:int}")]

    public async Task<IActionResult> OrderDetail(int id, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        try

        {

            var result = await _orders.GetOrderDetailAsync(id, cancellationToken);

            if (!result.Status || result.Data == null)

                return NotFound(new { message = result.ErrorMessage ?? "Sipariş bulunamadı." });

            EnrichOrderDetailMedia(result.Data);
            return Ok(result.Data);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }

    [HttpGet]
    [Route("account/orders/{id:int}/shipment/track")]
    public async Task<IActionResult> TrackShipment(int id, bool refresh = false, CancellationToken cancellationToken = default)
    {
        if (!_customerSession.IsLoggedIn())
            return Unauthorized();

        try
        {
            var result = await _orders.TrackShipmentAsync(id, refresh, cancellationToken);
            if (!result.Status)
                return NotFound(new { message = result.ErrorMessage ?? "Takip bilgisi alınamadı." });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("account/returns")]
    public async Task<IActionResult> Returns(CancellationToken cancellationToken)
    {
        if (!_customerSession.IsLoggedIn())
            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("returns", cancellationToken);
        return View();
    }

    [HttpGet]
    [Route("account/returns/list")]
    public async Task<IActionResult> ReturnList(CancellationToken cancellationToken)
    {
        if (!_customerSession.IsLoggedIn())
            return Unauthorized();

        try
        {
            var result = await _returns.GetMyReturnsAsync(cancellationToken);
            if (!result.Status)
                return BadRequest(new { message = result.ErrorMessage ?? "İade talepleri yüklenemedi." });

            return Ok(result.Data ?? new List<ReturnRequestListItemModel>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]
    [Route("account/returns/{id:int}")]
    public async Task<IActionResult> ReturnDetail(int id, CancellationToken cancellationToken)
    {
        if (!_customerSession.IsLoggedIn())
            return Unauthorized();

        try
        {
            var result = await _returns.GetReturnDetailAsync(id, cancellationToken);
            if (!result.Status || result.Data == null)
                return NotFound(new { message = result.ErrorMessage ?? "İade talebi bulunamadı." });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    [Route("account/orders/{id:int}/returns")]
    public async Task<IActionResult> CreateReturn(int id, [FromBody] StoreReturnCreateForm? body, CancellationToken cancellationToken)
    {
        if (!_customerSession.IsLoggedIn())
            return Unauthorized();

        if (body == null || body.Lines == null || body.Lines.Count == 0)
            return BadRequest(new { message = "En az bir ürün satırı seçin." });

        try
        {
            var result = await _returns.CreateReturnAsync(id, body, cancellationToken);
            if (!result.Status)
                return BadRequest(new { message = result.ErrorMessage ?? "İade talebi oluşturulamadı." });

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet]

    [Route("account/invoices")]

    public async Task<IActionResult> Invoices(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("invoices", cancellationToken);

        return View();

    }



    [HttpGet]

    [Route("account/invoices/list")]

    public async Task<IActionResult> InvoiceList(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        try

        {

            var result = await _invoices.GetMyInvoicesAsync(cancellationToken);

            if (!result.Status)

                return BadRequest(new { message = result.ErrorMessage ?? "Faturalar yüklenemedi." });

            var items = result.Data ?? new List<CustomerInvoiceListItemModel>();
            EnrichInvoiceListPdfUrls(items);
            return Ok(items);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpGet]

    [Route("account/invoices/{id:int}")]

    public async Task<IActionResult> InvoiceDetail(int id, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();

        try

        {

            var result = await _invoices.GetInvoiceDetailAsync(id, cancellationToken);

            if (!result.Status || result.Data == null)

                return NotFound(new { message = result.ErrorMessage ?? "Fatura bulunamadı." });

            EnrichInvoicePdfUrl(result.Data);

            return Ok(result.Data);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpGet]

    [Route("account/invoices/{id:int}/preview")]

    public async Task<IActionResult> InvoicePreview(int id, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        var html = await _invoices.GetInvoicePreviewHtmlAsync(id, cancellationToken);

        if (string.IsNullOrWhiteSpace(html))

            return NotFound();

        html = FixInvoicePreviewLogo(html);

        return Content(html, "text/html; charset=utf-8");

    }



    [HttpGet]

    [Route("account/invoices/{id:int}/pdf")]

    public async Task<IActionResult> InvoicePdf(int id, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        var file = await _invoices.GetInvoicePdfAsync(id, cancellationToken);

        if (!file.Found || file.Bytes == null)

        {

            var message = string.IsNullOrWhiteSpace(file.ErrorMessage)

                ? "Fatura PDF henüz hazır değil. Kesim sonrası birkaç saniye bekleyip tekrar deneyin veya fatura önizlemesini kullanın."

                : file.ErrorMessage;

            return Content(

                "<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>PDF</title></head><body style=\"font-family:sans-serif;padding:24px\">" +

                "<h1>PDF bulunamadı</h1><p>" + System.Net.WebUtility.HtmlEncode(message) + "</p>" +

                "<p><a href=\"/account/invoices/" + id + "/preview\">Fatura önizlemesine git</a></p></body></html>",

                "text/html; charset=utf-8");

        }

        return File(file.Bytes, "application/pdf", file.FileName ?? $"invoice-{id}.pdf");

    }



    [HttpGet]

    [Route("account/addresses")]

    public async Task<IActionResult> Addresses(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return RedirectToAction("Index", "Home");

        await PrepareAccountViewAsync("addresses", cancellationToken);

        return View();

    }



    [HttpGet]

    [Route("account/addresses/list")]

    public async Task<IActionResult> AddressList(CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();



        try

        {

            var result = await _customerAddresses.ListAsync("tr", cancellationToken);

            if (!result.Status)

                return BadRequest(new { message = result.ErrorMessage ?? "Adresler yüklenemedi." });



            return Ok(result.Data ?? new List<CustomerAddressModel>());

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpPost]

    [Route("account/addresses/save")]

    public async Task<IActionResult> SaveAddress([FromBody] CustomerAddressForm form, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();



        if (form == null || string.IsNullOrWhiteSpace(form.AddressLine1) || form.CountryId <= 0)

            return BadRequest(new { message = "Ülke ve adres satırı zorunludur." });



        try

        {

            BaseResponse<CustomerAddressModel> result;

            if (form.Id is > 0)

                result = await _customerAddresses.UpdateAsync(form.Id.Value, form, cancellationToken);

            else

                result = await _customerAddresses.CreateAsync(form, cancellationToken);



            if (!result.Status || result.Data == null)

                return BadRequest(new { message = result.ErrorMessage ?? "Adres kaydedilemedi." });



            return Ok(result.Data);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpDelete]

    [Route("account/addresses/{id:int}")]

    public async Task<IActionResult> DeleteAddress(int id, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();



        try

        {

            var result = await _customerAddresses.DeleteAsync(id, cancellationToken);

            if (!result.Status)

                return BadRequest(new { message = result.ErrorMessage ?? "Adres silinemedi." });



            return Ok(new { ok = true });

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpPost]

    [Route("account/addresses/{id:int}/set-default")]

    public async Task<IActionResult> SetDefaultAddress(int id, [FromBody] SetDefaultAddressForm? form, CancellationToken cancellationToken)

    {

        if (!_customerSession.IsLoggedIn())

            return Unauthorized();



        if (form == null || string.IsNullOrWhiteSpace(form.Type))

            return BadRequest(new { message = "Adres tipi zorunludur." });



        try

        {

            var result = await _customerAddresses.SetDefaultAsync(id, form.Type, cancellationToken);

            if (!result.Status || result.Data == null)

                return BadRequest(new { message = result.ErrorMessage ?? "Varsayılan adres ayarlanamadı." });



            return Ok(result.Data);

        }

        catch (Exception ex)

        {

            return StatusCode(500, new { message = ex.Message });

        }

    }



    [HttpGet]

    [Route("account/locations/countries")]

    public async Task<IActionResult> LocationCountries(CancellationToken cancellationToken)

    {

        var result = await _locations.GetCountriesAsync("tr", cancellationToken);

        return Ok(result.Data ?? new List<LocationLookupItemModel>());

    }



    [HttpGet]

    [Route("account/locations/cities")]

    public async Task<IActionResult> LocationCities([FromQuery] long countryId, CancellationToken cancellationToken)

    {

        var result = await _locations.GetCitiesAsync(countryId, "tr", cancellationToken);

        return Ok(result.Data ?? new List<LocationLookupItemModel>());

    }



    [HttpGet]

    [Route("account/locations/towns")]

    public async Task<IActionResult> LocationTowns([FromQuery] long cityId, CancellationToken cancellationToken)

    {

        var result = await _locations.GetTownsAsync(cityId, "tr", cancellationToken);

        return Ok(result.Data ?? new List<LocationLookupItemModel>());

    }



    [HttpGet]

    [Route("account/forgot-password")]

    public IActionResult ForgotPassword()

    {

        return View();

    }



    [HttpPost]

    [Route("account/forgot-password")]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> ForgotPasswordPost(string email, CancellationToken cancellationToken)

    {

        var result = await _customerAuth.RequestPasswordResetAsync(email ?? "", cancellationToken);

        ViewBag.Message = "İstek alındı. Hesap varsa e-posta ile sıfırlama bağlantısı gönderilir.";

        if (!result.Status && !string.IsNullOrWhiteSpace(result.ErrorMessage))

            ViewBag.Error = result.ErrorMessage;

        return View("ForgotPassword");

    }



    [HttpGet]

    [Route("account/reset-password")]

    public IActionResult ResetPassword([FromQuery] string? token)

    {

        ViewBag.Token = token ?? "";

        return View();

    }



    [HttpPost]

    [Route("account/reset-password")]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> ResetPasswordPost(string token, string newPassword, string confirmPassword, CancellationToken cancellationToken)

    {

        ViewBag.Token = token ?? "";

        if (string.IsNullOrWhiteSpace(token))

        {

            ViewBag.Error = "Geçersiz bağlantı.";

            return View("ResetPassword");

        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)

        {

            ViewBag.Error = "Şifre en az 6 karakter olmalıdır.";

            return View("ResetPassword");

        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))

        {

            ViewBag.Error = "Şifreler eşleşmiyor.";

            return View("ResetPassword");

        }



        var result = await _customerAuth.ConfirmPasswordResetAsync(token, newPassword, cancellationToken);

        if (!result.Status)

        {

            ViewBag.Error = result.ErrorMessage ?? "Şifre sıfırlanamadı.";

            return View("ResetPassword");

        }



        ViewBag.Success = "Şifreniz güncellendi. Giriş yapabilirsiniz.";

        ViewBag.Token = "";

        return View("ResetPassword");

    }



    [HttpGet]

    [Route("account/verify-email")]

    public async Task<IActionResult> VerifyEmail([FromQuery] string? token, CancellationToken cancellationToken)

    {

        if (string.IsNullOrWhiteSpace(token))

        {

            ViewBag.Error = "Geçersiz veya eksik doğrulama bağlantısı.";

            return View();

        }



        var result = await _customerAuth.VerifyEmailAsync(token, cancellationToken);

        if (!result.Status)

        {

            ViewBag.Error = result.ErrorMessage ?? "E-posta doğrulanamadı.";

            return View();

        }



        ViewBag.Success = "E-posta adresiniz doğrulandı.";

        return View();

    }



    [HttpPost]

    [Route("account/login")]

    public async Task<IActionResult> Login([FromBody] CustomerLoginForm form, CancellationToken cancellationToken)

    {

        if (form == null || string.IsNullOrWhiteSpace(form.Email) || string.IsNullOrWhiteSpace(form.Password))

            return BadRequest(new { message = "E-posta ve şifre zorunludur." });



        var result = await _customerAuth.LoginAsync(form, cancellationToken);

        if (!result.Status || result.Data == null)

            return BadRequest(new { message = result.ErrorMessage ?? "Giriş başarısız." });



        await _customerSession.SaveAsync(result.Data, cancellationToken);

        await MergeCartAfterAuthAsync(cancellationToken);

        return Ok(new

        {

            customer = result.Data.Customer,

            displayName = BuildDisplayName(result.Data.Customer)

        });

    }



    [HttpPost]

    [Route("account/register")]

    public async Task<IActionResult> Register([FromBody] CustomerRegisterForm form, CancellationToken cancellationToken)

    {

        if (form == null || string.IsNullOrWhiteSpace(form.Email) || string.IsNullOrWhiteSpace(form.Password))

            return BadRequest(new { message = "E-posta ve şifre zorunludur." });



        var session = await _storeAuth.GetSessionAsync(cancellationToken);

        var result = await _customerAuth.RegisterAsync(form, session.LanguageCode ?? "tr", cancellationToken);

        if (!result.Status || result.Data == null)

            return BadRequest(new { message = result.ErrorMessage ?? "Kayıt başarısız." });



        await _customerSession.SaveAsync(result.Data, cancellationToken);

        await MergeCartAfterAuthAsync(cancellationToken);

        return Ok(new

        {

            customer = result.Data.Customer,

            displayName = BuildDisplayName(result.Data.Customer)

        });

    }



    [HttpGet]

    [Route("account/logout")]

    public async Task<IActionResult> Logout(CancellationToken cancellationToken)

    {

        await _customerSession.ClearAsync(cancellationToken);

        return Redirect("/");

    }



    [HttpPost]

    [Route("account/logout")]

    public async Task<IActionResult> LogoutApi(CancellationToken cancellationToken)

    {

        await _customerSession.ClearAsync(cancellationToken);

        return Ok(new { ok = true });

    }



    [HttpGet]

    [Route("account/me")]

    public async Task<IActionResult> Me(CancellationToken cancellationToken)

    {

        var auth = await _customerSession.GetAsync(cancellationToken);

        if (auth?.Customer == null)

            return Ok(new { loggedIn = false });



        return Ok(new

        {

            loggedIn = true,

            customer = auth.Customer,

            displayName = BuildDisplayName(auth.Customer)

        });

    }



    private async Task PrepareAccountViewAsync(string activeSection, CancellationToken cancellationToken)

    {

        var auth = await _customerSession.GetAsync(cancellationToken);

        ViewBag.AccountActiveSection = activeSection;

        ViewBag.AccountDisplayName = BuildDisplayName(auth?.Customer);

    }



    private static string BuildDisplayName(CustomerProfileModel? customer)

    {

        if (customer == null)

            return "";



        var fullName = $"{customer.FirstName} {customer.LastName}".Trim();

        if (!string.IsNullOrWhiteSpace(fullName))

            return fullName;



        return customer.Email ?? customer.UserName ?? "";

    }



    private async Task MergeCartAfterAuthAsync(CancellationToken cancellationToken)

    {

        var guestId = _basketUserId.GetGuestBasketUserId();

        if (string.IsNullOrWhiteSpace(guestId))

            return;

        var session = await _storeAuth.GetSessionAsync(cancellationToken);

        try

        {

            await _orders.MergeCartAsync(new MergeCartForm

            {

                GuestUserId = guestId,

                LanguageCode = session.LanguageCode ?? "tr"

            }, cancellationToken);

        }

        catch

        {

            // Sepet birleştirme başarısız olsa da giriş tamamlanır.

        }

    }



    private static void EnrichOrderListMedia(List<ShopOrderListItemModel> items)

    {

        foreach (var item in items)

        {

            item.Thumbnails = item.Thumbnails

                .Select(MediaUrlHelper.Build)

                .Where(u => !string.IsNullOrWhiteSpace(u))

                .ToList();

        }

    }



    private static void EnrichOrderDetailMedia(ShopOrderDetailModel data)

    {

        foreach (var line in data.Lines)

        {

            if (!string.IsNullOrWhiteSpace(line.Picture))

                line.Picture = MediaUrlHelper.Build(line.Picture);

        }

    }



    private static string FixInvoicePreviewLogo(string html)

    {

        return Regex.Replace(

            html,

            "(<img class=\"logo\" src=\")([^\"]+)(\")",

            match =>

            {

                var src = match.Groups[2].Value;

                if (src.StartsWith("http://", StringComparison.OrdinalIgnoreCase)

                    || src.StartsWith("https://", StringComparison.OrdinalIgnoreCase)

                    || src.StartsWith("data:", StringComparison.OrdinalIgnoreCase))

                    return match.Value;

                var full = MediaUrlHelper.BuildBrandAsset(AppConfig.CDNServices?.Logo, src);

                if (string.IsNullOrWhiteSpace(full))

                    full = MediaUrlHelper.Build(src);

                return match.Groups[1].Value + full + match.Groups[3].Value;

            },

            RegexOptions.IgnoreCase);

    }



    private static void EnrichInvoiceListPdfUrls(IEnumerable<CustomerInvoiceListItemModel> items)

    {

        foreach (var item in items)

        {

            if (item.HasPdf || item.Status >= 1)

                item.PdfUrl = $"/account/invoices/{item.ID}/pdf";

        }

    }



    private static void EnrichInvoicePdfUrl(CustomerInvoiceDetailModel data)

    {

        if (data.Status >= 1 || !string.IsNullOrWhiteSpace(data.PdfUrl))

            data.PdfUrl = $"/account/invoices/{data.ID}/pdf";

        else

            data.PdfUrl = null;

    }

}



public sealed class SetDefaultAddressForm

{

    public string Type { get; set; } = "";

}


