using System;
using System.Collections.Generic;

namespace PIC16F8X.DataModel
{
    public static class Fields
    {
        //ToDo: ProgrammCode with List of Class Command
        private static List<Command> program = new List<Command>();

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

        //Integrity
        private static bool ProgrammInitialized;








        #region Getter and Setter
        public static void SetProgramm(List<string> commandList)
        {
            //Create a list of Commands out of list of read hexadecimal commands

            if (commandList == null) throw new ArgumentNullException();

            program = new List<Command>();

            for (int i = 0; i < commandList.Count; i++)
            {
                Command line = new Command(commandList[i]);
                program.Add(line);
            }

            Console.WriteLine(program);

            ProgrammInitialized = true;
        }

        public static int[] GetStack()
        {
            return stack;
        }

        public static int GetPc()
        {
            return pc;
        }

        public static decimal GetRuntime()
        {
            return runtime;
        }

        public static decimal GetWatchdog()
        {
            return watchdog;
        }

        public static void ResetWatchdog()
        {
            watchdog = 0;
        }

        public static byte[] GetAllRegister()
        {
            return register;
        }

        public static byte GetRegisterW()
        {
            return w;
        }

        public static void SetRegisterW(byte newValue)
        {
            w = newValue;
        }

        #endregion
    }
}
