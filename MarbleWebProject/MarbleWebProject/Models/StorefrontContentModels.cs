namespace MarbleWebProject.Models;

public sealed class StorefrontFooterLinksModel
{
    public List<StorefrontFooterLinkGroupModel> Groups { get; set; } = new();
}

public sealed class StorefrontFooterLinkGroupModel
{
    public string Title { get; set; } = "";
    public List<StorefrontFooterLinkItemModel> Links { get; set; } = new();
}

public sealed class StorefrontFooterLinkItemModel
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class StorefrontCheckoutLegalModel
{
    public List<StorefrontCheckoutLegalItemModel> Items { get; set; } = new();
}

public sealed class StorefrontCheckoutLegalItemModel
{
    public int InformationId { get; set; }
    public int RevisionId { get; set; }
    public string PageCode { get; set; } = "";
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string ContentHash { get; set; } = "";
}

public sealed class PlaceOrderLegalConsentApiItem
{
    public int InformationId { get; set; }
    public int RevisionId { get; set; }
    public string PageCode { get; set; } = "";
    public string ContentHash { get; set; } = "";
}
