namespace PIC16F8X.DataModel
{
    public static class Instructions
    {
        public enum Instruction
        {
            ADDWF, ANDWF, CLRF, CLRW, COMF, DECF, DECFSZ, INCF, INCFSZ, IORWF, MOVF, MOVWF, NOP, RLF, RRF, SUBWF, SWAPF, XORWF,
            BCF, BSF, BTFSC, BTFSS,
            ADDLW, ANDLW, CALL, CLRWDT, GOTO, IORLW, MOVLW, RETFIE, RETLW, RETURN, SLEEP, SUBLW, XORLW,
            UNKNOWN
        }

        public static Instruction InstructionDecoder(Command command)
        {
            // Gets the command High- and Lowbyte and looks up for the matching Instruction and returns the matching Enum entry
            // The OpCode of the Instructions are out of the Datasheet. In Some Cases you also need to check the lowbyte to determ the
            // correct instruction because the highbyte is identical.

            // !!!Important!!!
            // For some operations a logical AND operation with a specific mask is important to make sure that unessecary bits are 0
            // The important bits stay the same
            // a.e.: & 60 (111100): 01 00bb => 01 0000
            // => So thats why we need to & number the Highbyte before checking the switch




            //BYTE-ORIENTED FILE REGISTER OPERATIONS
            switch (command.GetHighByte())
            {
                case 7:  return Instruction.ADDWF;
                case 5:  return Instruction.ANDWF;
                case 1:  if ((command.GetLowByte() & 128) == 1) return Instruction.CLRF;
                         else return Instruction.CLRW;
                case 9:  return Instruction.COMF;
                case 3:  return Instruction.DECF;
                case 11: return Instruction.DECFSZ;
                case 10: return Instruction.INCF;
                case 15: return Instruction.INCFSZ;
                case 4:  return Instruction.IORWF;
                case 8:  return Instruction.MOVF;
                case 13: return Instruction.RLF;
                case 12: return Instruction.RRF;
                case 2:  return Instruction.SUBWF;
                case 14: return Instruction.SWAPF;
                case 6:  return Instruction.XORWF;
            }

            //LITERAL AND CONTROL OPERATIONS
            switch (command.GetHighByte())
            {
                case 57: return Instruction.ANDLW;

                // the Highbyte is all the same, so we need to look at the Lowbyte
                case 0:  if (command.GetLowByte() == 100) return Instruction.CLRWDT;
                    else if (command.GetLowByte() == 9)  return Instruction.RETFIE;
                    else if (command.GetLowByte() == 8)  return Instruction.RETURN;
                    else if (command.GetLowByte() == 99) return Instruction.SLEEP;
                    else if ((command.GetLowByte() & 159) == 0) return Instruction.NOP;
                    else return Instruction.MOVWF;

                case 56: return Instruction.IORLW;
                case 58: return Instruction.XORLW;
            }


            //BIT-ORIENTED FILE REGISTER OPERATIONS
            switch (command.GetHighByte() & 60) // (60 = 111100) 
            {
                case 16: return Instruction.BCF;
                case 20: return Instruction.BSF;
                case 24: return Instruction.BTFSC;
                case 28: return Instruction.BTFSS;
            }

            //LITERAL AND CONTROL OPERATIONS
            switch (command.GetHighByte() & 60) // (60 = 111100) 
            {
                case 48: return Instruction.MOVLW;
                case 52: return Instruction.RETLW;
            }

            //LITERAL AND CONTROL OPERATIONS
            switch (command.GetHighByte() & 56) // (56 = 111000)
            {
                case 32: return Instruction.CALL;
                case 40: return Instruction.GOTO;
            }

            //LITERAL AND CONTROL OPERATIONS
            switch (command.GetHighByte() & 62) // (62 = 111110)
            {
                case 62: return Instruction.ADDLW;
                case 60: return Instruction.SUBLW;
            }

            return Instruction.UNKNOWN; //return unknown if instructiondecoder can´t find a matching case
        }
    }
}
