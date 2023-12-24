using System.Net;
using System.Text;
using System.Web;
using MyHttpServer.Configuration;
using MyHttpServer.Services;

namespace MyHttpServer;

public class StaticFilesHandler
{
    public async Task HandleRequest(HttpListenerContext context)
    {
        var config = AppSettingsLoader.Instance();
        IsDirectoryExistAndCreate("../../../" + config!.Configuration!.StaticFilesPath);
        var response = context.Response;
        var request = context.Request;
        var localPath = request.Url!.LocalPath;
        if (localPath is "" or "/")
            localPath = "/index.html";

        if (localPath.Contains('.'))
        {
            var filePath = "../../../" + config.Configuration.StaticFilesPath + localPath;
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
        else if (request.HttpMethod == "POST" && request.Url.LocalPath.Equals("/send-email"))
        {
            using var reader = new StreamReader(request.InputStream);
            var streamRead = await reader.ReadToEndAsync();
            var decodedData = HttpUtility.UrlDecode(streamRead, Encoding.UTF8);

            var str = decodedData.Split("&");

            var emailSender = new EmailSenderService(config.Configuration);
            emailSender.SendEmail(str[0].Split("=")[1], str[1].Split("=")[1]);

            Console.WriteLine("Email sent successfully!");

            response.RedirectLocation = "/";
            response.StatusCode = (int)HttpStatusCode.Redirect;
            response.Close();
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