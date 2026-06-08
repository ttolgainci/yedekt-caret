using MarbleWebProject.Models;



namespace MarbleWebProject.Services.Api;



public interface IStoreLocationApi

{

    Task<BaseResponse<List<LocationLookupItemModel>>> GetCountriesAsync(string languageCode = "tr", CancellationToken cancellationToken = default);

    Task<BaseResponse<List<LocationLookupItemModel>>> GetCitiesAsync(long countryId, string languageCode = "tr", CancellationToken cancellationToken = default);

    Task<BaseResponse<List<LocationLookupItemModel>>> GetTownsAsync(long cityId, string languageCode = "tr", CancellationToken cancellationToken = default);

}

