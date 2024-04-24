namespace PIC16F8X.DataModel
{
    public static class Registers
    {
        //Bank 0
        public static readonly byte INDF = 0x00;
        public static readonly byte TRM0 = 0x01;
        public static readonly byte PCL = 0x02;
        public static readonly byte STATUS = 0x03;
        public static readonly byte FSR = 0x04;
        public static readonly byte PORTA = 0x05;
        public static readonly byte PORTB = 0x06;
        public static readonly byte EEDATA = 0x08;
        public static readonly byte EEADR = 0x09;
        public static readonly byte PCLATH = 0x0A;
        public static readonly byte INTCON = 0x0B;

        //Bank 1
        public static readonly byte OPTION = 0x81;
        public static readonly byte PCL2 = 0x82;
        public static readonly byte TRISA = 0x85;
        public static readonly byte TRISB = 0x86;
        public static readonly byte EECON1 = 0x88;
        public static readonly byte EECON2 = 0x89;
    }
}
