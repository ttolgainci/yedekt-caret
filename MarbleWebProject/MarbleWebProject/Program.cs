using MarbleWebProject.Helper;
using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Models.Options;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.CheckoutDraft;
using MarbleWebProject.Services.Storefront;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var apiBaseUrl = NormalizeLocalDevServiceUrl(
    (configuration.GetValue<string>("ApiService:BaseUrl")
        ?? configuration.GetValue<string>("CMSService:EndPoint")
        ?? "http://localhost:5206").Trim().TrimEnd('/'));

AppConfig.CMSService = new CMSService
{
    EndPoint = apiBaseUrl,
    CustomName = configuration.GetValue<string>("StoreAuth:CustomName")
        ?? configuration.GetValue<string>("CMSService:CustomName") ?? "",
    Password = configuration.GetValue<string>("StoreAuth:Password")
        ?? configuration.GetValue<string>("CMSService:Password") ?? "",
    UserName = configuration.GetValue<string>("StoreAuth:UserName")
        ?? configuration.GetValue<string>("CMSService:UserName") ?? "",
};
AppConfig.CDNServices = new CDNServices
{
    ContentUploads = NormalizeLocalDevServiceUrl(configuration.GetValue<string>("CDNServices:ContentUploads") ?? ""),
    ExtraContentPhotos = configuration.GetValue<string>("CDNServices:ExtraContentPhotos") ?? "",
    Favicon = configuration.GetValue<string>("CDNServices:Favicon") ?? "",
};
AppConfig.ProjectService = ProjectServiceSettings.FromConfiguration(configuration);

builder.Services.Configure<ApiServiceOptions>(configuration.GetSection(ApiServiceOptions.SectionName));
builder.Services.AddOptions<ProjectServiceSettings>()
    .BindConfiguration(ProjectServiceSettings.SectionName)
    .PostConfigure(ProjectSector.ApplyConfigurationDefaults);
builder.Services.Configure<StoreAuthOptions>(options =>
{
    configuration.GetSection(StoreAuthOptions.SectionName).Bind(options);
    if (string.IsNullOrWhiteSpace(options.UserName))
        options.UserName = configuration.GetValue<string>("CMSService:UserName") ?? "";
    if (string.IsNullOrWhiteSpace(options.Password))
        options.Password = configuration.GetValue<string>("CMSService:Password") ?? "";
    if (string.IsNullOrWhiteSpace(options.CustomName))
        options.CustomName = configuration.GetValue<string>("CMSService:CustomName") ?? "";
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
builder.Services.AddScoped<IBasketUserIdProvider, BasketUserIdProvider>();
builder.Services.AddScoped<IStoreCatalogApi, StoreCatalogApi>();
builder.Services.AddScoped<IStoreBasketApi, StoreBasketApi>();
builder.Services.AddScoped<IStoreShippingApi, StoreShippingApi>();
builder.Services.AddScoped<IStoreContentApi, StoreContentApi>();
builder.Services.AddScoped<IStoreRouteBootstrap, StoreRouteBootstrap>();
builder.Services.AddScoped<IStoreStorefrontApi, StoreStorefrontApi>();
builder.Services.AddScoped<IStorefrontRuntimeProvider, StorefrontRuntimeProvider>();
builder.Services.AddSingleton<ICheckoutDraftStore, CheckoutDraftStore>();

builder.Services.AddScoped<TelemetryClient>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
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

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "sitemap", pattern: "sitemap.xml", defaults: new { controller = "Sitemap", action = "Index" });

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

app.MapControllerRoute(
    name: "product-result-search",
    pattern: "arama",
    defaults: new { controller = "ProductResult", action = "Index" });

foreach (var routeDefinition in routeDefinitions.GenerateRoute)
{
    app.MapControllerRoute(
        name: routeDefinition.Name,
        pattern: routeDefinition.Pattern,
        defaults: routeDefinition.Defaults);
}

app.Run();

static string NormalizeLocalDevServiceUrl(string url)
{
    if (url.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase)
        || url.StartsWith("https://127.0.0.1", StringComparison.OrdinalIgnoreCase))
        return "http://" + url[8..];
    return url;
}
