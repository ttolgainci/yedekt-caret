using System.Net.Http.Json;
using MarbleWebProject.Infrastructure;
using MarbleWebProject.Models;

namespace MarbleWebProject.Services;

/// <summary>
/// Uygulama açılışında storefront API’den kiracı layout sürümünü okur; başarısızsa <c>Storefront:LayoutVersion</c> kalır.
/// </summary>
public class AgencyFeatureBootstrapHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgencyFeatureBootstrapHostedService> _logger;

    public AgencyFeatureBootstrapHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<AgencyFeatureBootstrapHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        AppConfig.StorefrontLayoutVersion =
            _configuration.GetValue<string>("Storefront:LayoutVersion") ?? "v1";

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(nameof(AgencyFeatureBootstrapHostedService));
            var cn = AppConfig.CMSService?.CustomName;
            if (string.IsNullOrWhiteSpace(cn))
                cn = "default";

            var url =
                $"api/admin/agency-features?customName={Uri.EscapeDataString(cn.Trim())}&includeRows=false";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.TryAddWithoutValidation(
                CorrelationIdDefaults.HeaderName,
                "web-bootstrap-" + Guid.NewGuid().ToString("N"));
            using var resp = await client.SendAsync(req, cancellationToken);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(cancellationToken);
                if (body.Length > 400)
                    body = body[..400] + "…";
                _logger.LogWarning(
                    "Agency feature bootstrap HTTP {Status}; appsettings layout sürümü kullanılıyor. Body: {Body}",
                    (int)resp.StatusCode,
                    string.IsNullOrWhiteSpace(body) ? "—" : body);
                return;
            }
            var dto = await resp.Content.ReadFromJsonAsync<AgencyFeatureBootstrapDto>(cancellationToken: cancellationToken);
            if (!string.IsNullOrWhiteSpace(dto?.StorefrontLayoutVersion))
            {
                AppConfig.StorefrontLayoutVersion = dto.StorefrontLayoutVersion.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Agency feature profili okunamadı; appsettings Storefront:LayoutVersion kullanılıyor.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
