using System.Collections.Concurrent;

public static class OS
{
    internal static ConcurrentQueue<string> Buffer = [];
    public static void WriteLine(object data)
    {
        var message = data.ToString();
        foreach (var item in message.Split("\n"))
            Buffer.Enqueue(item);
    }
}