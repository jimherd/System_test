using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;

namespace System_test {
    class FPGA_Sys {

        enum CMD { READ_REGISTER = 0, WRITE_REGISTER = 1 };
        enum BUS { READ = 0, WRITE = 1 };
        enum LEVEL { LOW = 0, HIGH = 1 };

        //////////////////////////////////////////////////////////////////////////
        // SYS_data registers

        const UInt32 SYS_DATA_REG_ADDR = 0;

        //////////////////////////////////////////////////////////////////////////
        // PWM registers with relative addresses
        //
        public enum PWM_REG { PERIOD = 0, ON_TIME = 1, CONFIG = 2, STATUS = 3 };
        //
        // constants to define bits in PWM config register

        const UInt32 PWM_CONFIG_DEFAULT = 0x00;
        enum PWM { DISABLE = 0x0, ENABLE = 0x1 };
        enum INT_H_BRIDGE { DISABLE = 0x0, ENABLE = 0x10000 };
        enum EXT_H_BRIDGE { DISABLE = 0x0, ENABLE = 0x20000 };
        enum PWM_MODE { PWM_CONTROL = 0x0, DIR_CONTROL = 0x40000 };
        enum MOTOR { COAST = 0x0, FORWARD = 0x100000, BACKWARD = 0x200000, BRAKE = 0x300000 };
        enum SWAP { NO = 0x0, YES = 0x1000000 };
        enum PWM_DWELL { BRAKE = 0x0, COAST = 0x2000000 };
        enum INVERT { NONE = 0x0, INVERT_IN1 = 0x4000000, INVERT_IN2 = 0x8000000, INVERT_IN1_IN2 = 0xC000000 };
        enum PWM_DIRECTION { BACKWARD, FORWARD };

        //////////////////////////////////////////////////////////////////////////
        // QE registers with relative addresses
        //
        enum
        QE_COUNT_REG {
            BUFFER = 0, TURN_BUFFER = 1, SPEED_BUFFER = 2, SIM_PHASE_TIME = 3,
            COUNTS_PER_REV = 4, CONFIG = 5, STATUS = 6
        };
        //
        // constants to define bits in QE config register

        const UInt32 QE_CONFIG_DEFAULT = 0x00;
        enum QE_SIG { EXT = 0x00, INT_SIM = 0x02 };
        enum QE_INT { SIM_DISABLE = 0x0, SIM_ENABLE = 0x04 };
        enum QE_SIM { DIR_FORWARD = 0x0, DIR_BACKWARD = 0x08 };
        enum QE_SWAP_AB { NO = 0x00, YES = 0x10 };
        enum QE_SPEED_CALC { DISABLE = 0x00, ENABLE = 0x10000 };
        enum QE_SPEED_CALC_FILTER { DISABLE = 0x00, ENABLE = 0x20000 };
        enum QE_FILTER_SAMPLE {
            X2 = 0x00, X4 = 0x100000, X8 = 0x200000, X16 = 0x300000, X32 = 0x400000
        };

        //////////////////////////////////////////////////////////////////////////   
        // RC servo registers with relative addresses
        //
        enum RC_SERVO_REG { PERIOD = 0, CONFIG = 1, STATUS = 2, ON_TIME = 3 };

   
    }
}
