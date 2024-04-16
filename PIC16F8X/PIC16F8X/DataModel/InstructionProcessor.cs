namespace PIC16F8X.DataModel
{
    public static class InstructionProcessor
    {
        #region Instruction Implementation

        //BYTE-ORIENTED FILE REGISTER OPERATIONS

        public static void ADDWF(Command com)
        {
            // logical & with bitmask to get argument
        }

        public static void ANDWF(Command com) { }

        public static void CLRF(Command com) { }

        public static void CLRW(Command com) { }

        public static void COMF(Command com) { }

        public static void DECF(Command com) { }

        public static void DECFSZ(Command com) { }

        public static void INCF(Command com) { }

        public static void INCFSZ(Command com) { }

        public static void IORWF(Command com) { }

        public static void MOVF(Command com) { }

        public static void MOVWF(Command com) { }

        public static void NOP(Command com) { }

        public static void RLF(Command com) { }

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
