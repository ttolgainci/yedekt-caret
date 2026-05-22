namespace MarbleWebProject.Infrastructure;

/// <summary>Storefront API ile aynı başlık adı: <c>X-Correlation-ID</c>.</summary>
public static class CorrelationIdDefaults
{
    public const string HeaderName = "X-Correlation-ID";
    public const string HttpContextItemKey = "CorrelationId";
}
