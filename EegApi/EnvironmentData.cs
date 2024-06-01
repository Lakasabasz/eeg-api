namespace EegApi;

public static class EnvironmentData
{
    public static string DbHost => Environment.GetEnvironmentVariable("DB_HOST") ?? "db.local";
    public static string DbPort => Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
    public static string DbName => Environment.GetEnvironmentVariable("DB_NAME") ?? "Eeg";
    public static string DbUser => Environment.GetEnvironmentVariable("DB_USER") ?? "root";
    public static string DbPassword => Environment.GetEnvironmentVariable("DB_PWD") ?? "root";
}