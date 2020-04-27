using System;
using System.Windows.Forms;

using System.IO.Ports;
using System.Threading;

namespace System_test
{
    public partial class Form1 : Form {

        //***********************************************************************
        // Constant definitions
        //***********************************************************************
        const int OK      = 0;
        const int COMBAUD = 115200;

        //***********************************************************************
        // Constant definitions
        //*********************************************************************** 
        UInt32 global_error;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            foreach (string s in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(s);
            }
            comboBox1.SelectedIndex = 0;
            serialPort1.BaudRate = COMBAUD;
            global_error = OK;
            Thread.Sleep(2000);
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click_2(object sender, EventArgs e)
        {
            serialPort1.Close();
            this.Close();
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("C# program to test Wattbot_nt control board", "WattBot-nt");
        }

        private void button1_Click(object sender, EventArgs e) {
            string com_port = comboBox1.SelectedItem.ToString();
            try {
                serialPort1.PortName = com_port;
                serialPort1.Open();
            }
            catch {
                DebugWindow.AppendText("Cannot open " + com_port + Environment.NewLine);
                return;
            }
            DebugWindow.AppendText(com_port + "now open" + Environment.NewLine);
        }
    }
}
