using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SatelliteOS;

internal class OSManager
{
    public static OSManager Current { get; private set; } = new();
    public static void Reset() 
        => Current = new();
    public static void Save()
    {
        var json = JsonSerializer.Serialize(Current);
        byte[] Key = Encoding.UTF8.GetBytes("minha-chave-secreta1234567890124");
        byte[] IV  = Encoding.UTF8.GetBytes("vetor-inicial-12");
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(json);
        sw.Close();
        var final = Convert.ToBase64String(ms.ToArray());
        File.WriteAllText("save", final);  
    }
    public static void Load()
    {
        var save = File.ReadAllText("save");
        var buffer = Convert.FromBase64String(save);
        byte[] Key = Encoding.UTF8.GetBytes("minha-chave-secreta1234567890124");
        byte[] IV  = Encoding.UTF8.GetBytes("vetor-inicial-12");
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(buffer);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        var json = sr.ReadToEnd();
        sr.Close();
        Current = JsonSerializer.Deserialize<OSManager>(json);
    }
    
    public readonly OSFolder Root;
    public OSFolder CurrentDir { get; set;}

    public OSManager()
    {
        Root = new() {
            Name = ""
        };
        Root.Add(new OSFolder {
            Name = "home",
            Parent = Root
        });
        var userFolder = new OSFolder {
            Name = "usr",
            Parent = Root
        };
        Root.Add(userFolder);
        userFolder.Add(new OSFolder {
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
        CurrentDir.Add(newFolder);
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
        parent.Remove(item);
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
        CurrentDir.Add(file);
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