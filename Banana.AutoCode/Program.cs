using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Banana.AutoCode
{
    static class Program
    {
        static Program()
        {
            CosturaUtility.Initialize();
        }

        [STAThread]
        static void Main()
        {
            CultureHelper.SetCulture();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
