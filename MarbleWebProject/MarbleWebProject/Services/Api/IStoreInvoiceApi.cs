using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreInvoiceApi
{
    Task<BaseResponse<List<CustomerInvoiceListItemModel>>> GetMyInvoicesAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<CustomerInvoiceDetailModel>> GetInvoiceDetailAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<string?> GetInvoicePreviewHtmlAsync(int invoiceId, CancellationToken cancellationToken = default);
    Task<(bool Found, byte[]? Bytes, string? FileName, string? ErrorMessage)> GetInvoicePdfAsync(int invoiceId, CancellationToken cancellationToken = default);
}
