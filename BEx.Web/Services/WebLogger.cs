public class WebLogger : BEx.Core.ILogger
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}