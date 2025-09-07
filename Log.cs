using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PUBGLiteBackendWV
{
    public static class Log
    {
        public static RichTextBox box = null;

        public static void WriteLine(string who, string s, object c = null)
        {
            if (box == null)
                return;
            try
            {
                box.Invoke((MethodInvoker)delegate()
                {
                    box.SelectionStart = box.TextLength;
                    box.SelectionLength = 0;
                    if (c != null)
                        box.SelectionColor = (Color)c;
                    else
                        box.SelectionColor = Color.Black;
                    box.AppendText(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " [" + who + "] : " + s + "\n");
                    box.SelectionStart = box.Text.Length;
                    box.ScrollToCaret();
                });
            }
            catch { }
        }
    }
}
