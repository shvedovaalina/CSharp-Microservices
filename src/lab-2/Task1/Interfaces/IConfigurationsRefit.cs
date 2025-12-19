using Refit;
using Task1.Dto;

namespace Task1.Interfaces;

public interface IConfigurationsRefit
{
    [Get("/configurations")]
    Task<ConfigurationResponseDto> GetConfigurationsAsync(
        [Query] string? pageToken,
        [Query] int pageSize,
        CancellationToken cancellationToken);
}