using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;

namespace System_test {
    class FPGA_Sys {

        enum CMD { 
            READ_REGISTER  = 0, 
            WRITE_REGISTER = 1 };
        enum BUS { 
            READ  = 0, 
            WRITE = 1 };
        enum LEVEL { 
            LOW  = 0, 
            HIGH = 1 };

        //////////////////////////////////////////////////////////////////////////
        // SYS_data registers

        const UInt32 SYS_DATA_REG_ADDR = 0;

        //////////////////////////////////////////////////////////////////////////
        // PWM registers with relative addresses
        //
        public enum PWM_REG { 
            PERIOD  = 0, 
            ON_TIME = 1, 
            CONFIG  = 2, 
            STATUS  = 3 };
        //
        // constants and mask definitions for PWM config register

        public const UInt32 PWM_CONFIG_DEFAULT  =  0b0;
        public const uint   INT_H_BRIDGE_MASK   = (0b1   << 16);
        public const UInt32 EXT_H_BRIDGE_MASK   = (0b1   << 17);
        public const UInt32 H_BRIDGE_MODE_MASK  = (0b11  << 18);
        public const UInt32 MOTOR_CMDS_MASK     = (0b111 << 20);
        public const UInt32 SWAP_PINS_MASK      = (0b1   << 24);
        public const UInt32 PWM_DWELL_MODE_MASK = (0b1   << 25);
        public const UInt32 INVERT_PINS_MASK    = (0b11  << 26);

        public enum PWM { 
            DISABLE = 0x0, 
            ENABLE  = 0x1 };
        public enum INT_H_BRIDGE { 
            DISABLE = 0x0, 
            ENABLE  = 0x10000 };
        public enum EXT_H_BRIDGE { 
            DISABLE = 0x0, 
            ENABLE  = 0x20000 };
        public enum H_BRIDGE_MODE { 
            PWM_CONTROL = 0x0, 
            DIR_CONTROL = 0x40000 };
        public enum MOTOR_CMDS { 
            COAST    = 0x0, 
            FORWARD  = 0x100000, 
            BACKWARD = 0x200000, 
            BRAKE    = 0x300000 };
        public enum SWAP_PINS { 
            NO  = 0x0, 
            YES = 0x1000000 };
        public enum PWM_DWELL_MODE { 
            BRAKE = 0x0, 
            COAST = 0x2000000 };
        public enum INVERT_PINS { 
            NONE           = 0x0, 
            INVERT_IN1     = 0x4000000, 
            INVERT_IN2     = 0x8000000, 
            INVERT_IN1_IN2 = 0xC000000 };
        public enum PWM_DIRECTION { 
            BACKWARD, 
            FORWARD };

        //////////////////////////////////////////////////////////////////////////
        // QE registers with relative addresses
        //
        public enum QE_REG {
            BUFFER         = 0, 
            TURN_BUFFER    = 1, 
            SPEED_BUFFER   = 2, 
            SIM_PHASE_TIME = 3,
            COUNTS_PER_REV = 4, 
            CONFIG         = 5, 
            STATUS         = 6
        };

        //
        // constants and mask definitions for QE config register

        public const int QE_CONFIG_DEFAULT = 0b0;
        //
        public const int QE_ENABLE_MASK                   = (0b1);
        public const int QE_SIGNAL_SOURCE_MASK            = (0b1   << 1);
        public const int QE_SIMULATE_ENABLE_MASK          = (0b1   << 2);
        public const int QE_SIMULATE_DIRECTION_MASK       = (0b1   << 3);   
        public const int QE_SIMULATE_SWAP_A_B_MASK        = (0b1   << 4);
        public const int QE_SPEED_CALC_ENABLE_MASK        = (0b1   << 16);
        public const int QE_ENABLE_SPEED_FILTER_MASK      = (0b1   << 17);
        public const int QE_SPEED_FILTER_SAMPLE_SIZE_MASK = (0b111 << 20);
        //
        // constants to define bits in QE config register

        public enum QE_ENABLE {
            NO = 0x0,
            YES = 0x1
        };
        public enum QE_SIG { 
            EXT     = 0x00, 
            INT_SIM = 0x02 
        };
        public enum QE_INT { 
            SIM_DISABLE = 0x0, 
            SIM_ENABLE  = 0x04 
        };
        public enum QE_SIM { 
            DIR_FORWARD  = 0x0, 
            DIR_BACKWARD = 0x08 
        };
        public enum QE_SWAP_AB { 
            NO  = 0x00, 
            YES = 0x10 
        };
        public enum QE_SPEED_CALC { 
            DISABLE = 0x00, 
            ENABLE  = 0x10000 
        };
        public enum QE_SPEED_CALC_FILTER { 
            DISABLE = 0x00, 
            ENABLE  = 0x20000 
        };
        public enum QE_FILTER_SAMPLE : int {
            X2  = 0x00, 
            X4  = 0x100000, 
            X8  = 0x200000, 
            X16 = 0x300000, 
            X32 = 0x400000
        };

        //////////////////////////////////////////////////////////////////////////   
        // RC servo registers with relative addresses
        
        public enum RC_SERVO_REG { 
            PERIOD  = 0, 
            CONFIG  = 1, 
            STATUS  = 2, 
            ON_TIME = 3 };   // base address os set of RC servo channels

   
    }
}
