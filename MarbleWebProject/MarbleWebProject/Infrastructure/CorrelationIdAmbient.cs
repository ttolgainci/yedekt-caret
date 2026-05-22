namespace MarbleWebProject.Infrastructure;

/// <summary>RestSharp tabanlı <c>CmsClient</c> gibi DI kullanmayan çağrılar için async akışda correlation.</summary>
public static class CorrelationIdAmbient
{
    private static readonly AsyncLocal<string?> Holder = new();

    public static string? Current => Holder.Value;

    internal static void SetCurrent(string? value) => Holder.Value = value;
}
