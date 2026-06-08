using MarbleWebProject.Models;

namespace MarbleWebProject.Services.CheckoutDraft;

public interface ICheckoutDraftStore
{
    Task<CheckoutDraftModel?> GetAsync(string basketUserId, CancellationToken cancellationToken = default);
    Task SaveAsync(string basketUserId, CheckoutDraftModel draft, CancellationToken cancellationToken = default);
    Task DeleteAsync(string basketUserId, CancellationToken cancellationToken = default);
}
