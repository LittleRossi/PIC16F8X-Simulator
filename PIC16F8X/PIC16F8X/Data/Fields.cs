namespace PIC16F8X.Data
{
    public static class Fields
    {
        //ToDo: ProgrammCode with List of Class Command

        //Data Register
        private static byte[] register = new byte[256];
        private static int[] stack = new int[8];
        private static int stackPointer;
        private static byte w;
        private static int pc;

        // Status Variables
        private static decimal runtime, watchdog; //in ms
        private static int clockspeed = 4000000;  //in Hz
        private static bool sleeping;

        //Interrupts
        private static byte RBIntLastState;
        private static byte RB0IntLastState;
        private static byte RA4TimerLastState;


    }
}
