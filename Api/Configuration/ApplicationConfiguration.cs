
public static class ApplicationConfiguration
{
    private const string USER_SECRET_ID = "Lilith.Backend";

    private static ConfigurationManager? ConfigurationManager;
    public static string ConnectionString = string.Empty;
    public static string JwtSecret = string.Empty;
    public static TimeSpan JwtExpirationTime;

    public static void LoadConfiguration(ConfigurationManager configurationManager)
    {
        ConfigurationManager = configurationManager;
        ConfigurationManager.AddUserSecrets(USER_SECRET_ID).AddEnvironmentVariables();
        
        ConnectionString = GetConfigurationKey("ConnectionStrings:Default");
        JwtSecret = GetConfigurationKey("JwtConfig:Secret");
        JwtExpirationTime = TimeSpan.Parse(GetConfigurationKey("JwtConfig:ExpirationTimeFrame"));
    }

    private static string GetConfigurationKey(string Key)
    {
        var configurationKey = ConfigurationManager is not null ? ConfigurationManager.GetSection(Key).Value : null;
        if (configurationKey is null)
        {
            throw new Exception($"{Key} configuration key is required");
        }
        return configurationKey;
    }
}