

using System.Net;
using System.Reflection;
using System.Text;
using MyHttpServer.Attributes;
using Newtonsoft.Json;

namespace MyHttpServer.Handlers;

public class ControllersHandler: Handler
{
    public override void HandleRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            using var response = context.Response;

            var strParams = request.Url!
                .Segments
                .Skip(1)
                .Select(s => s.Replace("/", ""))
                .ToArray();

            using var streamReader = new StreamReader(context.Request.InputStream);
            var tempOfData = streamReader.ReadToEnd();
            var formData = new[] { "" };
            if (!string.IsNullOrEmpty(tempOfData))
            {
                var currentOfUserData = tempOfData.Split('&');
                formData = new[] { WebUtility.UrlDecode(currentOfUserData[0][6..]), currentOfUserData[1][9..] };
            }

            var controllerName = strParams[0];
            var methodName = strParams[1];
            var assembly = Assembly.GetExecutingAssembly();

            var controller = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(ControllerAttribute)))
                .FirstOrDefault(c =>
                    ((ControllerAttribute)Attribute.GetCustomAttribute(c, typeof(ControllerAttribute))!)
                    .ControllerName.Equals(controllerName, StringComparison.OrdinalIgnoreCase));

            var method = (controller?.GetMethods()!)
                .FirstOrDefault(x => x.GetCustomAttributes(true)
                    .Any(attr => attr.GetType().Name.Equals($"{request.HttpMethod}Attribute",
                                     StringComparison.OrdinalIgnoreCase) &&
                                 ((HttpMethodAttribute)attr).ActionName.Equals(methodName,
                                     StringComparison.OrdinalIgnoreCase)));

            var queryParams = Array.Empty<object>();

            if (formData.Length > 1)
            {
                queryParams = method?.GetParameters()
                    .Select((p, i) => Convert.ChangeType(formData[i], p.ParameterType))
                    .ToArray();
            }

            var resultFromMethod = method?.Invoke(Activator.CreateInstance(controller!), queryParams);
            
            if (!(method!.ReturnType == typeof(void)))
                ProcessResult(resultFromMethod, response, context);
            else
                response.Redirect("http://127.0.0.1:1414/");
        }
        catch (ArgumentNullException e)
        {
            Console.WriteLine(e.Message);
        }
    }
    
    private static void ProcessResult<T>(T result, HttpListenerResponse response, HttpListenerContext context)
    {
        switch (result)
        {
            case string resultOfString:
            {
                response.ContentType = StaticFilesHandler.GetContentType(context.Request.Url!.LocalPath);
                var buffer = Encoding.UTF8.GetBytes(resultOfString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                break;
            }
            case T[] arrayOfObjects:
            {
                response.ContentType = StaticFilesHandler.GetContentType(context.Request.Url!.LocalPath);
                var json = JsonConvert.SerializeObject(arrayOfObjects, Formatting.Indented);
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                break;
            }
        }
    }
}