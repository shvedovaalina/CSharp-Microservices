using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using Task1.Dto;
using Task1.Interfaces;
using Task1.Mappers;
using Task1.Models;
using Task1.Options;

namespace Task1.Services;

public class RefitConfigurationClient : IConfigurationClient
{
    private readonly IConfigurationsRefit _configurationsRefit;
    private readonly ConfigurationClientOptions _options;

    public RefitConfigurationClient(
        IConfigurationsRefit configurationsRefit,
        IOptions<ConfigurationClientOptions> options)
    {
        _configurationsRefit = configurationsRefit;
        _options = options.Value;
    }

    public async IAsyncEnumerable<ConfigurationItem> GetAllConfigurationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? pageToken = null;

        do
        {
            int pageSize = _options.PageSize;
            cancellationToken.ThrowIfCancellationRequested();

            ConfigurationResponseDto response =
                await _configurationsRefit.GetConfigurationsAsync(pageToken, pageSize, cancellationToken);

            foreach (ConfigurationItem item in response.Items.ToModels())
            {
                yield return item;
            }

            pageToken = response.PageToken;
        }
        while (pageToken != null);
    }
}