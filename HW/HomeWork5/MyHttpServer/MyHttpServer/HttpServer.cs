using System.Net;
using System.Text;
using MyHttpServer.Configuration;

namespace MyHttpServer;

public class HttpServer
{
    private HttpListener Listener { get; }

    private const string PathConfigFile = "../../../appsetting.json";
    
    private AppSettings Config { get; set; }

    public HttpServer(HttpListener listener)
    {
        Listener = listener;
        Config = AppSettingsLoader.Init(PathConfigFile);
        IsDirectoryExistAndCreate("../../../" + Config.StaticFilesPath);
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
                    if (localPath is "" or "/")
                        localPath = "/index.html";

                    if (!localPath.Contains('.')) continue;
                    var filePath =  "../../../" + Config.StaticFilesPath! + localPath;
                    if (File.Exists(filePath))
                    {
                        var buffer = await File.ReadAllBytesAsync(filePath);
                        response.ContentLength64 = buffer.Length;
                        response.ContentType = GetContentType(localPath);
                        await using var output = response.OutputStream;
                        await output.WriteAsync(buffer);
                        await output.FlushAsync();
                    }
                    else
                    {
                        Console.WriteLine($"Файл {localPath} не найден");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        const string error404 = "<h2>Ошибка 404</h2><h3>Файл не найден</h3>";
                        var notFoundBuffer = Encoding.UTF8.GetBytes(error404);
                        response.ContentLength64 = notFoundBuffer.Length;
                        response.ContentType = "text/html; charset=utf-8";
                        await using var output = response.OutputStream;
                        await output.WriteAsync(notFoundBuffer);
                        await output.FlushAsync();
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
    
    public static string GetContentType(string requestUrl)
    {
        string contentType;

        switch (Path.GetExtension(requestUrl).ToLower())
        {
            case ".html":
                contentType = "text/html; charset=utf-8";
                break;
            case ".png":
                contentType = "image/png";
                break;
            case ".svg":
                contentType = "image/svg+xml";
                break;
            case ".css":
                contentType = "text/css";
                break;
            default:
                contentType = "text/plain; charset=utf-8";
                break;
        }

        return contentType;
    }
    
    void IsDirectoryExistAndCreate(string path)
    {
        if (Directory.Exists(path))
            return;


        Directory.CreateDirectory(path);
    }
}