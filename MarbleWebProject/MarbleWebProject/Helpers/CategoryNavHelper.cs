using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class CategoryNavHelper
{
    public static int GetCategoryId(CategoryListModel model) =>
        model.CategoryID > 0 ? model.CategoryID : model.ID;

    public static string BuildCategoryUrl(CategoryListModel model) =>
        UrlSlugHelper.BuildCategoryPath(model.Url, GetCategoryId(model));

    public static HeaderNavigationModel BuildNavigation(
        List<CategoryListModel> roots,
        int maxVisible)
    {
        var nav = new HeaderNavigationModel { MaxVisible = maxVisible < 1 ? 9 : maxVisible };
        var ordered = roots
            .Where(c => c.Status)
            .OrderBy(c => c.Order ?? c.Name)
            .ThenBy(c => c.Name)
            .ToList();

        nav.MainNavCategories = ordered.Take(nav.MaxVisible).ToList();
        nav.OverflowCategories = ordered.Skip(nav.MaxVisible).ToList();
        return nav;
    }
}
