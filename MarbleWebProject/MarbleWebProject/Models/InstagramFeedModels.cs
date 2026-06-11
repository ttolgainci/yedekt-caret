namespace MarbleWebProject.Models;

public sealed class StorefrontInstagramFeedModel
{
    public List<StorefrontInstagramAccountFeedModel> Accounts { get; set; } = new();
}

public sealed class StorefrontInstagramAccountFeedModel
{
    public int AccountId { get; set; }
    public string Username { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
    public List<StorefrontInstagramPostModel> Posts { get; set; } = new();
}

public sealed class StorefrontInstagramPostModel
{
    public string MediaType { get; set; } = "";
    public string? Caption { get; set; }
    public string ImageUrl { get; set; } = "";
    public string Permalink { get; set; } = "";
    public DateTime PostedAtUtc { get; set; }
}
