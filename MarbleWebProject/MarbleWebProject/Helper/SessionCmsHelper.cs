using MarbleWebProject.Models;
using System.Text.Json;

namespace MarbleWebProject.Helper
{
    public class SessionCmsHelper
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public SessionCmsHelper(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public void SetSession(TokenResponse token)
        {
            HttpContextSessionHelper sessionHelper = new HttpContextSessionHelper(_contextAccessor.HttpContext);
            string setTokenResponse = JsonSerializer.Serialize(token);
            sessionHelper.SetSession("CmsApiToken", setTokenResponse);
        }
        public TokenResponse GetSession()
        {
            HttpContextSessionHelper sessionHelper = new HttpContextSessionHelper(_contextAccessor.HttpContext);
            TokenResponse response = new TokenResponse();
            if (!string.IsNullOrEmpty(sessionHelper.GetSession("CmsApiToken")))
            {
                var rtn = sessionHelper.Get<TokenResponse>("CmsApiToken");
                response.Token = rtn.Token;
                //response.Market = rtn.Market;
                response.LanguageName = rtn.LanguageName;
                response.LanguageCode = rtn.LanguageCode;
                response.LanguageCulture = rtn.LanguageCulture;
            }
            return response;
        }
    }
}
