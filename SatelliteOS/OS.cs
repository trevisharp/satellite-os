using System.Threading;
using System.Collections.Concurrent;

public static class OS
{
    internal static ConcurrentQueue<string> Buffer = [];
    internal static readonly float[] Sensors = new float[6];
    internal static readonly float[] Actuators = new float[4];
    public static void WriteLine(object data)
    {
        var message = data.ToString();
        foreach (var item in message.Split("\n"))
            Buffer.Enqueue(item);
    }

    public static void Sleep(int millis)
        => Thread.Sleep(millis);
    
    public static float GetSensor(int address)
        => Sensors[address];

    public static void SetActuator(int address, float value)
        => Actuators[address] = value;
}