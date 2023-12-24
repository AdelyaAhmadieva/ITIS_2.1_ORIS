using System.Text.Json;

namespace MyHttpServer.Configuration;

public class AppSettingsLoader
{
    public static AppSettings Init(string pathConfigFile)
    {
        if (!File.Exists(pathConfigFile))
        { 
            Console.WriteLine("Файл appsetting.json не был найден");
            throw new Exception();
        }

        using var file = File.OpenRead(pathConfigFile);
        var config = JsonSerializer.Deserialize<AppSettings>(file);
        return config!;
    }
}