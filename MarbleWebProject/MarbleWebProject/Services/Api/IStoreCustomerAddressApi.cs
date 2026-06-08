using MarbleWebProject.Models;



namespace MarbleWebProject.Services.Api;



public interface IStoreCustomerAddressApi

{

    Task<BaseResponse<List<CustomerAddressModel>>> ListAsync(string? languageCode = null, CancellationToken cancellationToken = default);

    Task<BaseResponse<CustomerAddressModel>> GetAsync(int id, string? languageCode = null, CancellationToken cancellationToken = default);

    Task<BaseResponse<CustomerAddressModel>> CreateAsync(CustomerAddressForm form, CancellationToken cancellationToken = default);

    Task<BaseResponse<CustomerAddressModel>> UpdateAsync(int id, CustomerAddressForm form, CancellationToken cancellationToken = default);

    Task<BaseResponse> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<BaseResponse<CustomerAddressModel>> SetDefaultAsync(int id, string type, CancellationToken cancellationToken = default);

}

