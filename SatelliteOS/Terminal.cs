using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

public class Terminal
{
    readonly Form form;
    readonly Label text;
    readonly List<string> content =  [ "Satellite OS@2025" ];
    public Terminal()
    {
        form = new Form {
            FormBorderStyle = FormBorderStyle.SizableToolWindow,
            BackColor = Color.Black,
            Width = 800,
            Height = 640,
            Text = "Satellite OS"
        };

        text = new Label {
            Dock = DockStyle.Fill,
            BackColor = Color.Black,
            ForeColor = Color.White
        };
        AppendLine();
        AppendLine();
        InitCommand();
        UpdateText();

        form.KeyPreview = true;
        bool onstring = false;
        form.KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.Enter && !onstring)
            {
                var command = content[^1][4..];
                AppendLine();
                RunCommand(command);
                AppendLine();
                InitCommand();
                UpdateText();
                return;
            }

            if (e.KeyCode == Keys.Back)
            {
                if (content[^1].Length == 4)
                    return;
                content[^1] = content[^1][..^1];
                UpdateText();
                return;
            }

            var baseChar = ((char)e.KeyValue)
                .ToString()
                .ToLower();
            if (e.KeyCode == Keys.OemPeriod && e.Shift)
                baseChar = ">";
            else if (e.KeyCode == Keys.OemPeriod)
                baseChar = ".";
            else if (e.KeyValue == 193)
                baseChar = "/";
            else if (e.KeyCode == Keys.ShiftKey)
                baseChar = "";
            else if (e.KeyCode == Keys.Oemtilde)
            {
                baseChar = "\'";
                onstring = !onstring;
            }
            else if (e.KeyCode == Keys.Enter && onstring)
            {
                baseChar = "\n";
            }
            
            text.Text += baseChar;
            content[^1] += baseChar;
        };

        form.Controls.Add(text);
    }

    void RunCommand(string prompt)
    {
        var parts = prompt.Split(" ");
        var command = parts[0].ToLower();
        var args = new List<string>();
        bool instring = false;
        var str = "";
        foreach (var p in parts[1..])
        {
            if (!instring && p.StartsWith('\''))
                instring = true;
            if (instring && p.EndsWith('\''))
            {
                str += p.Replace("\'", "") + " ";
                args.Add(str);
                str = "";
                instring = false;
                continue;
            }
            
            if (instring)
                str += p.Replace("\'", "") + " ";
            else args.Add(p);
        }
        switch (command)
        {
            case "":
                break;
            case "pwd":
                var pwdresult = OSManager.Current.PWD();
                Append(pwdresult);
                break;
            
            case "ls":
                var lsresult = OSManager.Current.LS();
                foreach (var item in lsresult)
                {
                    Append(item);
                    AppendLine();
                }
                break;

            case "cd":
                if (args.Count == 0)
                {
                    Append("cd expected arguments.");
                    break;
                }
                var cdresult = OSManager.Current.CD(args[0]);
                Append(cdresult);
                break;
            
            case "clear":
                Clear();
                break;
            
            case "exit":
                form.Close();
                break;

            case "mkdir":
                if (args.Count == 0)
                {
                    Append("mkdir expected arguments.");
                    break;
                }
                var mkdirresult = OSManager.Current.MKDIR(args[0]);
                Append(mkdirresult);
                break;
                
            case "rm":
                if (args.Count == 0)
                {
                    Append("rm expected arguments.");
                    break;
                }
                var rmresult = OSManager.Current.RM(args[0]);
                Append(rmresult);
                break;
            
            case "touch":
                if (args.Count == 0)
                {
                    Append("touch expected arguments.");
                    break;
                }
                var touchresult = OSManager.Current.TOUCH(args[0]);
                Append(touchresult);
                break;
            
            case "cat":
                if (args.Count == 0)
                {
                    Append("cat expected arguments.");
                    break;
                }
                var catresult = OSManager.Current.CAT(args[0]);
                foreach (var item in catresult)
                {
                    Append(item);
                    AppendLine();
                }
                break;
            
            case "echo":
                if (args.Count != 3)
                {
                    Append("echo expected 3 arguments.");
                    break;
                }
                var echoresult = OSManager.Current.ECHO(
                    args[0], args[1], args[2]
                );
                Append(echoresult);
                break;

            default:
                Append($"unknow command '{command}'.");
                break;
        }
    }

    void Clear()
    {
        content.Clear();
        content.Add("");
    }

    void Append(string text)
        => content[^1] += text;

    void AppendLine()
    {
        content.Add("");
        if (content.Count > 22)
            content.RemoveAt(0);
    }

    void InitCommand()
        => content[^1] += " >> ";

    void UpdateText()
        => text.Text = string.Join("\n", content);

    public void Start()
    {
        Application.Run(form);
    }
}