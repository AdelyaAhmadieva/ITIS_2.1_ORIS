namespace MyHttpServer.Configuration;

public class AppSettings
{
    public string? Address {  get; set; }
        
    public uint Port { get; set; }
        
    public string? StaticFilesPath { get; set; }
}