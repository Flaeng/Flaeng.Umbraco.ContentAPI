namespace Flaeng.Umbraco.ContentAPI.Handlers;

public class Request
{
    public string ContentType { get; set; }
    public object ContentId { get; set; }
}
public interface IRequestInterpreter
{
    Request Interprete(string localPath);
}
public class DefaultRequestInterpreter : IRequestInterpreter
{
    public Request Interprete(string localPath)
    {
        throw new System.NotImplementedException();
    }
}