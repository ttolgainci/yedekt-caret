namespace MarbleWebProject.Models;

public sealed class HomeBrandModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Picture { get; set; }
    public string? Slug { get; set; }
}
