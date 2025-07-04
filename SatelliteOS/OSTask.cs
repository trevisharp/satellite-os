using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SatelliteOS;

internal class OSTask
{
    public static IEnumerable<OSTask> Tasks => tasks.Values;
    static readonly List<int> PIDS = [.. Enumerable.Range(1000, 8999)];

    static readonly Dictionary<int, OSTask> tasks = [];

    public int PID { get; set; }
    public string ProcessName { get; set; }
    public Thread Thread { get; set; }

    public static string[] New(OSFile file, bool inBackgorund, params string[] args)
    {
        var bin = file.Content;
        var code = OSManager.Decript(bin);
        
        if (code is null)
            return [ "the selected file is not a executable." ];

        if (inBackgorund)
        {
            int randIndex = Random.Shared.Next(PIDS.Count);
            var randPID = PIDS[randIndex];
            PIDS.RemoveAt(randIndex);
            
            var thread = new Thread(() =>
            {
                try
                {
                    var compiler = new Compiler();
                    var result = compiler.GetNewAssembly([ code ], []);
                    foreach (var message in result.Item2)
                        OS.WriteLine(message);
                    if (result.Item1 is null)
                        return;
                    result.Item1.EntryPoint.Invoke(null, [ args ]);
                }
                catch (Exception ex)
                {
                    var message = ex.InnerException?.Message;
                    if (message is null || message is "Task Finished")
                        return;
                    OS.WriteLine($"'{message}' error on task {randPID}.");
                }
                finally
                {
                    tasks.Remove(randPID);
                }
            });

            var task = new OSTask {
                PID = randPID,
                ProcessName = file.Name,
                Thread = thread
            };
            tasks.Add(task.PID, task);
            thread.Start();
        }
        else
        {
            try
            {
                var compiler = new Compiler();
                var assembly = compiler.GetNewAssembly([ code ], []);
                if (assembly.Item1 is null)
                    return [ "The executable file has erros." ];
                assembly.Item1.EntryPoint.Invoke(null, [ args ]);
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message;
                return [ message ];
            }
        }
        return [ "" ];
    }
    
    public static void InterrupAll()
    {
        foreach (var task in tasks.Values)
            task.Thread.Interrupt();
    }

    public static string Kill(int pid)
    {
        if (!tasks.TryGetValue(pid, out var task))
            return $"unknown PID '{pid}'.";
        
        OS.TasksToKill.TryAdd(task.Thread.ManagedThreadId, pid);
        return "";
    }

    public static void RestorePID(int pid)
        => PIDS.Add(pid);
}