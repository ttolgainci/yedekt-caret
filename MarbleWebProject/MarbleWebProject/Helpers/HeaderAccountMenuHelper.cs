using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;

namespace MarbleWebProject.Helpers;

public static class HeaderAccountMenuHelper
{
    public static async Task<HeaderAccountMenuModel> BuildAsync(IStoreCustomerSession customerSession, CancellationToken cancellationToken = default)
    {
        var auth = await customerSession.GetAsync(cancellationToken);
        var customer = auth?.Customer;
        if (customer == null)
            return new HeaderAccountMenuModel();

        var fullName = $"{customer.FirstName} {customer.LastName}".Trim();
        var displayName = !string.IsNullOrWhiteSpace(fullName)
            ? fullName
            : customer.Email ?? customer.UserName ?? "";

        return new HeaderAccountMenuModel
        {
            IsLoggedIn = true,
            DisplayName = displayName
        };
    }
}
