public static class Server
{
    //config properties
    public static string Version { get; set; } = "1.0";
    public static string[] ServicePaths { get; set; } = new string[] { "api" };
    public static int BcryptWorkfactor { get; set; } = 10;
    public static string Salt { get; set; } = "";
    public static bool HasAdmin { get; set; } = false; //no admin account exists
    public static bool ResetPass { get; set; } = false; //force admin to reset password
    public static bool IsDocker { get; set; } //true if running in Docker container or in Linux
    public static List<Saber.Models.ApiKey> DeveloperKeys { get; set; } = new List<Saber.Models.ApiKey>(); //used for public APIs
    public static IConfiguration Config { get; set; }
    public static IHostApplicationLifetime AppLifetime { get; set; }
    public static string SaberCmsHost { get; set; } = "https://www.sabercms.com/";
    public static bool RunTests { get; set; } = false;
}

public class WinService : BackgroundService
{
    public WinService(ILoggerFactory loggerFactory)
    {
        Logger = loggerFactory.CreateLogger<WinService>();
    }

    public ILogger Logger { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Saber is starting.");

        stoppingToken.Register(() => Logger.LogInformation("Saber is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1000), stoppingToken);
        }

        Logger.LogInformation("Saber has stopped.");
    }
}
