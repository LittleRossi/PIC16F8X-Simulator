namespace PIC16F8X.DataModel
{
    public static class Flags
    {
        //Status Register Flags
        public static class Status
        {
            public static readonly int C = 0;
            public static readonly int DC = 1;
            public static readonly int Z = 2;
            public static readonly int PD = 3;
            public static readonly int TO = 4;
            public static readonly int RP0 = 5;
            public static readonly int RP1 = 6;
            public static readonly int IRP = 7;
        }

        //Option Register Flags
        public static class Option
        {
            public static readonly int PS0 = 0;
            public static readonly int PS1 = 1;
            public static readonly int PS2 = 2;
            public static readonly int PSA = 3;
            public static readonly int T0SE = 4;
            public static readonly int T0CS = 5;
            public static readonly int INTEDG = 6;
            public static readonly int RBPU = 7;
        }

        // Intcon Register Flags
        public static class Intcon
        {
            public static readonly int RBIF = 0;
            public static readonly int INTF = 1;
            public static readonly int T0IF = 2;
            public static readonly int RBIE = 3;
            public static readonly int INTE = 4;
            public static readonly int T0IE = 5;
            public static readonly int EEIE = 6;
            public static readonly int GIE = 7;
        }

        public static class EECON1
        {
            public static readonly int EEIF = 4;
            public static readonly int WRERR = 3;
            public static readonly int WREN = 2;
            public static readonly int WR = 1;
            public static readonly int RD = 0;
        }

        public static void CheckZFlag(byte res)
        {
            // Check if result of operation is 0
            if (res == 0)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.Z, true); // result 0 => Set ZeroFlag
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.Z, false); // result not 0 => Reset ZeroFlag
            }
        }
    }
}
