using Task1.Models;

namespace Task1.Interfaces;

public interface IConfigurationClient
{
    IAsyncEnumerable<ConfigurationItem> GetAllConfigurationsAsync(CancellationToken cancellationToken);
}