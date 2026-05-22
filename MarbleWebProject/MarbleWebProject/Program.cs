using MarbleWebProject.Helper;
using MarbleWebProject.Infrastructure;
using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;



IConfiguration Configuration;
//IServiceCollection Services;
var builder = WebApplication.CreateBuilder(args);
Configuration = builder.Configuration;

var forwardHeadersEnabled = Configuration.GetValue("ForwardedHeaders:Enabled", false);
if (forwardHeadersEnabled)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

AppConfig.UseIndexedProductRouting = Configuration.GetValue("Routing:UseIndexedProductRouting", true);
//Services = builder.Services;


AppConfig.CMSService = new CMSService()
{
    CustomName = Configuration.GetValue<string>("CMSService:CustomName"),
    Password = Configuration.GetValue<string>("CMSService:Password"),
    UserName = Configuration.GetValue<string>("CMSService:UserName"),
};
AppConfig.CDNServices = new CDNServices()
{

    ContentUploads = Configuration.GetValue<string>("CDNServices:ContentUploads"),
    ExtraContentPhotos = Configuration.GetValue<string>("CDNServices:ExtraContentPhotos"),
    Favicon = Configuration.GetValue<string>("CDNServices:Favicon"),
};
AppConfig.StorefrontApiBaseUrl = Configuration.GetValue<string>("StorefrontApi:BaseUrl") ?? "https://localhost:7198/";
if (!AppConfig.StorefrontApiBaseUrl.EndsWith('/'))
{
    AppConfig.StorefrontApiBaseUrl += "/";
}
AppConfig.StorefrontGuestCheckoutKey = Configuration.GetValue<string>("StorefrontApi:GuestCheckoutKey") ?? string.Empty;
AppConfig.StorefrontLayoutVersion = Configuration.GetValue<string>("Storefront:LayoutVersion") ?? "v1";

builder.Services.AddHttpClient(nameof(AgencyFeatureBootstrapHostedService), (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var u = cfg["StorefrontApi:BaseUrl"] ?? "https://localhost:7198/";
    if (!u.EndsWith('/'))
        u += "/";
    client.BaseAddress = new Uri(u);
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHostedService<AgencyFeatureBootstrapHostedService>();

builder.Services.AddScoped<TelemetryClient>();

//builder.Services.AddAutoMapper(typeof(Program));
//builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddDistributedMemoryCache();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
//builder.Services.AddHttpClient<ApiBaseUrl>();
//builder.Services.AddHttpClient<LoginService>(client =>
//{
//    client.BaseAddress = new Uri("https://localhost:7198/");
//});
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//        .AddCookie(options =>
//        {
//            options.LoginPath = "/Login/";
//            options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
//        });
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("Language", policy => policy.RequireClaim("LangCode"));
//});
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
builder.Services.AddControllersWithViews()
    .AddViewLocalization();
//builder.Services.Configure<RazorViewEngineOptions>(options =>
//{
//    options.ViewLocationExpanders.Add(new RazorViewLocationExpander());
//    //options.AreaViewLocationFormats.Clear();
//    // options.AreaViewLocationFormats.Add("/Areas/Hotels/Views/Hotels/{0}" + RazorViewEngine.ViewExtension);
//    //options.AreaViewLocationFormats.Add("/Areas/HotelPackages/Views/HotelPackages/{0}" + RazorViewEngine.ViewExtension);
//});
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});// Multi language support
builder.Services.AddMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
    option.Cookie.SameSite = SameSiteMode.Lax;
    option.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "ready" });
var app = builder.Build();

if (forwardHeadersEnabled)
    app.UseForwardedHeaders();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCookiePolicy();
//app.UseAuthentication();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error/500");
    app.UseStatusCodePagesWithReExecute("/error/404", "?code={0}");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.Use(async (ctx, next) =>
{
    if (!ctx.Request.Cookies.ContainsKey("UserIDForBasket"))
    {
        Guid guid = Guid.NewGuid();
        ctx.Response.Cookies.Append("UserIDForBasket", guid.ToString(), new CookieOptions
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
        string originalPath = ctx.Request.Path.Value;
        ctx.Items["originalPath"] = originalPath;
        ctx.Request.Path = "/error/404";
        await next();
    }
});

app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Home}/{action=Index}/{id?}",
   constraints: new { controller = @"^(Home|Checkout|Cart|Category|ProductDetail|Error|Faq|StaticPage|Wishlist|ShoppingCart|Language|CatalogDispatch|Sitemap)$" });
app.MapControllerRoute(
    name: "sitemap",
    pattern: "sitemap.xml",
    defaults: new { controller = "Sitemap", action = "Index" }
);
app.UseEndpoints(endpoints =>
{
    var routeDefinitions = DynamicRouteHelper.GenerateRouteAll();
    foreach (var routeDefinition in routeDefinitions.GenerateRoute)
    {
        endpoints.MapControllerRoute(
            name: routeDefinition.Name,
            pattern: routeDefinition.Pattern,
            defaults: routeDefinition.Defaults
        );
    }

});

//var loginService = app.Services.GetRequiredService<LoginService>();
//await loginService.LoginAsync("ADMIN", "123456", "sadsad");
app.Run();
