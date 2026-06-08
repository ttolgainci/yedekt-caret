namespace MarbleWebProject.Helpers;

public static class CurrencyDisplayHelper
{
    public static string GetLabel(string? symbol, string? code, string fallback = "₺")
    {
        if (!string.IsNullOrWhiteSpace(symbol))
            return symbol.Trim();
        if (!string.IsNullOrWhiteSpace(code))
            return code.Trim();
        return fallback;
    }
}
