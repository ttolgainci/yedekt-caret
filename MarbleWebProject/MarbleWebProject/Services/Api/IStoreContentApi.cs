using MarbleWebProject.Models;
using MarbleWebProject.Services;

namespace MarbleWebProject.Services.Api;

public interface IStoreContentApi
{
    Task<BaseResponse<TranslateResponse>> GetTranslateAsync(TranslateRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<TranslateAllResponse>>> GetTranslateAllAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<LanguageCultureResponse>>> GetLanguageCultureAsync(CancellationToken cancellationToken = default);
    Task<BaseResponse<List<AllBannerResponse>>> GetBannerAllAsync(BannerAllRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<AllInfoResponse>> GetInfoByUrlAsync(InfoPageRequest request, CancellationToken cancellationToken = default);
    Task<BaseResponse<List<InformationRouteModel>>> GetInformationForRouteAsync(CancellationToken cancellationToken = default);
}
