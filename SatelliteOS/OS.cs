using System.Threading;
using System.Collections.Concurrent;
using System;
using SatelliteOS;

public static class OS
{
    internal static ConcurrentDictionary<int, int> TasksToKill = [];
    internal static ConcurrentQueue<string> Buffer = [];
    internal static readonly float[] Sensors = new float[6];
    internal static readonly float[] Actuators = new float[4];
    
    internal static void TryKill()
    {
        var id = Environment.CurrentManagedThreadId;
        if (!TasksToKill.TryRemove(id, out var pid))
            return;
        
        OSTask.RestorePID(pid);
        throw new Exception("Task Finished");
    }
    
    public static void WriteLine(object data)
    {
        var message = data.ToString();
        foreach (var item in message.Split("\n"))
            Buffer.Enqueue(item);
        TryKill();
    }

    public static void Sleep(int millis)
    {
        Thread.Sleep(millis);
        TryKill();
    }
    
    public static float GetSensor(int address)
    {
        var sensorValue = Sensors[address];
        TryKill();
        return sensorValue;
    }

    public static void SetActuator(int address, float value)
    {
        Actuators[address] = value;
        TryKill();
    }
}