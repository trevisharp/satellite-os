using System.Text;

namespace SatelliteOS;

public class OSManager
{
    public static OSManager Current { get; private set; } = new();
    public static void Reset() 
        => Current = new();
    
    public readonly OSFolder Root;
    public OSFolder CurrentDir { get; set;}

    OSManager()
    {
        Root = new() {
            Name = ""
        };
        Root.Content.Add(new OSFolder {
            Name = "home",
            Parent = Root
        });
        var userFolder = new OSFolder {
            Name = "usr",
            Parent = Root
        };
        userFolder.Content.Add(new OSFolder {
            Name = "bin",
            Parent = userFolder
        });

        CurrentDir = Root;
    }

    public string PWD()
    {
        string path = "";
        var it = CurrentDir;
        while (it != null)
        {
            path = "/" + it.Name + path;
            it = it.Parent;
        }
        return path;
    }


}