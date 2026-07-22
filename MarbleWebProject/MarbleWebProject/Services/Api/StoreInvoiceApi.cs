using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public sealed class StoreInvoiceApi : IStoreInvoiceApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreCustomerSession _session;

    public StoreInvoiceApi(IStoreApiClient api, IStoreCustomerSession session)
    {
        _api = api;
        _session = session;
    }

    public async Task<BaseResponse<List<CustomerInvoiceListItemModel>>> GetMyInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<List<CustomerInvoiceListItemModel>>>(
            "/api/customer/invoices", token, cancellationToken);
    }

    public async Task<BaseResponse<CustomerInvoiceDetailModel>> GetInvoiceDetailAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        var token = await _session.GetTokenAsync(cancellationToken);
        return await _api.GetAsync<BaseResponse<CustomerInvoiceDetailModel>>(
            $"/api/customer/invoices/{invoiceId}", token, cancellationToken);
    }

    public async Task<string?> GetInvoicePreviewHtmlAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _session.GetTokenAsync(cancellationToken);
            return await _api.GetStringAsync($"/api/customer/invoices/{invoiceId}/preview", token, cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Found, byte[]? Bytes, string? FileName, string? ErrorMessage)> GetInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _session.GetTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
                return (false, null, null, "Oturum süresi dolmuş olabilir. Lütfen tekrar giriş yapın.");

            var bytes = await _api.GetBytesAsync($"/api/customer/invoices/{invoiceId}/pdf", token, cancellationToken);
            if (bytes.Length == 0)
                return (false, null, null, "Fatura PDF henüz hazır değil.");
            return (true, bytes, $"invoice-{invoiceId}.pdf", null);
        }
        catch (HttpRequestException ex)
        {
            return (false, null, null, ex.Message);
        }
        catch
        {
            return (false, null, null, "Fatura PDF indirilemedi.");
        }
    }
}
