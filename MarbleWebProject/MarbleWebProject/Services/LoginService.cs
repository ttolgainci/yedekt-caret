using MarbleWebProject.Models;
using Microsoft.AspNetCore.Identity.Data;
using System.Net.Http;

namespace MarbleWebProject.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        //public async Task<LoginResponseModel> LoginAsync(LoginRequestModel loginRequest)
        //{
        //    var returnModel =new  LoginResponseModel();
        //    var response = await _httpClient.PostAsJsonAsync("AccountManager/login", loginRequest);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseModel>();
        //        returnModel.Token=loginResponse.Token;
        //    }
        //    else
        //    {
        //        returnModel.Token = null;
        //    }
        //    return returnModel;
        //}
    }
}
