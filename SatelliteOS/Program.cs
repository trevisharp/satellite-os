using System;
using SatelliteOS;

class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        Terminal.Current.Start();
    }
}