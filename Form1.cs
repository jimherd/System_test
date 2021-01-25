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
        private const int SUCCESS = 0;

        private const int DEFAULT_PORT = 9;

        private string[] baud_rates = new string[] { "256000", "230400", "128000", "115200", "9600" };

        private const double MIN_PWM_FREQUENCY = 0.01;   //kHz
        private const double MAX_PWM_FREQUENCY = 100.0;  //kHz

        //***********************************************************************
        // variables and methods
        //***********************************************************************

        private UInt32 nos_PWM_units = 0;
        private UInt32 nos_QE_units = 0;
        private UInt32 nos_RC_units = 0;

        private Boolean connected = false;
        private UInt32 global_error;

        //***********************************************************************
        //  Initialise oblects
        //***********************************************************************

        private FPGA_uP_IO FPGA_uP_IO = new FPGA_uP_IO();  // Import FPGA interface functions
        private FPGA_Sys FPGA_Sys = new FPGA_Sys();        // Import FPGA configuration constants

        public Form1()
        {
            InitializeComponent();
        }

        //***********************************************************
        // User functions
        //***********************************************************
        // build_command : Format an ASCII string ready to be sent to the uP
        // =============

        private string build_command(char cmd_name, int port, int register, int data)
        {
            string command = cmd_name + " " + port + " " + register + " " + data + "\n";
            return command;
        }


        private FPGA_uP_IO.ErrorCode Read_register(int register)
        {
            int data;
            string command_str;
            FPGA_uP_IO.ErrorCode status;

            command_str = build_command('r', DEFAULT_PORT, register, 0);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (FPGA_uP_IO.do_command(command_str, out data));
            InfoWindow.AppendText("Register = " + register + ":: Value = " + data + Environment.NewLine);
            return status;
        }

        public FPGA_uP_IO.ErrorCode do_command(string command, out int data)
        {
            string reply_string;

            reply_string = "\n";

            FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.ErrorCode.NO_ERROR;

            status = FPGA_uP_IO.send_command(command);
            data = 0;
            if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                return status;
            }
            for (; ; ) {
                status = FPGA_uP_IO.get_reply(ref reply_string);
                if ((reply_string[0] == 'D') && (reply_string[1] == ':')) {
                    DebugWindow.AppendText(reply_string + Environment.NewLine);
                    continue;
                }
                else {
                    break;
                }
            }
            if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                return status;
            }
            status = FPGA_uP_IO.parse_parameter_string(reply_string);
            if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                return status;
            }
            data = FPGA_uP_IO.int_parameters[2];
            return status;
        }

        //***********************************************************
        // Window interface functions
        //***********************************************************

        private void Form1_Load(object sender, EventArgs e)
        {
            // 1. Populate Serial port combobox
            // 2. Initialise baud rate combobox
            // 3. Initialse global variables
            // 4. Sleep for a couple of seconds 

            comboBox1.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0) {
                foreach (string s in ports) {
                    comboBox1.Items.Add(s);
                }
            }
            else {
                InfoWindow.AppendText("No serial ports" + Environment.NewLine);
                return;
            }
            comboBox1.SelectedIndex = 0;
            //
            comboBox2.DataSource = baud_rates;
            comboBox2.SelectedIndex = 0;
            //
            global_error = SUCCESS;
            Thread.Sleep(2000);
        }

            //***********************************************************
            // Open_COM_port : Open selected seial COM port
            // =============
            private void Open_COM_port(object sender, EventArgs e)
            {
                if (comboBox1.SelectedItem == null) {
                    InfoWindow.AppendText("No COM port selected " + Environment.NewLine);
                    return;
                }
                string com_port = comboBox1.SelectedItem.ToString();
                int baud_rate = Convert.ToInt32(comboBox2.SelectedItem);

                FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.Init_comms(com_port, baud_rate);

                if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                    InfoWindow.AppendText("Cannot open " + com_port + Environment.NewLine);
                    return;
                }
                connected = true;
                button4.Enabled = true;

                InfoWindow.AppendText(com_port + " now open" + Environment.NewLine);
            }

            //***********************************************************
            private void exitToolStripMenuItem_Click_2(object sender, EventArgs e)
        {
            serialPort1.Close();
            this.Close();
        }

        private void aboutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("C# program to test Wattbot_nt control board", "WattBot-nt");
        }

        private void Clear_DEBUG_window(object sender, EventArgs e)
        {
            InfoWindow.Clear();
        }

        //****************************************************************
        // Run commands to PING FPGA/uP or read FPGA subsystem configuration

        private void Execute_PING_cmds(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;

            if (radioButton1.Checked == true) {
                status = FPGA_uP_IO.soft_bus_check();
                InfoWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                InfoWindow.AppendText("Soft bus check : Return code = " + status + Environment.NewLine);
            }
            if (radioButton2.Checked == true) {
                status = FPGA_uP_IO.hard_bus_check();
                InfoWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                InfoWindow.AppendText("Hard bus check : Return code = " + status + Environment.NewLine);
            }
            /*if (radioButton3.Checked == true) {
                status = FPGA_uP_IO.get_sys_data();
                if (status == SUCCESS) {
                    UInt32 data = (UInt32)FPGA_uP_IO.int_parameters[2];
                    nos_PWM_units = ((data >> 8) & 0x0F);
                    nos_QE_units = ((data >> 12) & 0x0F);
                    nos_RC_units = ((data >> 16) & 0x0F);
                    DebugWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                    DebugWindow.AppendText("get_sys_data : Return data = " + data + Environment.NewLine);
                    DebugWindow.AppendText("Version = " + (data & 0x0F) + "." + ((data >> 4) & 0x0F) + Environment.NewLine);
                    DebugWindow.AppendText("PWM units = " + nos_PWM_units + Environment.NewLine);
                    DebugWindow.AppendText("QE  units = " + nos_QE_units + Environment.NewLine);
                    DebugWindow.AppendText("RC  units = " + nos_RC_units + Environment.NewLine);

                    Write_Register.Enabled = true;
                    Read_Register.Enabled = true;
                    button9.Enabled = true;
                    button11.Enabled = true;
                }
                else {
                    DebugWindow.AppendText("get_sys_data : Error code = " + status + Environment.NewLine);
                }
            }*/
        }

        //****************************************************************
        // Run commands to control a PWM channel

        private void Execute_PWM_cmd(object sender, EventArgs e)
        {
        string command_str;
        int PWM_channel, PWM_width_percent, channel_base_address, on_time_nS, on_time_FPGA_count, config_value, data, period_time_FPGA_count;
        double period_time_nS, PWM_frequency;
        FPGA_uP_IO.ErrorCode status;

            status = FPGA_uP_IO.ErrorCode.NO_ERROR;

        // get values from PWM form and make checks

            PWM_channel      = (int)numericUpDown5.Value;
            PWM_frequency    = double.Parse(textBox3.Text);
            if ((PWM_frequency < MIN_PWM_FREQUENCY) || (PWM_frequency > MAX_PWM_FREQUENCY)) {
                MessageBox.Show("Input PWM frequenct in range 0.01 to 100 (kHz)");
                return;
            }
            PWM_width_percent = Convert.ToInt32(numericUpDown6.Value);
 
            // Calculate FPGA/uP command parameters

            channel_base_address    = (Int32)(FPGA_uP_IO.PWM_base + (PWM_channel * FPGA_uP_IO.REGISTERS_PER_PWM_CHANNEL));

            period_time_nS          = Convert.ToInt32(1000000.0 / PWM_frequency);
            period_time_FPGA_count  = (int)(period_time_nS / 20);
            on_time_nS              = Convert.ToInt32((PWM_width_percent * period_time_nS) / 100);
            on_time_FPGA_count      = on_time_nS / 20;

            // check mode to execute

            if (radioButton5.Checked == true) {     // full configuration
                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.PERIOD), period_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.ON_TIME), on_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
                config_value = 0x00010001;
                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), config_value);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
            }

            if (radioButton6.Checked == true) {    // set PWM ON time
                command_str = build_command('w', 
                                            DEFAULT_PORT, 
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.ON_TIME), 
                                            on_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
            }

            if (radioButton7.Checked == true) {    // enable PWM channel
                config_value = 1;
                command_str = build_command('w',
                                            DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG),
                                            config_value);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
            }

            if (radioButton8.Checked == true) {    // disable PWM channel
                config_value = 0;
                command_str = build_command('w',
                                            DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG),
                                            config_value);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (FPGA_uP_IO.do_command(command_str, out data));
            }
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Run command to read a specified register
        private void Read_Register_Click(object sender, EventArgs e)
        {
            int data;

            string command_str = "r " + DEFAULT_PORT + " " + numericUpDown2.Value + " " + "0" + "\n";
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str, out data));
            InfoWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
            InfoWindow.AppendText("Register No = " + numericUpDown2.Value + Environment.NewLine);
            InfoWindow.AppendText("Register Value = " + FPGA_uP_IO.int_parameters[2] + Environment.NewLine);
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Run command to write to a specified register
        private void Write_Register_Click(object sender, EventArgs e)
        {
            int data;

            data = int.Parse(textBox1.Text, System.Globalization.NumberStyles.HexNumber);
            string command_str = "w " + DEFAULT_PORT + " " + numericUpDown1.Value + " " + data + '\n';
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            FPGA_uP_IO.ErrorCode status = (do_command(command_str, out data));
            InfoWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
            InfoWindow.AppendText("Register No = " + numericUpDown1.Value + Environment.NewLine);
            InfoWindow.AppendText("Register Value = " + textBox1.Text + Environment.NewLine);
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Run commands to RC servo commands
        private void Execute_RC_servo_cmds(object sender, EventArgs e)
        {
        }

        private void Read_System_Data(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;

            status = FPGA_uP_IO.get_sys_data();
            if (status == SUCCESS) {
                UInt32 data = (UInt32)FPGA_uP_IO.int_parameters[2];
                nos_PWM_units = ((data >> 8) & 0x0F);
                nos_QE_units = ((data >> 12) & 0x0F);
                nos_RC_units = ((data >> 16) & 0x0F);
                InfoWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                InfoWindow.AppendText("get_sys_data : Return data = " + data + Environment.NewLine);
                InfoWindow.AppendText("Version = " + (data & 0x0F) + "." + ((data >> 4) & 0x0F) + Environment.NewLine);
                InfoWindow.AppendText("PWM units = " + nos_PWM_units + Environment.NewLine);
                InfoWindow.AppendText("QE  units = " + nos_QE_units + Environment.NewLine);
                InfoWindow.AppendText("RC  units = " + nos_RC_units + Environment.NewLine);

                Write_Register.Enabled = true;
                Read_Register.Enabled = true;
                button9.Enabled = true;
                button11.Enabled = true;
            }
            else {
                InfoWindow.AppendText("get_sys_data : Error code = " + status + Environment.NewLine);
            }
        }
    }
}






        // command_str = "w " + DEFAULT_PORT + " " + (PWM_unit_base_address + FPGA_Sys.PWM_REG.ON_TIME) + " " + numericUpDown1.Value + "\n";


//private void button7_Click(object sender, EventArgs e)
//{
//    string command_str = "r " + DEFAULT_PORT + " " + numericUpDown1.Value + " " + "0" + Environment.NewLine;
//    DebugWindow.AppendText("command = " + command_str + Environment.NewLine);
//    FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str));
//    DebugWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
//    DebugWindow.AppendText("Register No = " + numericUpDown1.Value + Environment.NewLine);
//    DebugWindow.AppendText("Register Value = " + FPGA_uP_IO.int_parameters[2] + Environment.NewLine);
//    DebugWindow.AppendText("Return code = " + status + Environment.NewLine);
//}

//private void button6_Click(object sender, EventArgs e)
//{
//    string command_str = "w " + DEFAULT_PORT + " " + numericUpDown1.Value + " " + textBox1.Text + Environment.NewLine;
//    DebugWindow.AppendText("command = " + command_str + Environment.NewLine);
//    FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str));
//    DebugWindow.AppendText("Port = " + DEFAULT_PORT + Environment.NewLine);
//    DebugWindow.AppendText("Register No = " + numericUpDown1.Value + Environment.NewLine);
//    DebugWindow.AppendText("Register Value = " + textBox1.Text + Environment.NewLine);
//    DebugWindow.AppendText("Return code = " + status + Environment.NewLine);
//}

//    if (radioButton5.Checked == true) {   // run full setup
//    }

//    if (radioButton6.Checked == true) {   // change PWM width only
//        command_str = build_command('w', DEFAULT_PORT, (PWM_unit_base_address + (int)FPGA_Sys.PWM_REG.ON_TIME), (int)numericUpDown1.Value);
//        DebugWindow.AppendText("command = " + command_str + Environment.NewLine);
//        FPGA_uP_IO.ErrorCode status = (FPGA_uP_IO.do_command(command_str));
//    }




//    if (radioButton7.Checked == true) {   // Enable channel
//    }
//    if (radioButton8.Checked == true) {   // disable channel
//    }
//}

////
//// calculate base address of selected PWM unit

//int PWM_unit_base_address = FPGA_uP_IO.PWM_base + ((int)numericUpDown5.Value * FPGA_uP_IO.REGISTERS_PER_PWM_CHANNEL);

//// bus.set_PWM_period(0, 5.0 /* KHz */);  // PWM_ch0

//// bus.set_PWM_duty(0, 50 /* % */);

//// bus.PWM_config(0, (PWM_ON + MODE_DIR_CONTROL + INT_H_BRIDGE_ON + MOTOR_FORWARD));