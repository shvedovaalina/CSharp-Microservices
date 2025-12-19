using Microsoft.Extensions.Configuration;
using Task2.Provider;

namespace Task2.Sources;

public class CustomConfigurationSource : IConfigurationSource
{
    private readonly CustomConfigurationProvider _provider;

    public CustomConfigurationSource(CustomConfigurationProvider provider)
    {
        _provider = provider;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return _provider;
    }
}