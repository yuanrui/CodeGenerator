using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Banana.AutoCode.Forms
{
    public static class FormExtension
    {
        public static void SetLanguage(this Form form)
        {
            String cultureName = System.Globalization.CultureInfo.InstalledUICulture.Name;
            SetLanguage(form, cultureName);
        }

        public static void SetLanguage(this Form form, String cultureName)
        {
            ComponentResourceManager resManager = new ComponentResourceManager(form.GetType());
            CultureInfo cultureInfo = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            foreach (Control ctrl in form.Controls)
            {
                resManager.ApplyResources(ctrl, ctrl.Name);
                Debug.WriteLine(ctrl.Name);
            }

            form.ResumeLayout(false);
            form.PerformLayout();
            resManager.ApplyResources(form, "$this");
        }

    }
}
