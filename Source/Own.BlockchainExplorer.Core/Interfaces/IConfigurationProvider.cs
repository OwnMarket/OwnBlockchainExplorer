namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IConfigurationProvider
    {
        string GetString(string settingKey, string defaultValue = null);
        bool? GetBoolean(string settingKey, bool? defaultValue = null);
        int? GetInteger(string settingKey, int? defaultValue = null);
        decimal? GetDecimal(string settingKey, decimal? defaultValue = null);
    }
}
