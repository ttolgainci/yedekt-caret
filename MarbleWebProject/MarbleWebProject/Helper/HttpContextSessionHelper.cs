namespace MarbleWebProject.Helper
{
    public class HttpContextSessionHelper : IDisposable
    {
        private readonly HttpContext _httpContext;

        public HttpContextSessionHelper(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        public void SetSession(string key, string value)
        {
            _httpContext.Session.SetString(key, value);
        }
        public string GetSession(string key)
        {
            return _httpContext.Session.GetString(key);
        }
        public T Get<T>(string key) where T : class
        {
            var value = _httpContext.Session.GetString(key);
            return value == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
        }
        public void RemoveSession(string key)
        {
            _httpContext.Session.Remove(key);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
