using Newtonsoft.Json;
using RestSharp.Serialization;
using RestSharp;
using System.Net;
using System.Text;

namespace MarbleWebProject.Services
{

    public class MarbleWebClient : RestClient, IRestClient, IDisposable
    {
        private AirWebEventArgs args = new AirWebEventArgs();

        public MarbleWebClient()
        {
            Initialize();
        }

        public void Initialize()
        {
            args = new AirWebEventArgs();
            args.WebClient = this;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            this.UseSerializer<AirWebSerializer>();
            this.RemoteCertificateValidationCallback += (val1, val2, val3, val4) =>
            {
                return true;
            };
        }

        public override IRestResponse Execute(IRestRequest request)
        {
            request.Timeout = 1000 * 60 * 2; //2 minutes

            args.LastRequest = request;

            OnEntry?.Invoke(this, args);

            args.LastResponse = base.Execute(request);
            if (args.LastResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                OnException?.Invoke(this, args);
            }
            else
            {
                OnSuccess?.Invoke(this, args);
            }

            OnExit?.Invoke(this, args);

            return args.LastResponse;
        }

        public event AirWebExecEventHandler OnEntry;
        public event AirWebExecEventHandler OnException;
        public event AirWebExecEventHandler OnSuccess;
        public event AirWebExecEventHandler OnExit;

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class AirWebSerializer : IRestSerializer
    {
        private readonly JsonSerializerSettings _serializer = null;

        public AirWebSerializer()
        {
            _serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _serializer);
        }

        public T Deserialize<T>(IRestResponse response)
        {
            return JsonConvert.DeserializeObject<T>(response.Content, _serializer);
        }

        public string ContentType { get; set; } = "application/json";

        public string Serialize(RestSharp.Parameter parameter) => Serialize(parameter.Value);

        public string[] SupportedContentTypes { get; } =
        {
            "application/json", "text/json", "text/x-json", "text/javascript", "*+json"
        };

        public DataFormat DataFormat { get; } = DataFormat.Json;
    }

    public delegate void AirWebExecEventHandler(object sender, AirWebEventArgs e);

    public class AirWebEventArgs : EventArgs
    {
        public MarbleWebClient WebClient { get; set; }
        public IRestRequest LastRequest { get; set; }
        public IRestResponse LastResponse { get; set; }
        public object Tag { get; set; }

        public bool HasError
        {
            get
            {
                if (LastResponse != null && LastResponse.StatusCode != HttpStatusCode.OK)
                {
                    return true;
                }

                return false;
            }
        }

        public string MethodName
        {
            get
            {
                if (LastRequest != null)
                {
                    return LastRequest.Resource;
                }

                return "";
            }
        }

        public string GenerateRequest()
        {
            if (LastRequest != null)
            {
                var resource = LastRequest.Resource;
                var httpRequest = new StringBuilder();

                var urlSegments = LastRequest.Parameters.FindAll(i => i.Type == ParameterType.UrlSegment);
                if (urlSegments.Count >= 1)
                {
                    foreach (var item in urlSegments)
                    {
                        if (item.Value != null)
                        {
                            resource = resource.Replace("{" + item.Name + "}", item.Value.ToString());
                        }

                    }
                }

                var queries = LastRequest.Parameters.FindAll(i => i.Type == ParameterType.QueryString);
                if (queries.Count >= 1)
                {
                    var queryItems = new List<string>();
                    foreach (var item in queries)
                    {
                        queryItems.Add($"{item.Name}={item.Value}");
                    }
                    resource += $"?{string.Join("&", queryItems)}";
                }

                httpRequest.AppendLine($"{LastRequest.Method.ToString()} {resource} HTTP/1.1");
                httpRequest.AppendLine($"Host: {WebClient.BaseUrl}");

                var headers = LastRequest.Parameters.FindAll(i => i.Type == ParameterType.HttpHeader);
                if (headers.Count >= 1)
                {
                    foreach (var item in headers)
                    {
                        httpRequest.AppendLine($"{item.Name}: {item.Value}");
                    }
                }

                var bodies = LastRequest.Parameters.FindAll(i => i.Type == ParameterType.RequestBody);
                if (bodies.Count >= 1)
                {
                    httpRequest.AppendLine("");

                    foreach (var item in bodies)
                    {
                        if (item.Value == null) continue;

                        var json = JsonConvert.SerializeObject(item.Value);
                        if (string.IsNullOrEmpty(json)) continue;

                        httpRequest.AppendLine($"{json}");
                    }
                }

                return httpRequest.ToString();
            }

            return "";
        }

        public string GenerateResponse()
        {
            if (LastResponse != null)
            {
                if (!string.IsNullOrEmpty(LastResponse.Content))
                {
                    return LastResponse.Content;
                }
                else if (LastResponse.ErrorException != null)
                {
                    return LastResponse.ErrorException.Message;
                }
                else if (!string.IsNullOrEmpty(LastResponse.ErrorMessage))
                {
                    return LastResponse.ErrorMessage;
                }
                else
                {
                    return $"{LastResponse.StatusCode} # {LastResponse.StatusDescription} - Not found any response";
                }
            }

            return "";
        }
    }
}
