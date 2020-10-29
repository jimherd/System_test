using MotionIOLibrary;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace System_test {
    public partial class Form1 : Form {

        //***********************************************************************
        // Constant definitions
        //***********************************************************************
        const int SUCCESS = 0;
        const string DEFAULT_PORT = "9";

        string[] baud_rates = new string[] { "256000", "230400", "128000", "115200", "9600" };

        //***********************************************************************
        // variables and methods
        //***********************************************************************

        Boolean connected = false;
        UInt32  global_error;

        FPGA_uP_IO FPGA_uP_IO = new FPGA_uP_IO();

        public Form1()
        {
            InitializeComponent();
        }

        //***********************************************************************
        // User functions
        //*********************************************************************** 


        //***********************************************************************
        // Window interface functions
        //*********************************************************************** 
        private void Form1_Load(object sender, EventArgs e)
        {
            //
            // Serial port combobox

            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0) {
                foreach (string s in ports) {
                    comboBox1.Items.Add(s);
                }
            }
            else {
                DebugWindow.AppendText("No serial ports" + Environment.NewLine);
                return;
            }
            comboBox1.SelectedIndex = 0;
            //
            // baud rate combobox

            comboBox2.DataSource = baud_rates;
            comboBox2.SelectedIndex = 0;

            global_error = SUCCESS;
            Thread.Sleep(2000);
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

        private void button1_Click(object sender, EventArgs e)
        {

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
            connected = true;
            DebugWindow.AppendText(com_port + " now open" + Environment.NewLine);
        }


        private void button5_Click(object sender, EventArgs e)
        {
            DebugWindow.Clear();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string command_str = "w " + DEFAULT_PORT + " " + numericUpDown1.Value + " " + textBox1.Text + Environment.NewLine;
            FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str));
            DebugWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
            DebugWindow.AppendText("Register No = " + numericUpDown1.Value + Environment.NewLine);
            DebugWindow.AppendText("Register Value = " + textBox1.Text + Environment.NewLine);
            DebugWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string command_str = "r " + DEFAULT_PORT + " " + numericUpDown1.Value + " " + "0" + Environment.NewLine;
            FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str));
            DebugWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
            DebugWindow.AppendText("Register No = " + numericUpDown1.Value + Environment.NewLine);
            DebugWindow.AppendText("Register Value = " + FPGA_uP_IO.int32_parameters[2] + Environment.NewLine);
            DebugWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;

            if (connected == false) {
                DebugWindow.AppendText("No serial connection" + Environment.NewLine);
                return;
            }

            if (radioButton1.Checked == true) {
                status = FPGA_uP_IO.soft_bus_check();
                DebugWindow.AppendText("Port = " + FPGA_uP_IO.int32_parameters[0] + Environment.NewLine);
                DebugWindow.AppendText("Soft bus check : Return code = " + status + Environment.NewLine);
            }
            if (radioButton2.Checked == true) {
                status = FPGA_uP_IO.hard_bus_check();
                DebugWindow.AppendText("Port = " + FPGA_uP_IO.int32_parameters[0] + Environment.NewLine);
                DebugWindow.AppendText("Hard bus check : Return code = " + status + Environment.NewLine);
            }
            if (radioButton3.Checked == true) {
                status = FPGA_uP_IO.get_sys_data();
                UInt32 data = (UInt32)FPGA_uP_IO.int32_parameters[2];
                DebugWindow.AppendText("Port = " + FPGA_uP_IO.int32_parameters[0] + Environment.NewLine);
                DebugWindow.AppendText("get_sys_data : Return data = " + data + Environment.NewLine);
                DebugWindow.AppendText("Version = " + (data & 0x0F) + "." + ((data >> 4) & 0x0F) + Environment.NewLine);
                DebugWindow.AppendText("PWM units = " + ((data >> 8) & 0x0F) + Environment.NewLine);
                DebugWindow.AppendText("QE  units = " + ((data >> 12) & 0x0F) + Environment.NewLine);
                DebugWindow.AppendText("RC  units = " + ((data >> 16) & 0x0F) + Environment.NewLine);
                DebugWindow.AppendText("get_sys_data : Return code = " + status + Environment.NewLine);
            }
        }
    }
}
