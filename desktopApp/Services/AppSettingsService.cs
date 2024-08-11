using System.Net.Http;

namespace StarChatDesktopApp.Services;

public class AppSettingsService
{
    private static AppSettingsService _instance;
    public static AppSettingsService Instance => _instance ??= new AppSettingsService();
    
    public string ApiUrl { get; set; }
    public string AuthToken { get; set; }
    public HttpClient HttpClient { get; private set; }

    private AppSettingsService()
    {
        // Api url is hard coded, so that we don't have to copy any files over and put them on the
        // users computers. Just the exe file will be enough.
        ApiUrl = "YourApiUrl";
        AuthToken = string.Empty;
        HttpClient = new HttpClient();
    }
}
