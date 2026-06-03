using MarbleWebProject.Models;

namespace MarbleWebProject.Services.Api;

public interface IStoreRouteBootstrap
{
    Task<List<RouteListModel>> LoadRouteListAsync(CancellationToken cancellationToken = default);
}
