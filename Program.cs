using System;
using System.Windows.Forms;

namespace ApexGamerBooster
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Forms.Form1());
        }
    }
}