using Task2.Provider;
using Task2.Sources;

namespace Presentation.Extensions;

public static class ConfigurationExtension
{
    public static IConfigurationBuilder AddJCustomConfiguration(
        this IConfigurationBuilder builder, CustomConfigurationProvider provider)
    {
        var source = new CustomConfigurationSource(provider);
        return builder.Add(source);
    }
}