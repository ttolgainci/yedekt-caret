using MarbleWebProject.Models;



namespace MarbleWebProject.Services.Api;



public sealed class StoreLocationApi : IStoreLocationApi

{

    private readonly IStoreApiClient _api;



    public StoreLocationApi(IStoreApiClient api)

    {

        _api = api;

    }



    public Task<BaseResponse<List<LocationLookupItemModel>>> GetCountriesAsync(string languageCode = "tr", CancellationToken cancellationToken = default)

    {

        return _api.GetAsync<BaseResponse<List<LocationLookupItemModel>>>(

            $"/api/locations/countries?languageCode={Uri.EscapeDataString(languageCode)}",

            bearerToken: null,

            cancellationToken);

    }



    public Task<BaseResponse<List<LocationLookupItemModel>>> GetCitiesAsync(long countryId, string languageCode = "tr", CancellationToken cancellationToken = default)

    {

        return _api.GetAsync<BaseResponse<List<LocationLookupItemModel>>>(

            $"/api/locations/cities?countryId={countryId}&languageCode={Uri.EscapeDataString(languageCode)}",

            bearerToken: null,

            cancellationToken);

    }



    public Task<BaseResponse<List<LocationLookupItemModel>>> GetTownsAsync(long cityId, string languageCode = "tr", CancellationToken cancellationToken = default)

    {

        return _api.GetAsync<BaseResponse<List<LocationLookupItemModel>>>(

            $"/api/locations/towns?cityId={cityId}&languageCode={Uri.EscapeDataString(languageCode)}",

            bearerToken: null,

            cancellationToken);

    }

}

