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



    [HttpPost]

    [Route("account/logout")]

    public async Task<IActionResult> Logout(CancellationToken cancellationToken)

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

            return Unauthorized();



        return Ok(new

        {

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

}



public sealed class SetDefaultAddressForm

{

    public string Type { get; set; } = "";

}


