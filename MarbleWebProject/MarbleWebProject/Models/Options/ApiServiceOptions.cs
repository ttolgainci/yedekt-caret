namespace MarbleWebProject.Models.Options;

public sealed class ApiServiceOptions
{
    public const string SectionName = "ApiService";

    public string BaseUrl { get; set; } = "http://localhost:5210";
}
