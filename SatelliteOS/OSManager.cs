using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace SatelliteOS;

internal class OSManager
{
    public static OSManager Current { get; private set; } = new();
    
    public static void Reset() 
        => Current = new();
    
    public static void Save()
    {
        var json = JsonSerializer.Serialize(Current);
        var final = Encript(json);
        File.WriteAllText("save", final);  
    }
    
    public static void Load()
    {
        var save = File.ReadAllText("save");
        var json = Decript(save);
        Current = JsonSerializer.Deserialize<OSManager>(json);
        fixParents(Current.Root);
        Current.CurrentDir = Current.Root;
        
        static void fixParents(OSItem item)
        {
            if (item is OSFolder folder)
            {
                foreach (var x in folder.Content)
                {
                    x.Parent = folder;
                    fixParents(x);
                }
            }
        }
    }
    
    public static string Encript(string text)
    {
        byte[] Key = Encoding.UTF8.GetBytes("minha-chave-secreta1234567890124");
        byte[] IV  = Encoding.UTF8.GetBytes("vetor-inicial-12");
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(text);
        sw.Close();
        return Convert.ToBase64String(ms.ToArray());
    }
    
    public static string Decript(string text)
    {
        try
        {
            var buffer = Convert.FromBase64String(text);
            byte[] Key = Encoding.UTF8.GetBytes("minha-chave-secreta1234567890124");
            byte[] IV  = Encoding.UTF8.GetBytes("vetor-inicial-12");
            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            var txt = sr.ReadToEnd();
            sr.Close();
            return txt;
        }
        catch
        {
            return null;
        }
    }

    public OSFolder Root { get; set; }
    public OSFolder CurrentDir { get; set; }
    public OSFolder BinaryFolder { get; set; }

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
        userFolder.Add(BinaryFolder = new OSFolder {
            Name = "bin",
            Parent = userFolder
        });

        CurrentDir = Root;
    }

    public string[] Run(string command)
    {
        var parts = command.Split(" ");
        var item = BinaryFolder.Files.FirstOrDefault(
            x => x.Name == parts[0]
        );
        if (item is not OSFile file)
            return [ $"unknow command '{parts[0]}'." ];
        
        var bin = file.Content;
        var code = Decript(bin);
        if (code is null)
            return [ "the selected file is not a executable." ];

        if (command.Contains('&'))
        {
            var thread = new Thread(() =>
            {
                var compiler = new Compiler();
                var assembly = compiler.GetNewAssembly([ code ], []);
                assembly.Item1.EntryPoint.Invoke(null, [ new string[0] ]);
            });
            thread.Start();
        }
        else
        {
            var compiler = new Compiler();
            var assembly = compiler.GetNewAssembly([ code ], []);
            if (assembly.Item1 is null)
                return [ "The executable file has erros." ];
            assembly.Item1.EntryPoint.Invoke(null, [ new string[0] ]);
        }
        return [ "" ];
    }

    public string MV(string start, string target)
    {
        var parts = start.Split(".");
        if (parts.Length == 1)
            return "A file needs name and extension.";
        
        var fileName = parts[0];
        var extension = parts[1];

        var item = CurrentDir.Content.FirstOrDefault(
            i => i is OSFile f && f.ItemName == start
        );
        if (item is null)
            return $"unknow file named '{start}'.";
        
        item.Parent.Remove(item);
        item.Parent = null;
        
        var it = CurrentDir;
        var moves = target.Split("/");
        foreach (var mov in moves[..^1])
        {
            if (mov == "..")
            {
                it = it.Parent;
                continue;
            }

            var next = it.Content
                .FirstOrDefault(f => f.Name == mov);
            
            if (next is null || next is not OSFolder folder)
                return $"unknown folder '{next}'.";
            
            it = folder;
        }

        item.Name = moves[^1].Split(".")[0];
        item.Parent = it;
        it.Add(item);
        return "";
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
            i => i is OSFile f && f.ItemName == name
                || i.Name == name
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