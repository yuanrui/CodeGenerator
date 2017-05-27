using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Drawing;

namespace Banana.AutoCode.Forms
{
    public class OutputTraceListener : TraceListener
    {
        OutputPanel _panel;

        public OutputTraceListener(OutputPanel panel)
        {
            _panel = panel;
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            _panel.AppendText(message + Environment.NewLine, Color.Red);
        }

        public override void Write(string message)
        {
            _panel.AppendText(message + Environment.NewLine, Color.Gray);
        }

        public override void WriteLine(string message)
        {
            _panel.AppendText(message + Environment.NewLine, Color.Black);
        }
    }
}
