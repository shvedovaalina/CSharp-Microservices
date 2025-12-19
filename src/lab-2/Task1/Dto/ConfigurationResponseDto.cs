using System.Text.Json.Serialization;

namespace Task1.Dto;

public record ConfigurationResponseDto(
    [property: JsonPropertyName("items")] IEnumerable<ConfigurationItemDto> Items,
    [property: JsonPropertyName("pageToken")]
    string? PageToken);