using System.Net;
using System.Net.Mail;
using MyHttpServer.Configuration;
using MyHttpServer.services;

namespace MyHttpServer.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly string _smtpServer;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly ushort _smtpPort;

    public EmailSenderService(AppSettings? config)
    {
        _smtpUsername = config!.SmtpUsername;
        _smtpPassword = config.SmtpPassword;
        _smtpServer = config.SmtpServer;
        _smtpPort = config.Port;
    }

    public void SendEmail(string login, string password)
    {
        var from = new MailAddress(_smtpUsername, "BattleNet");
        var to = new MailAddress(login);
        var message = new MailMessage(from, to);
        message.Subject = "BattleNet Login Details";
        message.Body = $"Login: {login}\nPassword: {WebUtility.HtmlDecode(password)}";
        // message.Attachments.Add(new Attachment("../../../MyHttpServer.rar"));
        var smtpClient = new SmtpClient(_smtpServer);
        smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
        smtpClient.EnableSsl = true;
        smtpClient.Send(message);
        smtpClient.Dispose();
    }
}