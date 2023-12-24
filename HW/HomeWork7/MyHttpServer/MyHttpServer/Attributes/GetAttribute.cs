namespace MyHttpServer.Attributes;

public class GetAttribute : HttpMethodAttribute
{
    public GetAttribute(string actionName) : base(actionName)
    {
    }
}