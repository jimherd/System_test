//
//
//

using MotionIOLibrary;
using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using LiveCharts;

namespace System_test {

    public partial class Form1 : Form {

        private const bool Debug = true;

        //***********************************************************************
        // Constant definitions
        //***********************************************************************

        //private const int SUCCESS = 0;

        private const int DEFAULT_PORT = 9;

        private string[] baud_rates = new string[] { "256000", "230400", "128000", "115200", "9600" };

        private const double MIN_PWM_FREQUENCY = 0.01;   //kHz
        private const double MAX_PWM_FREQUENCY = 100.0;  //kHz

        const int SUCCESS       = 0;
        const int COMBAUD       = 115200;
        const int READ_TIMEOUT  = 10000;   // timeout for read reply (10 seconds)


        //***********************************************************************
        // variables and methods
        //***********************************************************************

        private UInt32 nos_PWM_units = 0;
        private UInt32 nos_QE_units = 0;
        private UInt32 nos_RC_units = 0;

        private Boolean connected = false;
        private UInt32  global_error;

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

        //***********************************************************
        // do_command : execute command on uP/FPGA system
        // ==========

        public FPGA_uP_IO.ErrorCode do_command(string command, out int data)
        {
            string reply_string;

            FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.ErrorCode.NO_ERROR;
            //
            // if in Debug mode execute a local version of 'do_command' thet
            // writes received debug information to a debug window.
            //
            
            if (!Debug) {
                int tmp_data = 0;

                status = FPGA_uP_IO.do_command(command, out tmp_data);
                data = tmp_data;
                return status;
            }

            reply_string = "\n";
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


        //***********************************************************************
        // User functions
        //*********************************************************************** 
        // get_reply : Read a status/data reply from LLcontrol subsystem
        //
        public Int32 get_reply()
        {
            string reply;
            Int32 status;

            //serialPort1.DiscardInBuffer();
            serialPort1.ReadTimeout = READ_TIMEOUT;
            try
            {
                reply = serialPort1.ReadLine();
            }
            catch (TimeoutException)
            {
                DebugWindow.AppendText("ReadLine timeout fail" + Environment.NewLine);
                return -1;
            }
            DebugWindow.AppendText("Reply = " + reply);
            return SUCCESS;
        }
 
        //***********************************************************************
        // Window interface functions
        //*********************************************************************** 

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
            comboBox3.SelectedIndex = 2;


            serialPort1.BaudRate = COMBAUD;

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

            if (radioButton17.Checked == true) {
                status = FPGA_uP_IO.ping_uP();
                InfoWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                InfoWindow.AppendText("Ping uP : Return code = " + status + Environment.NewLine);
            }
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
            if (radioButton18.Checked == true) {
                status = FPGA_uP_IO.restart_FPGA();
                InfoWindow.AppendText("Port = " + FPGA_uP_IO.int_parameters[0] + Environment.NewLine);
                InfoWindow.AppendText("Restart FPGA : Return code = " + status + Environment.NewLine);
            }
        }

        //****************************************************************
        // Run commands to control a PWM channel
        //
        // Notes
        // * Will only affect "bit 0" of PWM config register. Other 31 bits should be undisturbed.

        private void Execute_PWM_cmd(object sender, EventArgs e)
        {
            string command_str;

            int channel_base_address, on_time_nS, on_time_FPGA_count, data, period_time_FPGA_count;
            int PWM_status;
            double period_time_nS;
            FPGA_uP_IO.ErrorCode status;

            status = FPGA_uP_IO.ErrorCode.NO_ERROR;
            int config_value = 0;

            // get values from PWM form and make checks

            int PWM_channel = (int)numericUpDown5.Value;
            double PWM_frequency = double.Parse(textBox3.Text);
            if ((PWM_frequency < MIN_PWM_FREQUENCY) || (PWM_frequency > MAX_PWM_FREQUENCY)) {
                MessageBox.Show("Input PWM frequenct in range 0.01 to 100 (kHz)");
                return;
            }
            int PWM_width_percent = Convert.ToInt32(numericUpDown6.Value);

            // Calculate FPGA/uP command parameters

            channel_base_address = (Int32)(FPGA_uP_IO.PWM_base + (PWM_channel * FPGA_uP_IO.REGISTERS_PER_PWM_CHANNEL));
            period_time_nS = Convert.ToInt32(1000000.0 / PWM_frequency);
            period_time_FPGA_count = (int)(period_time_nS / 20);
            on_time_nS = Convert.ToInt32((PWM_width_percent * period_time_nS) / 100);
            on_time_FPGA_count = on_time_nS / 20;

            // check mode to execute

            if (radioButton5.Checked == true) {     // full configuration
                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.PERIOD), period_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (do_command(command_str, out data));
                InfoWindow.AppendText("Return code = " + status + Environment.NewLine);

                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.ON_TIME), on_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (do_command(command_str, out data));
                InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
            }

            if (radioButton6.Checked == true) {    // set PWM ON time
                command_str = build_command('w', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.ON_TIME), on_time_FPGA_count);
                InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
                status = (do_command(command_str, out data));
                InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
            }

            if ((radioButton7.Checked == true) | (checkBox2.Checked == true)) {    // enable PWM channel
                config_value = 1;
            }

            if (radioButton8.Checked == true) {    // disable PWM channel
                config_value = 0;
            }

            // Process configuration data. Ensure that only bit-1 is changed. 

            command_str = build_command('r', DEFAULT_PORT,
                                            (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), config_value);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out PWM_status));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);

            config_value = ((int)(PWM_status & 0xFFFF0000) | (int)(config_value));

            if (radioButton4.Checked == true) {  // override to clear config register
                config_value = 0;
            }

            command_str = build_command('w', DEFAULT_PORT,
                                        (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), config_value);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out data));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Run command to read a specified register
        private void Read_Register_Click(object sender, EventArgs e)
        {
            int data;

            int count = (int)numericUpDown8.Value;

            for (int i = 0; i < count; i++) {
                FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.execute_command('r', DEFAULT_PORT, (int)numericUpDown2.Value, 0, out data);
                InfoWindow.AppendText("Register Value = " + data + "/0x" + Convert.ToString(data, 16) + Environment.NewLine);
                InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
            }
        }

        //****************************************************************
        // Run command to write to a specified register
        private void Write_Register_Click(object sender, EventArgs e)
        {
            int out_data;

            int data = int.Parse(textBox1.Text, System.Globalization.NumberStyles.HexNumber);
            FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.execute_command('w', DEFAULT_PORT, (int)numericUpDown1.Value, data, out out_data);
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Run commands to execute RC servo commands
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

        

        //****************************************************************
        // Set the H-bridge configuration register
        private void Set_H_bridge_config_Click(object sender, EventArgs e)
        {
            int config_value, PWM_config, data, command;
            FPGA_uP_IO.ErrorCode status;

            config_value = 0;
            int PWM_channel = (int)numericUpDown5.Value;
            int channel_base_address = (int)(FPGA_uP_IO.PWM_base + (PWM_channel * FPGA_uP_IO.REGISTERS_PER_PWM_CHANNEL));
            int motor_cmd = (int)comboBox3.SelectedIndex;

            // get current value of PWM configuration 

            string command_str = build_command('r', DEFAULT_PORT,
                                               (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), 0);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out PWM_config));

            // create new configuration value

            if (checkBox3.Checked == true) {
                config_value = ((int)(config_value & (~FPGA_Sys.INT_H_BRIDGE_MASK)) | (int)FPGA_Sys.INT_H_BRIDGE.ENABLE);
            }
            else {
                config_value = (int)(config_value & (~FPGA_Sys.INT_H_BRIDGE_MASK));
            }

            if (checkBox4.Checked == true) {
                config_value = ((int)(config_value & (~FPGA_Sys.EXT_H_BRIDGE_MASK)) | (int)FPGA_Sys.EXT_H_BRIDGE.ENABLE);
            }
            else {
                config_value = (int)(config_value & (~FPGA_Sys.EXT_H_BRIDGE_MASK));
            }

            if (radioButton14.Checked == true) {   // PWM/PWM H-bridge mode
                config_value = ((int)(config_value & (~FPGA_Sys.H_BRIDGE_MODE_MASK)) | (int)FPGA_Sys.H_BRIDGE_MODE.PWM_CONTROL);
            }
            else {
                config_value = (int)(config_value & (~FPGA_Sys.H_BRIDGE_MODE_MASK));
            }

            if (radioButton3.Checked == true) {    //  PWM/DIR H-bridge mode
                config_value = ((int)(config_value & (~FPGA_Sys.H_BRIDGE_MODE_MASK)) | (int)FPGA_Sys.H_BRIDGE_MODE.DIR_CONTROL);
            }
            else {
                config_value = (int)(config_value & (~FPGA_Sys.H_BRIDGE_MODE_MASK));
            }
            if (radioButton16.Checked == true) {   // COAST during PWM dwell time
                config_value = ((int)(config_value & (~FPGA_Sys.PWM_DWELL_MODE_MASK)) | (int)FPGA_Sys.PWM_DWELL_MODE.COAST);
            } else {                               // BRAKE during PWM dwell time
                config_value = (int)(config_value & (~FPGA_Sys.PWM_DWELL_MODE_MASK));
            }
            command = 0;
            Boolean change_cmd = false;
            switch (motor_cmd) {
                case 0:   // no change
                    break;
                case 1:  // COAST
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.COAST;
                    break;
                case 2:   // FORWARD
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.FORWARD;
                    break;
                case 3:   // BACKWARD
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.BACKWARD;
                    break;
                case 4:   // BRAKE
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.BRAKE;
                    break;
                default:
                    break;
            }
            if (change_cmd == true) {
                config_value = ((int)(config_value & (~FPGA_Sys.MOTOR_CMDS_MASK)) | command);
            }
            config_value = (int)(config_value & 0xFFFF0000) | (PWM_config & 0x0000FFFF);

            // Output PWM configuaration value to PWM

            command_str = build_command('w', DEFAULT_PORT,
                                        (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), config_value);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out data));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Update H-bridge command only
        private void Update_H_bridge_cmd_click(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;
            int data, PWM_config;
            
            int config_value = 0;
            int PWM_channel = (int)numericUpDown5.Value;
            int channel_base_address = (int)(FPGA_uP_IO.PWM_base + (PWM_channel * FPGA_uP_IO.REGISTERS_PER_PWM_CHANNEL));
            int motor_cmd = (int)comboBox3.SelectedIndex;

            // get current value of PWM configuration 

            string command_str = build_command('r', DEFAULT_PORT,
                                               (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), 0);
            // InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out PWM_config));

            int command = 0;
            Boolean change_cmd = false;

            switch (motor_cmd) {
                case 0:   // no change
                    break;
                case 1:  // COAST
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.COAST;
                    break;
                case 2:   // FORWARD
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.FORWARD;
                    break;
                case 3:   // BACKWARD
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.BACKWARD;
                    break;
                case 4:   // BRAKE
                    change_cmd = true;
                    command = (int)FPGA_Sys.MOTOR_CMDS.BRAKE;
                    break;
                default:
                    break;
            }
            if (change_cmd == true) {
                config_value = ((int)(config_value & (~FPGA_Sys.MOTOR_CMDS_MASK)) | command);
            }
            config_value = (int)(config_value & 0xFFFF0000) | (PWM_config & 0x0000FFFF);

            // Output PWM configuaration value to PWM

            command_str = build_command('w', DEFAULT_PORT,
                                        (channel_base_address + (int)FPGA_Sys.PWM_REG.CONFIG), config_value);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out data));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        //****************************************************************
        // Set the QE (Quadrature Encoder) configuration register
        private void Set_QE_config_Click(object sender, EventArgs e)
        {
            int data, QE_config;
            FPGA_uP_IO.ErrorCode status;
            int config_value = 0;
            int QE_channel = (int)numericUpDown7.Value;
            int channel_base_address = (int)(FPGA_uP_IO.QE_base + (QE_channel * FPGA_uP_IO.REGISTERS_PER_QE_CHANNEL));

            // get current value of QE configuration 

            string command_str = build_command('r', DEFAULT_PORT,
                                               (channel_base_address + (int)FPGA_Sys.QE_REG.CONFIG), 0);
            // InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out QE_config));

            //
            // build updated configuration value

            if (checkBox5.Checked == true) {    // enable QE channel
                QE_config = QE_config | (int)FPGA_Sys.QE_ENABLE_MASK;
            } else {
                QE_config = (QE_config & ~(int)(FPGA_Sys.QE_ENABLE_MASK));
            }

            if (checkBox6.Checked == true) {    // enable speed measurement
                QE_config = QE_config | (int)FPGA_Sys.QE_SPEED_CALC_ENABLE_MASK;
            }
            else {
                QE_config = (QE_config & ~(int)(FPGA_Sys.QE_SPEED_CALC_ENABLE_MASK));
            }

            if (checkBox7.Checked == true) {    // enable speed averaging filter
                QE_config = QE_config | (int)FPGA_Sys.QE_ENABLE_SPEED_FILTER_MASK;
                int filter_sample_code = ((int)(comboBox4.SelectedIndex) << 20);  // convert to bit value
                QE_config = QE_config & ~(int)FPGA_Sys.QE_SPEED_FILTER_SAMPLE_SIZE_MASK;
                QE_config = QE_config | filter_sample_code;
            }
            else {
                QE_config = (QE_config & ~((int)(FPGA_Sys.QE_ENABLE_SPEED_FILTER_MASK) |(int)FPGA_Sys.QE_SPEED_FILTER_SAMPLE_SIZE_MASK));
            }

            command_str = build_command('w', DEFAULT_PORT,
                                        (channel_base_address + (int)FPGA_Sys.QE_REG.CONFIG), QE_config);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out data));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;
            int data;

            int QE_channel = (int)numericUpDown7.Value;
            int channel_base_address = (int)(FPGA_uP_IO.QE_base + (QE_channel * FPGA_uP_IO.REGISTERS_PER_QE_CHANNEL));
            string command_str = build_command('w', DEFAULT_PORT,
                                        (channel_base_address + (int)FPGA_Sys.QE_REG.CONFIG), (int)FPGA_Sys.QE_CONFIG_DEFAULT);
            InfoWindow.AppendText("command = " + command_str + Environment.NewLine);
            status = (do_command(command_str, out data));
            InfoWindow.AppendText("Return code = " + status + Environment.NewLine);
        }

        private void Close__COM_port_Click(object sender, EventArgs e)
        {
            

            FPGA_uP_IO.ErrorCode status = FPGA_uP_IO.Close_comms();

            if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                InfoWindow.AppendText("Cannot close COM port" + Environment.NewLine);
                return;
            }

            InfoWindow.AppendText("COM port now closed" + Environment.NewLine);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String command = "p 5 ";

            DebugWindow.AppendText(command + Environment.NewLine);
            serialPort1.WriteLine(command);
            DebugWindow.AppendText("Wating for reply" + Environment.NewLine);
            int status = get_reply();
        }
        string[] sequence_1 = new[] {
            "P4 9\n",              // reset system
            "w 9 1 5000\n",        // set period
            "w 9 2 3750\n",        // set PWM on-time
            "w 9 3 34930689\n",    // set configuration for PWM and H-bridge
            "w 9 14 65537\n",      // set quadrature encoder with velocity measurement
        };
        string[] sequence_2 = new[] {
            "P4 0"
        };

        private void execute_sequence__Click(object sender, EventArgs e)
        {
            FPGA_uP_IO.ErrorCode status;
            int data;

            if (radioButton19.Checked == true) {
                for (int i = 0; i < sequence_1.Length; i++) {
                    status = (do_command(sequence_1[i], out data));
                    if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                        break;
                    }
                }
                return;
            }
            if (radioButton20.Checked == true) {
                for (int i = 0; i < sequence_2.Length; i++) {
                    status = (do_command(sequence_2[i], out data));
                    if (status != FPGA_uP_IO.ErrorCode.NO_ERROR) {
                        break;
                    }
                }
                return;

            }
        }
    }
}

