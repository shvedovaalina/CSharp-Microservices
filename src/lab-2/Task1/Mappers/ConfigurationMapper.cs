using Task1.Dto;
using Task1.Models;

namespace Task1.Mappers;

public static class ConfigurationMapper
{
    public static ConfigurationItem ToModels(this ConfigurationItemDto dto)
    {
        string key = dto.Key ?? throw new NullReferenceException();
        string value = dto.Value ?? throw new NullReferenceException();

        return new ConfigurationItem(key, value);
    }

    public static IEnumerable<ConfigurationItem> ToModels(this IEnumerable<ConfigurationItemDto>? items)
    {
        return items?.Select(i => i.ToModels()) ?? Enumerable.Empty<ConfigurationItem>();
    }
}