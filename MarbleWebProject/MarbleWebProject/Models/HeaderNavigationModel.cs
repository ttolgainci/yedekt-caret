namespace MarbleWebProject.Models;

public sealed class HeaderNavigationModel
{
    public List<CategoryListModel> MainNavCategories { get; set; } = new();
    public List<CategoryListModel> OverflowCategories { get; set; } = new();
    public bool ShowBrowseCategories => OverflowCategories.Count > 0;
    public int MaxVisible { get; set; } = 9;
}
