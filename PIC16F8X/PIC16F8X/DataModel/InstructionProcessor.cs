namespace PIC16F8X.DataModel
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
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // Rotate left
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) << 1);

            // Add CarryFlag to result if its set
            if (Fields.GetRegisterBit(Registers.STATUS, Flags.Status.C)) result++;

            // Set CarryFlag for the current Calculation
            // Check if 8th Bit is set 
            if ((Fields.GetRegister(Fields.BankAddressResolution(f)) & 128) == 128)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, true); // 8th bit is 1 => set Carry
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, false); // 8th bit is 0 => reset Carry
            }

            Fields.DirectionalWrite(d, f, result);
        }
        public static void RRF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            //rotate right
            byte result = (byte)(Fields.GetRegister(Fields.BankAddressResolution(f)) >> 1);

            // Add CarryFlag to result if its set
            if (Fields.GetRegisterBit(Registers.STATUS, Flags.Status.C)) result+= 128; //adding carryflag to 8th bit


            // Set CarryFlag for the current Calculation
            // Check if 1th Bit is set 
            if ((Fields.GetRegister(Fields.BankAddressResolution(f)) & 1) == 1)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, true); // 1th bit is 1 => set Carry
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, false); // 1th bit is 0 => reset Carry
            }

            Fields.DirectionalWrite(d, f, result);
        }
        public static void SUBWF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            byte result = ArithmeticLogicUnit.BitwiseSubstract(Fields.GetRegister(Fields.BankAddressResolution(f)), Fields.GetRegisterW());

            Fields.DirectionalWrite(d, f, result);
        }
        public static void SWAPF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            // logical AND with 0000 1111 to isolate lower 4 bits and shift 4 bits left, logical AND 1111 0000 to isolate high 4 bits and shift 4 bits right and logical OR both together to change positions
            byte result = (byte)((Fields.GetRegister(Fields.BankAddressResolution(f)) & 0x0F) << 4 | (Fields.GetRegister(Fields.BankAddressResolution(f)) & 0xF0) >> 4);

            Fields.DirectionalWrite(d, f, result);
        }
        public static void XORWF(Command com)
        {
            byte f = (byte)(com.GetLowByte() & 127);
            byte d = (byte)(com.GetLowByte() & 128);

            byte result = (byte)(Fields.GetRegisterW() ^ Fields.GetRegister(Fields.BankAddressResolution(f)));
            Flags.CheckZFlag(result);

            Fields.DirectionalWrite(d, f, result);
        }


        //BIT-ORIENTED FILE REGISTER OPERATIONS

        public static void BCF(Command com)
        {
            int b1 = (com.GetHighByte() & 3) << 1; //Get the 2 last bits of HighByte and shift them to the left to get correct position
            int b = b1 + (((com.GetLowByte() & 128) == 128) ? 1 : 0); // get 8th bit of HighByte and check if its 128 if yes add 1 to b
            byte f = (byte)(com.GetLowByte() & 127);

            Fields.SetSingleRegisterBit(Fields.BankAddressResolution(f), b, false);
        }
        public static void BSF(Command com)
        {
            int b1 = (com.GetHighByte() & 3) << 1; //Get the 2 last bits of HighByte and shift them to the left to get correct position
            int b = b1 + (((com.GetLowByte() & 128) == 128) ? 1 : 0); // get 8th bit of HighByte and check if its 128 if yes add 1 to b
            byte f = (byte)(com.GetLowByte() & 127);

            Fields.SetSingleRegisterBit(Fields.BankAddressResolution(f), b, true);
        }
        public static void BTFSC(Command com)
        {
            int b1 = (com.GetHighByte() & 3) << 1; //Get the 2 last bits of HighByte and shift them to the left to get correct position
            int b = b1 + (((com.GetLowByte() & 128) == 128) ? 1 : 0); // get 8th bit of HighByte and check if its 128 if yes add 1 to b
            byte f = (byte)(com.GetLowByte() & 127);

            // check if bit b in register f != 1
            if (Fields.GetRegisterBit(Fields.BankAddressResolution(f), b) == false)
            {
                Fields.IncreasePC();
                // Skip Cycle
            }
        } //ToDo: Skip Cycle
        public static void BTFSS(Command com)
        {
            int b1 = (com.GetHighByte() & 3) << 1; //Get the 2 last bits of HighByte and shift them to the left to get correct position
            int b = b1 + (((com.GetLowByte() & 128) == 128) ? 1 : 0); // get 8th bit of HighByte and check if its 128 if yes add 1 to b
            byte f = (byte)(com.GetLowByte() & 127);

            // check if bit b in register f == 1
            if (Fields.GetRegisterBit(Fields.BankAddressResolution(f), b) == true)
            {
                Fields.IncreasePC();
                // Skip Cycle
            }
        } //ToDo: Skip Cycle


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
