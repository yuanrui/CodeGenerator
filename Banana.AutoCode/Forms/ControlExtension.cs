using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Banana.AutoCode.Forms
{
    public static class ControlExtension
    {
        public static T InvokeIfRequired<T>(this T control, Action<T> action)
            where T : Control
        {
            try
            {
                if (!control.InvokeRequired)
                    action(control);
                else
                    control.Invoke(new Action(() => action(control)));
            }
            catch (Exception ex)
            {
                Debug.Write("Call InvokeIfRequired Exception: {0}", ex.Message);
            }

            return control;
        }

    }
}
