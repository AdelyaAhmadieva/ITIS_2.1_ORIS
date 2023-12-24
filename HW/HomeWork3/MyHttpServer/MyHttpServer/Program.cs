using MyHttpServer.Configuration;
using System.Net;
using System.Text.Json;

const string pathConfigFile = "../../../appsetting.json";
HttpListener listener = new();

try
{
    if (!File.Exists(pathConfigFile))
    { 
        Console.WriteLine("Файл appsetting.json не был найден");
        throw new Exception();
    }

    AppSettings? config;

    using (var file = File.OpenRead(pathConfigFile)) 
        config = JsonSerializer.Deserialize<AppSettings>(file);

    listener.Prefixes.Add(config!.Address + ":" + config.Port + "/");
    listener.Start();
    Console.WriteLine("Server started");

    Task.Run(() =>
    {
        while (listener.IsListening)
        {
            var context = listener.GetContext();
            var response = context.Response;
            const string filePath = "../../../index.html";
            var buffer = File.ReadAllBytes(filePath);
            using var output = response.OutputStream;
            output.Write(buffer);
            output.Flush();
        }
    });

    Console.WriteLine("Write 'stop' to stop the server.");

    await Task.Run(() =>
    {
        while (true)
            if (Console.ReadLine()!.Equals("stop"))
                break;
    });
    listener.Stop();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}
finally
{
    Console.WriteLine("Работа сервера завершена");
}