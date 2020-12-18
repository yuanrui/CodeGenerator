using Banana.AutoCode.Core;
using System;
using System.IO;
using System.Windows.Forms;

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
            SetEnvironment();
            CultureHelper.SetCulture();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        static void SetEnvironment()
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);
            
            Environment.SetEnvironmentVariable("APP_DIR", directory);
            Environment.SetEnvironmentVariable("OUTPUT_DIR", Path.Combine(directory, ConfigConstants.OUTPUT_DIR));
            Environment.SetEnvironmentVariable("TEMPLATES_DIR", Path.Combine(directory, ConfigConstants.TEMPLATES_DIR));
        }
    }
}
