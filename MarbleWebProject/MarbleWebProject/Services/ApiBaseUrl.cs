using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MarbleWebProject.Services
{
    public class ApiBaseUrl 
    {
       
        //public HttpClient Client { get; }
        //public JsonSerializerOptions Options { get; }
        //public ApiBaseUrl(HttpClient client)
        //{
        //    client.BaseAddress = new Uri("https://localhost:7054/");
        //    Client = client;
        //    Options = new JsonSerializerOptions()
        //    {
        //        AllowTrailingCommas = true,
        //        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //        IgnoreReadOnlyProperties = true,
        //        NumberHandling = JsonNumberHandling.WriteAsString,
        //        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //        WriteIndented = true
        //    };

        //}

        ////private readonly HttpClient _httpClient;
        //public HttpClient _httpClient { get; }
        //public ApiBaseUrl(HttpClient httpClient)
        //{

        //    httpClient.BaseAddress = new Uri("https://localhost:7198/");
        //    _httpClient = httpClient;
        //}
        //public  LoginResponseModel getSession()
        //{
        //    HttpContextAccessor contextAccessor = new HttpContextAccessor();
        //    //ValidationModel validationModel = new ValidationModel();
        //    LoginResponseModel loginResponse = new LoginResponseModel();
        //    LoginRequestModel loginRequest=new LoginRequestModel();
        //    SessionCmsHelper _session = new SessionCmsHelper(contextAccessor);
        //    string Token = _session.GetSession()?.Token;
        //    double TotalSeconds;
        //    if (Token != null)
        //    {
        //        loginResponse = _session.GetSession();
        //    }
        //    else
        //    {
        //        loginRequest.UserName =AppConfig.CMSService.UserName;
        //        loginRequest.Password = AppConfig.CMSService.Password;
        //        loginRequest.CustomName = AppConfig.CMSService.CustomName;
        //        var asd = Login(_httpClient, loginRequest);
        //        loginResponse = asd.Result.Data;
        //    }
        //    return loginResponse;
        //}
        ////public  async Task<BaseResponse<LoginResponseModel>> Login(LoginRequestModel loginRequest)
        ////{
        ////    var returnModel = new BaseResponse<LoginResponseModel>();
        ////    var response = await _httpClient.PostAsJsonAsync("AccountManager/login", loginRequest);
        ////    if (response.IsSuccessStatusCode)
        ////    {
        ////        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
        ////        returnModel.Success();
        ////        returnModel.Data = loginResponse;

        ////    }
        ////    else
        ////    {
        ////        returnModel.Error("");
        ////        returnModel.Data = null;
        ////        // Handle errors (e.g., return an error response or throw an exception)
        ////        //return new BaseResponse<LoginResponseModel>
        ////        //{
        ////        //    Success = false,
        ////        //    Message = "Login failed",
        ////        //    Data = null
        ////        //};
        ////    }
        ////    return returnModel;
        ////}

        //public static async Task<BaseResponse<LoginResponseModel>> Login(HttpClient httpClient, LoginRequestModel loginRequest)
        //{
        //    SessionCmsHelper _session = new SessionCmsHelper(new HttpContextAccessor());
        //    var returnModel = new BaseResponse<LoginResponseModel>();
        //    var response = await httpClient.PostAsJsonAsync("AccountManager/login", loginRequest);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
        //        returnModel.Success();
        //        returnModel.Data = loginResponse;
        //        _session.SetSession(loginResponse);

        //    }
        //    else
        //    {
        //        returnModel.Error("");
        //        returnModel.Data = null;
        //        // Handle errors (e.g., return an error response or throw an exception)
        //        //return new BaseResponse<LoginResponseModel>
        //        //{
        //        //    Success = false,
        //        //    Message = "Login failed",
        //        //    Data = null
        //        //};
        //    }
        //    return returnModel;
        //}
        //public async Task<BaseResponse<List<CategoryListModel>>> getCagetory(int lang)
        //{
        //    LoginResponseModel loginResponse = new LoginResponseModel();
        //    loginResponse = getSession();
        //    var requestUrl = $"Category/getCagetory/{lang}";
        //    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        //    var response = await _httpClient.SendAsync(request);
        //    response.EnsureSuccessStatusCode();
        //    var lst = await response.Content.ReadFromJsonAsync<BaseResponse<List<CategoryListModel>>>();
        //    return lst;
        //}
        //public async Task<BaseResponse<List<CategoryListModel>>> getCagetoryRoute()
        //{
        //    LoginResponseModel loginResponse = new LoginResponseModel();
        //    loginResponse = getSession();
        //    var requestUrl = $"Category/getCagetoryRoute";
        //    var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        //    var response = await _httpClient.SendAsync(request);
        //    response.EnsureSuccessStatusCode();
        //    var lst = await response.Content.ReadFromJsonAsync<BaseResponse<List<CategoryListModel>>>();
        //    return lst;
        //}



        //public async Task<BaseResponse<List<LanguageCultureModel>>> LanguageGetByLang(string lang)
        //{
        //    return await _httpClient.GetFromJsonAsync<BaseResponse<List<LanguageCultureModel>>>($"LanguageCulture/GetByLanguage/{lang}");
        //}
        //public async Task<BaseResponse<UserInfo>> Login(UserInfo login)
        //{
        //    var result = await _httpClient.PostAsJsonAsync("Login/CheckUser", login);
        //    return await result.Content.ReadFromJsonAsync<BaseResponse<UserInfo>>();
        //}
        //public async Task<BaseResponse<List<PersonelInfo>>> PersonelGetByLanguage(string lang)
        //{
        //    return await _httpClient.GetFromJsonAsync<BaseResponse<List<PersonelInfo>>>($"PersonelInfo/GetByLanguage/{lang}");
        //}
        //public async Task<BaseResponse<PersonelInfo>> PersonelGetById(string id)
        //{
        //   return await _httpClient.GetFromJsonAsync<BaseResponse<PersonelInfo>>($"PersonelInfo/GetById/{id}");
        //}
        //public async Task<BaseResponse<PersonelInfo>> PersonelInsert(PersonelInfo dto)
        //{

        //    var result=await _httpClient.PostAsJsonAsync("PersonelInfo/Insert", dto);
        //    if (result.StatusCode == System.Net.HttpStatusCode.BadRequest)
        //    {
        //        var body = await result.Content.ReadAsStringAsync();
        //        Console.WriteLine(body);
        //    }
        //    else
        //    {
        //        return await result.Content.ReadFromJsonAsync<BaseResponse<PersonelInfo>>();
        //    }
        //    return null;
        //}
    }
}
