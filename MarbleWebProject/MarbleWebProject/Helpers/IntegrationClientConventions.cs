namespace MarbleWebProject.Helpers;

/// <summary>
/// Faz 0: Web (mağaza) entegrasyon kuralları — vitrin yalnızca storefront API kullanır.
/// Dokümantasyon: ../docs/INTEGRATION_MULTI_PROVIDER.md (repo kökü)
/// </summary>
public static class IntegrationClientConventions
{
    /// <summary>Ödeme gateway, e-Fatura, kargo API doğrudan Web'den çağrılmaz.</summary>
    public const bool ExternalProvidersAllowedInWeb = false;

    /// <summary>Müşteri fatura listesi / PDF.</summary>
    public const string CustomerInvoicesApiPrefix = "/api/customer/invoices";

    /// <summary>Kargo ücret hesaplama.</summary>
    public const string ShippingQuotePath = "/api/shipping/calculate";

    /// <summary>Kullanıcıya gösterilen mesajlarda entegratör markası kullanılmamalı (Faz 1+).</summary>
    public const string DocumentationFile = "docs/INTEGRATION_MULTI_PROVIDER.md";
}
