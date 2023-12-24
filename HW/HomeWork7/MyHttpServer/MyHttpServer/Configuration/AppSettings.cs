namespace MyHttpServer.Configuration;

public class AppSettings
{
    public ushort Port { get; private set; }
    
    public string Address { get; private set; }
    
    public string StaticFilesPath { get; private set; }
    
    public string SmtpUsername { get; private set; }
    
    public string SmtpPassword { get; private set; }
    
    public string SmtpServer { get; private set; }
    
    public ushort SmtpPort { get; private set; }
    
    
    public AppSettings(ushort port = 0, string address = "", string staticFilesPath = "",
        string smtpUsername = "", string smtpPassword = "", string smtpServer = "", ushort smtpPort = 0)
    {
        Port = port;
        Address = address;
        StaticFilesPath = staticFilesPath;
        SmtpUsername = smtpUsername;
        SmtpPassword = smtpPassword;
        SmtpServer = smtpServer;
        SmtpPort = smtpPort;
    }
}