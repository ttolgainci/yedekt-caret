using MarbleWebProject.Helper;
using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Models.Options;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.Cache;
using MarbleWebProject.Services.CheckoutDraft;
using StackExchange.Redis;
using MarbleWebProject.Services.Storefront;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Mağaza entegrasyonları yalnızca API üzerinden — IntegrationClientConventions (Faz 0).

var apiBaseUrl = NormalizeLocalDevServiceUrl(
    (configuration.GetValue<string>("ApiService:BaseUrl") ?? "http://localhost:5206").Trim().TrimEnd('/'));

var storeAuth = configuration.GetSection(StoreAuthOptions.SectionName).Get<StoreAuthOptions>()
    ?? new StoreAuthOptions();
storeAuth.EndPoint = apiBaseUrl;
storeAuth.Theme = StoreThemeHelper.Resolve(storeAuth.Theme);
ProjectSector.ApplyConfigurationDefaults(storeAuth);

AppConfig.Storefront.StoreAuth = storeAuth;
AppConfig.CDNServices = new CDNServices
{
    ContentUploads = NormalizeLocalDevServiceUrl(configuration.GetValue<string>("CDNServices:ContentUploads") ?? ""),
    ExtraContentPhotos = configuration.GetValue<string>("CDNServices:ExtraContentPhotos") ?? "",
    Favicon = configuration.GetValue<string>("CDNServices:Favicon") ?? "",
    Logo = configuration.GetValue<string>("CDNServices:Logo") ?? "",
};

builder.Services.Configure<ApiServiceOptions>(configuration.GetSection(ApiServiceOptions.SectionName));
builder.Services.AddOptions<StoreAuthOptions>()
    .BindConfiguration(StoreAuthOptions.SectionName)
    .PostConfigure(options =>
    {
        options.UserName = storeAuth.UserName;
        options.Password = storeAuth.Password;
        options.CustomName = storeAuth.CustomName;
        options.Theme = storeAuth.Theme;
        options.StoreSectorType = storeAuth.StoreSectorType;
        options.SectorCode = storeAuth.SectorCode;
        options.ProjectName = storeAuth.ProjectName;
        options.HeaderNavMaxVisibleCategories = storeAuth.HeaderNavMaxVisibleCategories;
        options.EndPoint = storeAuth.EndPoint;
        ProjectSector.ApplyConfigurationDefaults(options);
    });

builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        if (builder.Environment.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        return handler;
    });
builder.Services.AddScoped<IStoreAuthService, StoreAuthService>();
builder.Services.AddScoped<IStoreCustomerAuthApi, StoreCustomerAuthApi>();
builder.Services.AddScoped<IStoreCustomerSession, StoreCustomerSession>();
builder.Services.AddScoped<IStoreCustomerAddressApi, StoreCustomerAddressApi>();
builder.Services.AddScoped<IStoreLocationApi, StoreLocationApi>();
builder.Services.AddScoped<IStoreOrderApi, StoreOrderApi>();
builder.Services.AddScoped<IStoreInvoiceApi, StoreInvoiceApi>();
builder.Services.AddScoped<IStoreReturnApi, StoreReturnApi>();
builder.Services.AddScoped<IBasketUserIdProvider, BasketUserIdProvider>();
builder.Services.AddScoped<IStoreCatalogApi, StoreCatalogApi>();
builder.Services.AddScoped<IProductSearchUrlResolver, ProductSearchUrlResolver>();
builder.Services.AddScoped<IStoreBasketApi, StoreBasketApi>();
builder.Services.AddScoped<IStoreWishlistApi, StoreWishlistApi>();
builder.Services.AddScoped<IStoreShippingApi, StoreShippingApi>();
builder.Services.AddScoped<IStoreContentApi, StoreContentApi>();
builder.Services.AddScoped<IStoreRouteBootstrap, StoreRouteBootstrap>();
builder.Services.AddScoped<IStoreStorefrontApi, StoreStorefrontApi>();
builder.Services.AddScoped<IStorefrontRuntimeProvider, StorefrontRuntimeProvider>();
builder.Services.AddScoped<IStoreCurrencyFormatter, StoreCurrencyFormatter>();
builder.Services.AddSingleton<ICheckoutDraftStore, CheckoutDraftStore>();

builder.Services.AddScoped<TelemetryClient>();
builder.Services.AddHttpContextAccessor();
var redisConnection = configuration.GetConnectionString("Redis") ?? configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "MarbleWeb:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddSingleton<IGlobalCatalogCacheVersion, GlobalCatalogCacheVersion>();
builder.Services.AddSingleton<IGlobalInstagramCacheVersion, GlobalInstagramCacheVersion>();
builder.Services.AddSingleton<IGlobalContentCacheVersion, GlobalContentCacheVersion>();
builder.Services.AddSingleton<IWebCatalogCache, WebCatalogCache>();
builder.Services.AddSingleton<IWebInstagramCache, WebInstagramCache>();
builder.Services.AddSingleton<IWebContentCache, WebContentCache>();
builder.Services.AddScoped<IStoreInstagramApi, StoreInstagramApi>();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBufferSize = 2147483647;
    options.Limits.MaxRequestBodySize = 2147483647;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodyBufferSize = 2147483647;
    options.MaxRequestBodySize = 2147483647;
});
builder.Services.AddControllersWithViews().AddViewLocalization();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseCookiePolicy();
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/error/500");
    app.UseStatusCodePagesWithReExecute("/error/404", "?code={0}");
    app.UseHsts();
}

app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Cookies.ContainsKey("UserIDForBasket"))
    {
        ctx.Response.Cookies.Append("UserIDForBasket", Guid.NewGuid().ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(14)
        });
    }
    await next();
    if (ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        ctx.Items["originalPath"] = ctx.Request.Path.Value;
        ctx.Request.Path = "/error/404";
        await next();
    }
});

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.Use(async (ctx, next) =>
{
    try
    {
        var storefront = ctx.RequestServices.GetRequiredService<IStorefrontRuntimeProvider>();
        await storefront.GetAsync(ctx.RequestAborted);
    }
    catch (Exception ex)
    {
        var log = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("StorefrontRuntime");
        log.LogWarning(ex, "Storefront layout refresh failed; using previous or default flags.");
    }
    await next();
});
app.UseAuthorization();

List<RouteListModel> routeList;
try
{
    using var scope = app.Services.CreateScope();
    var bootstrap = scope.ServiceProvider.GetRequiredService<IStoreRouteBootstrap>();
    routeList = bootstrap.LoadRouteListAsync().GetAwaiter().GetResult();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("StoreRouteBootstrap");
    logger.LogWarning(ex, "API route list could not be loaded; using static routes only. ApiUrl={ApiUrl}", apiBaseUrl);
    routeList = new List<RouteListModel>();
}

var routeDefinitions = routeList.Count > 0
    ? DynamicRouteHelper.GenerateRouteAll(routeList)
    : DynamicRouteHelper.GenerateStaticRoutesOnly();

app.MapControllers();

app.MapControllerRoute(
    name: "account-invoice-preview",
    pattern: "account/invoices/{id:int}/preview",
    defaults: new { controller = "Account", action = "InvoicePreview" });

app.MapControllerRoute(
    name: "account-invoice-pdf",
    pattern: "account/invoices/{id:int}/pdf",
    defaults: new { controller = "Account", action = "InvoicePdf" });

app.MapControllerRoute(
    name: "product-result-category",
    pattern: "category/{categorySlug}",
    defaults: new { controller = "ProductResult", action = "Category" });

app.MapControllerRoute(
    name: "product-result-vehicle",
    pattern: "arac/{vehiclePath}",
    defaults: new { controller = "ProductResult", action = "Vehicle" });

app.MapControllerRoute(
    name: "product-result-vehicle-legacy",
    pattern: "arama/{makeSlug}/{modelSlug}/{generationSlug}/{engineSlug}",
    defaults: new { controller = "ProductResult", action = "VehicleLegacy" });

app.MapControllerRoute(
    name: "product-result-search",
    pattern: "arama",
    defaults: new { controller = "ProductResult", action = "Index" });

app.MapControllerRoute(
    name: "product-seo-canonical",
    pattern: "{brandSlug}/{productSegment}",
    defaults: new { controller = "ProductDetail", action = "Index" },
    constraints: new { productSegment = @".+-p-\d+$" });

foreach (var routeDefinition in routeDefinitions.GenerateRoute)
{
    app.MapControllerRoute(
        name: routeDefinition.Name,
        pattern: routeDefinition.Pattern,
        defaults: routeDefinition.Defaults);
}

app.MapControllerRoute(name: "sitemap", pattern: "sitemap.xml", defaults: new { controller = "Sitemap", action = "Index" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static string NormalizeLocalDevServiceUrl(string url)
{
    if (url.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://127.0.0.1", StringComparison.OrdinalIgnoreCase))
        return "http://" + url[8..];
    return url;
}
