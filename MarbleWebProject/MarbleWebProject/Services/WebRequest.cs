using MarbleWebProject.Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Xml.Serialization;

namespace MarbleWebProject.Services
{
    public class WebRequest : RestRequest, IRestRequest, IDisposable
    {
        public WebRequest()
        {
        }

        public WebRequest(string resource, RestSharp.Method method, string token)
        {
            this.Resource = resource;
            this.Method = method;

        }
        public Users UserDto { get; set; }
        public IAuthenticator Authenticator { get; set; } = null;
        public IWebProxy Proxy { get; set; } = null;
        public string Endpoint { get; set; }
        public bool LogEnable { get; set; }
        public string LogName { get; set; }
        public long SessionId { get; set; }
        public List<RestResponseCookie> Cookies { get; set; } = new List<RestResponseCookie>();
        public JsonSerializerSettings SerializerSettings { get; set; } = null;

        public void SetAllCookies(List<RestResponseCookie> cookies)
        {
            if (cookies != null)
            {
                foreach (var item in cookies)
                {
                    this.AddCookie(item.Name, item.Value);
                }
            }
        }

        public T Execute<T>(string logName = null)
        {
            using (var client = new MarbleWebClient())
            {
                if (!string.IsNullOrEmpty(Endpoint))
                {
                    client.BaseUrl = new Uri(Endpoint);
                }
                client.Authenticator = Authenticator;
                client.Proxy = Proxy;

                //if (ProviderId > 0 && LogEnable)
                //{
                //EVENTS
                client.OnEntry += OnEntry;
                client.OnException += OnException;
                client.OnExit += OnExit;
                client.OnSuccess += OnSuccess;
                // }

                LogName = logName;

                var response = client.Execute(this);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (response.ErrorException != null)
                    {
                        throw response.ErrorException;
                    }
                    else
                    {
                        throw new Exception(response.Content);
                    }
                }

                if (response.Cookies != null && response.Cookies.Count > 0)
                {
                    Cookies.AddRange(response.Cookies);
                }

                if (response.ContentType.Contains("xml"))
                {
                    StringReader stringReader = new StringReader(response.Content);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    return (T)xmlSerializer.Deserialize(stringReader);
                }
                if (SerializerSettings != null)
                    return JsonConvert.DeserializeObject<T>(response.Content, SerializerSettings);
                return JsonConvert.DeserializeObject<T>(response.Content);

            }
        }

        private void OnEntry(object sender, AirWebEventArgs e)
        {

        }

        private void OnSuccess(object sender, AirWebEventArgs e)
        {

        }

        private void OnException(object sender, AirWebEventArgs e)
        {

        }

        private void OnExit(object sender, AirWebEventArgs e)
        {

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
