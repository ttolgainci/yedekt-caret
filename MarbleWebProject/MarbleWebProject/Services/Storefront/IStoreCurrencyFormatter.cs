using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Storefront;

public interface IStoreCurrencyFormatter
{
    string Format(decimal? amount, string? currencyLabel = null, int? currencyId = null);
    StoreCurrencyModel? Resolve(string? currencyLabel = null, int? currencyId = null);
}
