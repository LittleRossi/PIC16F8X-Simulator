﻿using System.Collections.Generic;

namespace PIC16F8X.DataModel
{
    public static class Fields
    {
        //ToDo: ProgrammCode with List of Class Command
        private static List<Command> programm = new List<Command>();

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

        //Integrety
        private static bool ProgrammInitialized;




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

    }
}
