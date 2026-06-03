using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public sealed class StoreContentApi : IStoreContentApi
{
    private readonly IStoreApiClient _api;
    private readonly IStoreAuthService _auth;

    public StoreContentApi(IStoreApiClient api, IStoreAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<BaseResponse<TranslateResponse>> GetTranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<TranslateResponse>>("/Language/getTranslate", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<TranslateAllResponse>>> GetTranslateAllAsync(CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<TranslateAllResponse>>>("/Language/getTranslateAll", new { }, token, cancellationToken);
    }

    public async Task<BaseResponse<List<LanguageCultureResponse>>> GetLanguageCultureAsync(CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<LanguageCultureResponse>>>("/Language/getLanguageCulture", new { }, token, cancellationToken);
    }

    public async Task<BaseResponse<List<AllBannerResponse>>> GetBannerAllAsync(BannerAllRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<AllBannerResponse>>>("/Banner/GetBannerAll", request, token, cancellationToken);
    }

    public async Task<BaseResponse<AllInfoResponse>> GetInfoByUrlAsync(InfoPageRequest request, CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<AllInfoResponse>>("/Information/GetInfoByUrl", request, token, cancellationToken);
    }

    public async Task<BaseResponse<List<InformationRouteModel>>> GetInformationForRouteAsync(CancellationToken cancellationToken = default)
    {
        var token = await _auth.GetTokenAsync(cancellationToken);
        return await _api.PostAsync<BaseResponse<List<InformationRouteModel>>>("/Information/GetInformationForRoute", new { }, token, cancellationToken);
    }
}
