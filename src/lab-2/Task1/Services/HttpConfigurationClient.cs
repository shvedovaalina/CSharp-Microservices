using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Task1.Dto;
using Task1.Interfaces;
using Task1.Mappers;
using Task1.Models;
using Task1.Options;

namespace Task1.Services;

public class HttpConfigurationClient : IConfigurationClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ConfigurationClientOptions _options;

    public HttpConfigurationClient(IHttpClientFactory clientFactory, IOptions<ConfigurationClientOptions> options)
    {
        _clientFactory = clientFactory;
        _options = options.Value;
    }

    public async IAsyncEnumerable<ConfigurationItem> GetAllConfigurationsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using HttpClient httpClient = _clientFactory.CreateClient();

        string? pageToken = null;
        do
        {
            int pageSize = _options.PageSize;
            cancellationToken.ThrowIfCancellationRequested();
            string requestUrl = $"/configurations?pageSize={pageSize}";
            if (pageToken is not null)
            {
                requestUrl += $"&pageToken={pageToken}";
            }

            using var message = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            HttpResponseMessage response = await httpClient.SendAsync(message);

            ConfigurationResponseDto? configurationResponse =
                await response.Content.ReadFromJsonAsync<ConfigurationResponseDto>(cancellationToken);

            if (configurationResponse is not null)
            {
                foreach (ConfigurationItem item in configurationResponse.Items.ToModels())
                {
                    yield return item;
                }
            }

            pageToken = configurationResponse?.PageToken;
        }
        while (pageToken != null);
    }
}