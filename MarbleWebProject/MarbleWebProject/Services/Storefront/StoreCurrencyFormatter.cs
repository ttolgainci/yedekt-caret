using MarbleWebProject.Helpers;
using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Storefront;

public sealed class StoreCurrencyFormatter : IStoreCurrencyFormatter
{
    public string Format(decimal? amount, string? currencyLabel = null, int? currencyId = null) =>
        CurrencyDisplayHelper.FormatAmount(amount, currencyLabel, currencyId);

    public StoreCurrencyModel? Resolve(string? currencyLabel = null, int? currencyId = null) =>
        CurrencyDisplayHelper.ResolveCurrency(currencyLabel, currencyId);
}
