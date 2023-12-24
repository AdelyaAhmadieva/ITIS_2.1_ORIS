using System.Net;
using System.Text;
using MyHttpServer.Configuration;

namespace MyHttpServer;

public class HttpServer
{
    private HttpListener Listener { get; set; }

    private const string PathConfigFile = "../../../appsetting.json";
    
    private AppSettings Config { get; set; }

    public HttpServer(HttpListener listener)
    {
        Listener = listener;
        Config = AppSettingsLoader.Init(PathConfigFile);
    }

    public async Task Start()
    {
        try
        {
            
            Listener.Prefixes.Add(Config.Address + ":" + Config.Port + "/");
            Listener.Start();
            Console.WriteLine("Server started");
        
            Task.Run(async () =>
            {
                while (Listener.IsListening)
                {
                    var context = await Listener.GetContextAsync();
                    var response = context.Response;
                    var localPath = context.Request.Url!.LocalPath;
                    if (localPath.Equals("/") || !localPath.EndsWith(".html"))
                    {
                        const string fileName = "index.html";
                        if (Directory.Exists(Config.StaticFilesPath))
                        {
                            var filePath = Path.Combine(Config.StaticFilesPath!, fileName);
                            if (File.Exists(filePath))
                            {
                                var buffer = await File.ReadAllBytesAsync(filePath);
                                await using var output = response.OutputStream;
                                await output.WriteAsync(buffer);
                                await output.FlushAsync();
                            }
                            else
                            {
                                Console.WriteLine($"Файл {fileName} не найден");
                                const string error404 = "<h2>Ошибка 404</h2><h3>Файл не найден</h3>";
                                response.ContentType = "text/html; charset=utf-8";
                                var buffer = Encoding.UTF8.GetBytes(error404);
                                await using var output = response.OutputStream;
                                await output.WriteAsync(buffer);
                                await output.FlushAsync();
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(Config.StaticFilesPath!);
                            Console.WriteLine($"Была создана папка {Config.StaticFilesPath}");
                        }
                    }
                    else
                    {
                        var directory = "../../../" + localPath.Split("/")[1];
                        var file = localPath.Split("/")[2];
                        if (Directory.Exists(directory))
                        {
                            var filePath = Path.Combine(directory, file);
                            if (File.Exists(filePath))
                            {
                                var buffer = await File.ReadAllBytesAsync(filePath);
                                await using var output = response.OutputStream;
                                await output.WriteAsync(buffer);
                                await output.FlushAsync();
                            }
                            else
                            {
                                Console.WriteLine($"Файл {file} не найден");
                                const string error404 = "<h2>Ошибка 404</h2><h3>Файл не найден</h3>";
                                response.ContentType = "text/html; charset=utf-8";
                                var buffer = Encoding.UTF8.GetBytes(error404);
                                await using var output = response.OutputStream;
                                await output.WriteAsync(buffer);
                                await output.FlushAsync();
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Папка {directory[9..]} не существует");
                            const string error404 = "<h2>Ошибка 404</h2><h3>Папка не найдена</h3>";
                            response.ContentType = "text/html; charset=utf-8";
                            var buffer = Encoding.UTF8.GetBytes(error404);
                            await using var output = response.OutputStream;
                            await output.WriteAsync(buffer);
                            await output.FlushAsync();
                        }
                    }
                }
            });
        
            Console.WriteLine("Write 'stop' to stop the server.");
        
            await Task.Run(() =>
            {
                while (true)
                    if (Console.ReadLine()!.Equals("stop"))
                        break;
            });
        
            Listener.Stop();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        finally
        {
            Console.WriteLine("Работа сервера завершена");
        }
    }
}