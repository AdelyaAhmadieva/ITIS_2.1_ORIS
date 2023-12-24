using System.Text.Json;

namespace MyHttpServer.Configuration;

public class AppSettingsLoader
{
    private string Path { get; set; }
    public AppSettings? Configuration { get; private set; }

    private static bool _isInitialized;
    
    private static AppSettingsLoader? _instance;

    public const string CurrentDirectory = "../../../";

    public AppSettingsLoader() => Path = $"{CurrentDirectory}appsettings.json";

    private AppSettingsLoader(string path, AppSettings config)
    {
        Path = path;
        Configuration = config;
    }
    public void Init()
    {
        try
        {
            var json = File.ReadAllText(Path);
            Configuration = JsonSerializer.Deserialize<AppSettings>(json);
            _isInitialized = true;
            _instance = new AppSettingsLoader(Path, Configuration!);
        }
        
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (!File.Exists(Path))
            throw new ArgumentException("appsetting.json не найден");
    }
    
    public static AppSettingsLoader? Instance()
    {
        if (_isInitialized)
            return _instance;
        throw new InvalidOperationException("DataServer Singleton is not initialized");
    }
}