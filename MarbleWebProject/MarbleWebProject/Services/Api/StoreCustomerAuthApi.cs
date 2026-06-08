using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreCustomerAuthApi : IStoreCustomerAuthApi
{
    private readonly IStoreApiClient _api;

    public StoreCustomerAuthApi(IStoreApiClient api)
    {
        _api = api;
    }

    public Task<BaseResponse<CustomerAuthResult>> RegisterAsync(
        CustomerRegisterForm form,
        string languageCode,
        CancellationToken cancellationToken = default)
    {
        return _api.PostAsync<BaseResponse<CustomerAuthResult>>("/api/customers/register", new
        {
            email = form.Email,
            password = form.Password,
            firstName = form.FirstName,
            lastName = form.LastName,
            phone = form.Phone,
            customerType = form.CustomerType,
            companyName = form.CompanyName,
            taxOffice = form.TaxOffice,
            taxNumber = form.TaxNumber,
            languageID = 0
        }, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<CustomerAuthResult>> LoginAsync(
        CustomerLoginForm form,
        CancellationToken cancellationToken = default)
    {
        return _api.PostAsync<BaseResponse<CustomerAuthResult>>("/api/customers/login", new
        {
            email = form.Email,
            password = form.Password
        }, bearerToken: null, cancellationToken);
    }

    public Task<BaseResponse<CustomerProfileModel>> GetProfileAsync(
        string bearerToken,
        CancellationToken cancellationToken = default)
    {
        return _api.GetAsync<BaseResponse<CustomerProfileModel>>("/api/customers/me", bearerToken, cancellationToken);
    }

    public Task<BaseResponse<CustomerProfileModel>> UpdateProfileAsync(
        string bearerToken,
        CustomerProfileUpdateForm form,
        CancellationToken cancellationToken = default)
    {
        return _api.PutAsync<BaseResponse<CustomerProfileModel>>("/api/customers/me", new
        {
            firstName = form.FirstName,
            lastName = form.LastName,
            phone = form.Phone,
            companyName = form.CompanyName,
            taxOffice = form.TaxOffice,
            taxNumber = form.TaxNumber,
            birthDate = form.BirthDate
        }, bearerToken, cancellationToken);
    }
}
