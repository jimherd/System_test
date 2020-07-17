using System;
using System.Windows.Forms;

using System.IO.Ports;
using System.Threading;

using MotionIOLibrary;


namespace System_test {
    public partial class Form1 : Form {

        //***********************************************************************
        // Constant definitions
        //***********************************************************************
        const int SUCCESS      = 0;

        //***********************************************************************
        // Constant definitions
        //*********************************************************************** 
        UInt32 global_error;

        FPGA_uP_IO FPGA_uP_IO = new FPGA_uP_IO();

        public Form1() {
            InitializeComponent();
        }

        //***********************************************************************
        // User functions
        //*********************************************************************** 
        // do_command : execute motion system command
        // ==========
        //
        //public FPGA_uP_IO.ErrorCode do_command(string command) {

        //    DebugWindow.AppendText(command + Environment.NewLine);
        //    serialPort1.WriteLine(command);
        //    DebugWindow.AppendText("Wating for reply" + Environment.NewLine);
        //    int status = get_reply();

        //    return 0;
        //}

        //*********************************************************************** 
        // get_reply : Read a status/data reply from LLcontrol subsystem
        //
        //public Int32 get_reply() {
        //    string reply;
        //    int status;

        //    //serialPort1.DiscardInBuffer();
        //    serialPort1.ReadTimeout = READ_TIMEOUT;
        //    try {
        //        reply = serialPort1.ReadLine();
        //    }
        //    catch (TimeoutException) {
        //        DebugWindow.AppendText("ReadLine timeout fail" + Environment.NewLine);
        //        return -1;
        //    }
        //    DebugWindow.AppendText("Reply = " + reply);
        //    return SUCCESS;
        //}

        //***********************************************************************
        // Window interface functions
        //*********************************************************************** 
        private void Form1_Load(object sender, EventArgs e) {

            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0) {
                foreach (string s in ports) {
                    comboBox1.Items.Add(s);
                }
            } else {
                DebugWindow.AppendText("No serial ports" + Environment.NewLine);
                return;
            }
            comboBox1.SelectedIndex = 0;

            global_error = SUCCESS;
            Thread.Sleep(2000);
        }

        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void exitToolStripMenuItem_Click_2(object sender, EventArgs e) {

            serialPort1.Close();
            this.Close();
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e) {

            MessageBox.Show("C# program to test Wattbot_nt control board", "WattBot-nt");
        }

        private void button1_Click(object sender, EventArgs e) {

            if (comboBox1.SelectedItem == null) {
                DebugWindow.AppendText("No COM port selected " + Environment.NewLine);
                return;
            }
            string com_port = comboBox1.SelectedItem.ToString();
            int baud_rate = Convert.ToInt32(comboBox2.SelectedItem);

            FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.Init_comms(com_port, baud_rate);

            if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                DebugWindow.AppendText("Cannot open " + com_port + Environment.NewLine);
                return;
            }
            DebugWindow.AppendText(com_port + " now open" + Environment.NewLine);
        }

        private void button2_Click_1(object sender, EventArgs e) {

            String command = "Pu 5";

            DebugWindow.AppendText(command + Environment.NewLine);
            FPGA_uP_IO.do_command(command);
            DebugWindow.AppendText("Wating for reply" + Environment.NewLine);
           // int status = FPGA_uP_IO.get_reply();
        }
    }
}
