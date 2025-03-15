
using System.ComponentModel;

public static class Settings
{
    private const string USER_SECRET_ID = "Lilith.Backend";

    private static ConfigurationManager? ConfigurationManager;
    public static string ConnectionString = string.Empty;
    public static string JwtSecret = string.Empty;
    public static TimeSpan JwtExpirationTime;
    public static string FileUploadPath = string.Empty;
    public static long FileUploadLimitSize;

    public static void Load(ConfigurationManager configurationManager)
    {
        ConfigurationManager = configurationManager;
        ConfigurationManager.AddUserSecrets(USER_SECRET_ID).AddEnvironmentVariables();

        ConnectionString = GetConfigurationKey<string>("ConnectionStrings:Default");
        JwtSecret = GetConfigurationKey<string>("JwtConfig:Secret");
        JwtExpirationTime = GetConfigurationKey<TimeSpan>("JwtConfig:ExpirationTimeFrame");
        FileUploadPath = GetConfigurationKey<string>("FileManagment:UploadPath");
        FileUploadLimitSize = GetConfigurationKey<long>("FileManagment:LimitSize", 200) * 1024L * 1024L;
    }

    private static T GetConfigurationKey<T>(string key)
    {
        var configValue = ConfigurationManager?.GetSection(key).Value;
        if (configValue is null)
        {
            throw new Exception($"{key} configuration key is required");
        }

        // Podemos usar TypeDescriptor para cubrir más tipos (TimeSpan, int, etc.)
        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null && converter.CanConvertFrom(typeof(string)))
        {
            return (T)converter.ConvertFromInvariantString(configValue)!;
        }

        // Fallback: si no hay un conversor, usamos Convert.ChangeType (para casos básicos)
        return (T)Convert.ChangeType(configValue, typeof(T));
    }

    private static T GetConfigurationKey<T>(string key, T defaultValue)
    {
        var configValue = ConfigurationManager?.GetSection(key).Value;
        if (string.IsNullOrWhiteSpace(configValue))
        {
            return defaultValue;
        }

        var converter = TypeDescriptor.GetConverter(typeof(T));
        if (converter != null && converter.CanConvertFrom(typeof(string)))
        {
            return (T)converter.ConvertFromInvariantString(configValue)!;
        }

        return (T)Convert.ChangeType(configValue, typeof(T));
    }
}