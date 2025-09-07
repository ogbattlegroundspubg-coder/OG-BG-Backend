using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PUBGLiteBackendWV
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Log.box = rtb1;
            WebServer.Start();
            WSServer.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WebServer.Stop();
            WSServer.Stop();
        }
    }
}
