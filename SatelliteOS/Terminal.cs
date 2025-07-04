using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace SatelliteOS;

internal class Terminal
{
    public readonly static Terminal Current = new();
    List<string> history = [];
    int historicPosition = 0;
    SatelliteView view;
    readonly Compiler compiler;
    readonly Form form;
    readonly Label text;
    readonly Timer timer;
    readonly List<string> content =  [ "Satellite OS@2025" ];
    public Terminal()
    {
        compiler = new Compiler();

        form = new Form {
            FormBorderStyle = FormBorderStyle.FixedToolWindow,
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
        form.FormClosing += (o, e) => OSManager.Current.StopAll();
        form.KeyDown += (o, e) =>
        {
            if (e.KeyCode == Keys.Enter && !onstring)
            {
                historicPosition = 0;
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
                if (content[^1][^1] == '\'')
                    onstring = !onstring;
                content[^1] = content[^1][..^1];
                UpdateText();
                return;
            }


            var baseChar = ((char)e.KeyValue)
                .ToString()
                .ToLower();

            if (e.KeyCode == Keys.Up)
            {
                historicPosition += 1;
                if (historicPosition > history.Count)
                {
                    historicPosition -= 1;
                    return;
                }
                content[^1] = "";
                InitCommand();
                content[^1] += history[^historicPosition];
                UpdateText();
                return;
            }
            if (e.KeyCode == Keys.Down)
            {
                historicPosition -= 1;
                if (historicPosition < 1)
                {
                    historicPosition = 1;
                    return;
                }
                content[^1] = "";
                InitCommand();
                content[^1] += history[^historicPosition];
                UpdateText();
                return;
            }
            
            if (Control.IsKeyLocked(Keys.CapsLock) && !e.Shift)
                    baseChar = baseChar.ToUpper();
            if (e.Shift)
                baseChar = baseChar.ToUpper();
            if (e.Shift && e.KeyCode == Keys.D9)
                baseChar = "(";
            else if (e.Shift && e.KeyCode == Keys.D0)
                baseChar = ")";
            else if (e.Shift && e.KeyCode == Keys.D7)
                baseChar = "&";
            else if (e.Shift && e.KeyCode == Keys.D4)
                baseChar = "$";
            else if (e.KeyCode == Keys.Oem2)
                baseChar = ";";
            else if (e.KeyCode == Keys.OemPeriod && e.Shift)
                baseChar = ">";
            else if (e.KeyCode == Keys.Oemcomma && !e.Shift)
                baseChar = ",";
            else if (e.KeyCode == Keys.Oemcomma && e.Shift)
                baseChar = "<";
            else if (e.KeyCode == Keys.OemMinus)
                baseChar = "-";
            else if (e.KeyCode == Keys.Oem6 && e.Shift)
                baseChar = "{";
            else if (e.KeyCode == Keys.OemPipe && e.Shift)
                baseChar = "}";
            else if (e.KeyCode == Keys.OemPeriod)
                baseChar = ".";
            else if (e.KeyValue == 193)
                baseChar = "/";
            else if (e.KeyCode == Keys.ShiftKey)
                baseChar = "";
            else if (e.KeyCode == Keys.ControlKey)
                baseChar = "";
            else if (!e.Shift && e.KeyCode == Keys.Oemplus)
                baseChar = "=";
            else if (e.Shift && e.KeyCode == Keys.Oemplus)
                baseChar = "+";
            else if (!e.Shift && e.KeyCode == Keys.Oemtilde)
            {
                baseChar = "'";
                onstring = !onstring;
            }
            else if (e.Shift && e.KeyCode == Keys.Oemtilde)
                baseChar = "\"";
            else if (e.KeyCode == Keys.Enter && onstring)
                baseChar = "\n";
            else if (e.Control && e.KeyCode == Keys.V)
                baseChar = Clipboard.GetText();
            
            text.Text += baseChar;
            content[^1] += baseChar;
        };

        timer = new Timer {
            Interval = 10
        };
        timer.Tick += (o, e) =>
        {
            if (OS.Buffer.IsEmpty)
                return;
            
            while (OS.Buffer.TryDequeue(out var message))
            {
                content.Insert(
                    content.Count - 1,
                    message
                );
            }
            UpdateText();
        };
        form.Load += (o, e) => timer.Start();

        form.Controls.Add(text);
    }

    public void RunCommand(string prompt)
    {
        prompt = prompt.Trim();
        history.Add(prompt);

        var parts = prompt.Split(' ');
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
            
            case "load":
                OSManager.Load();
                break;
            
            case "save":
                OSManager.Save();
                break;
            
            case "reset":
                OSManager.Reset();
                break;

            case "view":
                view = new SatelliteView();
                view.Show();
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
                
            case "mv":
                if (args.Count != 2)
                {
                    Append("mv expected 2 arguments.");
                    break;
                }
                var mvresult = OSManager.Current.MV(args[0], args[1]);
                Append(mvresult);
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
            
            case "jobs":
                var jobsreuslt = OSManager.Current.JOBS();
                foreach (var job in jobsreuslt)
                {
                    Append(job);
                    AppendLine();
                }
                break;
            
            case "kill":
                if (args.Count == 0)
                {
                    Append("kill expected 1 argument.");
                    break;
                }

                var killresult = OSManager.Current.KILL(args[0]);
                Append(killresult);
                break;
            
            case "dotnet":
                RunDotnet([ ..args ]);
                break;
            
            case "code":
                if (args.Count == 0)
                {
                    Append("code expected 1 argument.");
                    break;
                }
                var vsresult = OSManager.Current.CODE(args[0]);
                Append(vsresult);
                break;

            default:
                var runres = OSManager.Current.Run(prompt);
                foreach (var item in runres)
                {
                    Append(item);
                    AppendLine();
                }
                break;
        }
    }

    void RunDotnet(string[] args)
    {
        if (args.Length == 0)
        {
            Append("dotnet 8.0.0");
            AppendLine();
            return;
        }

        switch (args[0])
        {
            case "new":
                RunCommand("touch program.cs");
                RunCommand("echo 'OS.WriteLine(\"Hello, World!\");' > program.cs");
                break;
            
            case "run":
                var rfile = args.Length < 2 ? "program.cs" : args[1];
                var lines = OSManager.Current.CAT(rfile);
                var code = string.Join("\n", lines);
                try
                {
                    var result = compiler.GetNewAssembly([ code ], []);
                    foreach (var message in result.Item2)
                    {
                        Append(message);
                        AppendLine();
                    }
                    if (result.Item1 is null)
                        break;
                    result.Item1.EntryPoint.Invoke(null, [ args[1..] ]);
                }
                catch (Exception ex)
                {
                    Append(ex.InnerException?.Message);
                }
                break;
            
            case "compile":
                var cfile = args.Length < 2 ? "program.cs" : args[1];
                var clines = OSManager.Current.CAT(cfile);
                var ccode = string.Join("\n", clines);
                var bin = OSManager.Encript(ccode);
                var binname = cfile.Split('.')[0] + ".bin";

                var compresult = compiler.GetNewAssembly([ ccode ], []);
                foreach (var message in compresult.Item2)
                {
                    Append(message);
                    AppendLine();
                }
                if (compresult.Item2.Length > 0)
                    break;

                RunCommand($"touch {binname}");
                RunCommand($"echo '{bin}' > {binname}");
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