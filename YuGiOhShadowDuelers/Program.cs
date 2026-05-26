using System;
using System.Windows.Forms;

namespace YuGiOhShadowDuelers
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormTitle());
        }
    }
}
