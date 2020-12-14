using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

public static class Server
{
    //config properties
    public static IConfiguration Config { get; set; }
    public static string[] ServicePaths { get; set; } = new string[] { "api" };
    public static int BcryptWorkfactor { get; set; } = 10;
    public static string Salt { get; set; } = "";
    public static bool HasAdmin { get; set; } = false; //no admin account exists
    public static bool ResetPass { get; set; } = false; //force admin to reset password
    public static string Version { get; set; } = "1.0";
    public static IHostApplicationLifetime AppLifetime { get; set; }

    //other settings
    public static bool IsDocker { get; set; }
}
