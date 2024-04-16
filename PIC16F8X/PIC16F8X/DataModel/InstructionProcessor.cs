﻿namespace PIC16F8X.DataModel
{
    public static class InstructionProcessor
    {
        #region Instruction Implementation

        //BYTE-ORIENTED FILE REGISTER OPERATIONS

        public static void ADDWF(Command com)
        {
            // ADDWF Encoding: LowByte: (dfff ffff)
            byte d = (byte)(com.GetLowByte() & 128);  // filter destination bit with mask (1000 0000)
            byte f = (byte)(com.GetLowByte() & 127); // filter f byte with mask (0111 1111)

            // perform Add
            byte result = ArithmeticLogicUnit.BitwiseAdd( Fields.GetRegisterW(), Fields.GetRegister(Fields.BankAddressResolution(f) ));

            // write result in register
            Fields.DirectionalWrite(d, f, result);
        }
        public static void ANDWF(Command com)
        {
            // ANDWF Encoding: LowByte: (dfff ffff)
            byte d = (byte)(com.GetLowByte() & 128);  // filter destination bit with mask (1000 0000)
            byte f = (byte)(com.GetLowByte() & 127); // filter f byte with mask (0111 1111)

            // perform AND
            byte result = (byte)(Fields.GetRegisterW() & Fields.GetRegister(Fields.BankAddressResolution(f)));

            // write result in register
            Fields.DirectionalWrite(d, f, result);
        }
        public static void CLRF(Command com)
        {
            // isolate f in Lowbyte by logical AND with 0111 1111
            byte f = (byte)(com.GetLowByte() & 127);

            // Clear register by setting it to 0
            Fields.SetRegister(Fields.BankAddressResolution(f), 0);

            // Set the Zeroflag
            Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.Z, true);
        }
        public static void CLRW(Command com)
        {
            // Clear w register
            Fields.SetRegisterW(0);

            // Set the Zeroflag
            Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.Z, true);
        }
        public static void COMF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // get value of register and build the complement through negating
            byte result = (byte)~(Fields.GetRegister(Fields.BankAddressResolution(f)));

            // set Zeroflag if result = 0
            Flags.CheckZFlag(result);

            Fields.DirectionalWrite(d, f, result);
        }
        public static void DECF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // get the value out of the Register and decrement it
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) - 1);

            Flags.CheckZFlag(result);

            Fields.DirectionalWrite(d, f, result);
        }
        public static void DECFSZ(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // get the value out of the Register and decrement it
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) - 1);

            if(result == 0)
            {
                // if Result = 0 we skip the next instruction
                Fields.IncreasePC();

                //ToDo:
                // CycleSkip!!!
            }
        } //ToDo: SkipCycle
        public static void INCF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // get the value out of the Register and increments it
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) + 1);

            Flags.CheckZFlag(result);

            Fields.DirectionalWrite(d, f, result);
        }
        public static void INCFSZ(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // get the value out of the Register and decrement it
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) + 1);

            if (result == 0)
            {
                // if Result = 0 we skip the next instruction
                Fields.IncreasePC();

                //ToDo:
                // CycleSkip!!!
            }


            Fields.DirectionalWrite(d, f, result);
        } //ToDo: SkipCycle
        public static void IORWF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // ORs the value in f register with the w register
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) | Fields.GetRegisterW());

            Flags.CheckZFlag(result);

            Fields.DirectionalWrite(d, f, result);
        }
        public static void MOVF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            Flags.CheckZFlag(Fields.GetRegister(Fields.BankAddressResolution(f)));

            if (d != 128)
            {
                //If Destinationbit is not set => store in W Register
                Fields.SetRegisterW(Fields.GetRegister(Fields.BankAddressResolution(f)));
            }
        }
        public static void MOVWF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);

            // get value of w register and set it to f register
            Fields.SetRegister(Fields.BankAddressResolution(f), Fields.GetRegisterW());
        }
        public static void NOP(Command com)
        {
            //No Operation instruction
        }

        public static void RLF(Command com)
        {

        }

        public static void RRF(Command com) { }

        public static void SUBWF(Command com) { }

        public static void SWAPF(Command com) { }

        public static void XORWF(Command com) { }


        //BIT-ORIENTED FILE REGISTER OPERATIONS

        public static void BCF(Command com) { }

        public static void BSF(Command com) { }

        public static void BTFSC(Command com) { }

        public static void BTFSS(Command com) { }


        //LITERAL AND CONTROL OPERATIONS

        public static void ADDLW(Command com) { }

        public static void ANDLW(Command com) { }

        public static void CALL(Command com) { }

        public static void CLRWDT(Command com) { }

        public static void GOTO(Command com) { }

        public static void IORLW(Command com) { }

        public static void MOVLW(Command com) { }

        public static void RETFIE(Command com) { }

        public static void RETLW(Command com) { }

        public static void RETURN(Command com) { }

        public static void SLEEP(Command com) { }

        public static void SUBLW(Command com) { }

        public static void XORLW(Command com) { }

        #endregion
    }
}
