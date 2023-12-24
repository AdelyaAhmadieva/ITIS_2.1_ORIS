using MyHttpServer.Attributes;
using MyHttpServer.Configuration;
using MyHttpServer.Services;

namespace MyHttpServer.Controllers;

[Controller("Account")]
public class AccountControllers
{
    [Post("SendToEmail")]
    public static void SendToEmail(string email, string password)
    {
        new EmailSenderService(AppSettingsLoader.Instance()?.Configuration).SendEmail(email, password);
        Console.WriteLine("Email was sent successfully!");
    }
}