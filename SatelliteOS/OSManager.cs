using System.Collections.Generic;
using System.Linq;
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
        Root.Content.Add(userFolder);
        userFolder.Content.Add(new OSFolder {
            Name = "bin",
            Parent = userFolder
        });

        CurrentDir = Root;
    }

    public string PWD()
        => CurrentDir.FullPath;

    public string[] LS()
    {
        List<string> result = [];
        var items = CurrentDir.Content
            .OrderBy(c => c.GetType().Name)
            .ThenBy(c => c.Name);
        foreach (var item in items)
            result.Add(item.ItemName);
        return [ ..result ];
    }

    public string CD(string path)
    {
        if (path == "..")
        {
            if (CurrentDir.Parent is null)
                return $"'..' does not exist in this directory.";

            CurrentDir = CurrentDir.Parent;
            return "";
        }
        var item = CurrentDir.Content.FirstOrDefault(
            i => i.Name == path
        );
        if (item is null)
            return $"'{path}' does not exist in this directory.";
        
        if (item is not OSFolder folder)
            return $"'{path}' is a file and cannot be oppened.";
        
        CurrentDir = folder;
        return "";
    }

    public string MKDIR(string name)
    {
        var item = CurrentDir.Content.FirstOrDefault(
            i => i.Name == name
        );
        if (item is not null)
            return $"A item named '{name}' already exists in this directory.";
        
        var newFolder = new OSFolder {
            Name = name,
            Parent = CurrentDir
        };
        CurrentDir.Content.Add(newFolder);
        return "";
    }

    public string RM(string name)
    {
        var item = CurrentDir.Content.FirstOrDefault(
            i => i.Name == name
        );
        if (item is null)
            return $"'{name}' does not exist in this directory.";

        var parent = item.Parent;
        parent.Content.Remove(item);
        return "";
    }

    public string TOUCH(string name)
    {
        var parts = name.Split(".");
        if (parts.Length == 1)
            return "A file needs name and extension.";
        
        var fileName = parts[0];
        var extension = parts[1];

        var item = CurrentDir.Content.FirstOrDefault(
            i => i.Name == name
        );
        if (item is not null)
            return $"A item named '{name}' already exists in this directory.";

        var file = new OSFile {
            Content = "",
            Name = fileName,
            Extension = extension,
            Parent = CurrentDir
        };
        CurrentDir.Content.Add(file);
        return "";
    }

    public string[] CAT(string name)
    {
        var parts = name.Split(".");
        if (parts.Length == 1)
            return [ "A file needs name and extension." ];
        
        var fileName = parts[0];
        var extension = parts[1];

        var item = CurrentDir.Content.FirstOrDefault(
            i => i is OSFile f && f.ItemName == name
        );
        if (item is null)
            return [ $"'{name}' does not exist in this directory." ];

        if (item is not OSFile file)
            return [ $"'{name}' need be a file." ];

        return file.Content.Split("\n");
    }

    public string ECHO(string content, string symbol, string name)
    {
        var parts = name.Split(".");
        if (parts.Length == 1)
            return "A file needs name and extension.";
        
        var fileName = parts[0];
        var extension = parts[1];

        var item = CurrentDir.Content.FirstOrDefault(
            i => i is OSFile f && f.ItemName == name
        );
        if (item is null)
            return $"'{name}' does not exist in this directory.";

        if (item is not OSFile file)
            return $"'{name}' need be a file.";
        
        if (symbol is not ">>" and not ">")
            return $"unknown symbol '{symbol}'.";
        
        if (symbol == ">")
            file.Content = content;
        else file.Content += content;
        return "";
    }
}