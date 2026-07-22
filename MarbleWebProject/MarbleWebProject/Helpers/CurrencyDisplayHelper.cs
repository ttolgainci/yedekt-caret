using System.Globalization;
using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class CurrencyDisplayHelper
{
    private const string DefaultPriceFormat = "{0:n2}";
    private const string DefaultFallbackSymbol = "₺";

    public static string GetLabel(string? symbol, string? code, string fallback = DefaultFallbackSymbol)
    {
        if (!string.IsNullOrWhiteSpace(symbol))
            return symbol.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            return code.Trim();
        return fallback;
    }

    public static string FormatAmount(decimal? amount, string? currencyLabel = null, int? currencyId = null)
    {
        if (!amount.HasValue)
            return string.Empty;

        var currency = ResolveCurrency(currencyLabel, currencyId);
        return FormatAmount(amount.Value, currency, currencyLabel);
    }

    public static string FormatAmount(
        decimal amount,
        StoreCurrencyModel? currency,
        string? fallbackLabel = null)
    {
        var culture = ResolveCulture(currency?.DisplayLocale);
        var priceFormat = string.IsNullOrWhiteSpace(currency?.PriceFormat)
            ? DefaultPriceFormat
            : currency.PriceFormat.Trim();

        string formattedAmount;
        try
        {
            formattedAmount = string.Format(culture, priceFormat, amount);
        }
        catch (FormatException)
        {
            formattedAmount = amount.ToString("N2", culture);
        }

        if (UsesBuiltInCurrencySymbol(priceFormat))
            return formattedAmount;

        var symbol = GetLabel(currency?.Symbol, currency?.Code, fallbackLabel);
        if (string.IsNullOrWhiteSpace(symbol))
            return formattedAmount;

        return currency?.SymbolOnRight == true
            ? JoinWithSpace(formattedAmount, symbol)
            : JoinWithSpace(symbol, formattedAmount);
    }

    public static StoreCurrencyModel? ResolveCurrency(string? currencyLabel = null, int? currencyId = null)
    {
        var currencies = AppConfig.Storefront.Currencies;
        if (currencies.Count == 0)
            return null;

        if (currencyId is > 0)
        {
            var byId = currencies.FirstOrDefault(c => c.Id == currencyId.Value);
            if (byId != null)
                return byId;
        }

        if (!string.IsNullOrWhiteSpace(currencyLabel))
        {
            var label = currencyLabel.Trim();
            var byLabel = currencies.FirstOrDefault(c =>
                string.Equals(c.Symbol, label, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Code, label, StringComparison.OrdinalIgnoreCase)
                || string.Equals(c.Title, label, StringComparison.OrdinalIgnoreCase));
            if (byLabel != null)
                return byLabel;
        }

        return currencies
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .FirstOrDefault();
    }

    private static bool UsesBuiltInCurrencySymbol(string priceFormat) =>
        priceFormat.Contains('C', StringComparison.OrdinalIgnoreCase)
        || priceFormat.Contains('¤', StringComparison.Ordinal);

    private static string JoinWithSpace(string left, string right)
    {
        if (string.IsNullOrEmpty(left))
            return right;
        if (string.IsNullOrEmpty(right))
            return left;
        return left + " " + right;
    }

    private static CultureInfo ResolveCulture(string? displayLocale)
    {
        if (!string.IsNullOrWhiteSpace(displayLocale))
        {
            try
            {
                return CultureInfo.GetCultureInfo(displayLocale.Trim());
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return CultureInfo.GetCultureInfo("tr-TR");
    }
}
