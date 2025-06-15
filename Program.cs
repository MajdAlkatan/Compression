// ✅ Program.cs
using System;
using System.Windows.Forms;
using WinFormsApp1.Forms;

namespace WinFormsApp1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}