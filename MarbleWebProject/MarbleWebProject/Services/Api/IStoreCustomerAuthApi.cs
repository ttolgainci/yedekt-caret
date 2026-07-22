using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreCustomerAuthApi
{
    Task<BaseResponse<CustomerAuthResult>> RegisterAsync(CustomerRegisterForm form, string languageCode, CancellationToken cancellationToken = default);
    Task<BaseResponse<CustomerAuthResult>> LoginAsync(CustomerLoginForm form, CancellationToken cancellationToken = default);
    Task<BaseResponse<CustomerProfileModel>> GetProfileAsync(string bearerToken, CancellationToken cancellationToken = default);
    Task<BaseResponse<CustomerProfileModel>> UpdateProfileAsync(string bearerToken, CustomerProfileUpdateForm form, CancellationToken cancellationToken = default);
    Task<BaseResponse> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);
    Task<BaseResponse> ConfirmPasswordResetAsync(string token, string newPassword, CancellationToken cancellationToken = default);
    Task<BaseResponse> VerifyEmailAsync(string token, CancellationToken cancellationToken = default);
}
