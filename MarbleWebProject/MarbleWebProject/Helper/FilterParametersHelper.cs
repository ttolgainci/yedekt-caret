using MarbleWebProject.Models;

namespace MarbleWebProject.Helper
{
    public static class FilterParametersHelper
    {
        public static List<TranslateAllResponse> GetTranslateByLanguage()
        {
            return TranslateFullList;
        }
        public static List<TranslateAllResponse> TranslateFullList { get; set; } = new();

        public static List<SiteMapUrlModel> GetUrlListForSiteMap()
        {
            return SiteMapUrlList;
        }
        public static List<SiteMapUrlModel> SiteMapUrlList { get; set; } = new();

        public static UserModel GetUserID()
        {
            return UserInfo;
        }
        public static UserModel UserInfo { get; set; }
    }
}
