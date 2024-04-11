namespace PIC16F8X.Data
{
    public static class Instructions
    {
        public enum Instruction
        {
            ADDWF, ANDWF, CLRF, CLRW, COMF, DECF, DECFSZ, INCF, INCFSZ, IORWF, MOVF, MOVWF, NOP, RLF, RRF, SUBWF, SWAPF, XORWF,
            BCF, BSF, BTFSC, BTFSS,
            ADDLW, ANDLW, CALL, CLRWDT, GOTO, IORLW, MOVLW, RETFIE, RETLW, RETURN, SLEEP, SUBLW, XORLW,
            UNKNOWNCOMMAND
        }
    }
}
