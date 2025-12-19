using Microsoft.Extensions.Configuration;
using Task1.Models;

namespace Task2.Provider;

public class CustomConfigurationProvider : ConfigurationProvider
{
    public void OnConfigurationUpdated(IEnumerable<ConfigurationItem> configurationItems)
    {
        var newData = configurationItems.ToDictionary(
            item => item.Key,
            item => (string?)item.Value);

        if (HasChanged(newData))
        {
            UpdateNewData(newData);
            OnReload();
        }
    }

    private bool HasChanged(Dictionary<string, string?> newData)
    {
        if (Data.Count != newData.Count)
        {
            UpdateNewData(newData);
            return true;
        }

        foreach (KeyValuePair<string, string?> kv in Data)
        {
            if (!newData.TryGetValue(kv.Key, out string? newValue) || !EqualityComparer<string?>.Default.Equals(kv.Value, newValue))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateNewData(Dictionary<string, string?> newData)
    {
        Data.Clear();
        foreach (KeyValuePair<string, string?> keyValuePair in newData)
        {
            Data[keyValuePair.Key] = keyValuePair.Value;
        }
    }
}