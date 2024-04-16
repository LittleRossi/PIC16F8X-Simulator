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

        //Watchdog
        private static readonly int watchdogLimit = 18000; //18ms watchdog time
        private static bool watchDogEnabled;

        // Watchdog postscaler and Timer prescaler
        private static int prePostscalerRatio, prePostscaler;

        //Interrupts
        private static byte RBIntLastState;
        private static byte RB0IntLastState;
        private static byte RA4TimerLastState;

        //Integrity
        private static bool ProgrammInitialized;






        #region Register Actions
        public static void ResetData()
        {
            sleeping = false;
            pc = 0;
            w = 0;
            register = new byte[256];
            stack = new int[8];
            stackPointer = 0;
            runtime = 0;
            watchdog = 0;

            // the Power-On Reset values are from the datasheet
            SetRegister(Registers.STATUS, 0x18); //Value on Power-on Reset: 0001 1000
            SetRegister(Registers.OPTION, 0xFF); //Value on Power-on Reset: 1111 1111
            SetRegister(Registers.TRISA, 0x1F);  //Value on Power-on Reset: 0001 1111 => Setting PORTA I/O-Pins
            SetRegister(Registers.TRISB, 0xFF);  //Value on Power-on Reset: 1111 1111 => Setting PORTB I/O-Pins

            SetPrePostscalerRatio();
        }
        public static void SetRegister(byte address, byte data)
        {
            // Set the data in the correct Register
            register[Convert.ToInt16(address)] = data;

            //Handling special functions
            switch (address)
            {
                case 0x00: //indirect memory access => Using FSR
                    register[GetRegister(Registers.FSR)] = data;
                    break;
                case 0x01:
                    ResetPrePostScaler();
                    //ToDo: handle TMR0
                    break;
                case 0x02: // PCL on Bank0 and Bank1
                    register[Convert.ToInt16(0x82)] = data;
                    //PCL is split to PCL on bank0 and bank1 and PCLATH, which is a 5 Bit Write buffer of the PC
                    SetPCFromBytes(GetRegister(Registers.PCLATH), GetRegister(Registers.PCL));
                    break;
                case 0x03: // STATUS Register on Bank0 and Bank1
                    register[Convert.ToInt16(0x83)] = data; // STATUS address on bank1
                    break;
                case 0x04: // FSR on Bank0 and Bank1
                    register[Convert.ToInt16(0x84)] = data; // FSR address on bank1
                    break;

                case 0x05:
                    //ToDo: PORTA Latch (TRISA)
                    break;
                case 0x06:
                    //ToDo: PORTB Latch (TRISB)
                    break;

                case 0x0A: //PCLATH on bank0 and bank1
                    register[Convert.ToInt16(0x8A)] = data; //PCLATH address on bank1
                    break;
                case 0x0B: //INTCON on bank0 and bank1
                    register[Convert.ToInt16(0x8B)] = data; //INTCON address on bank1
                    break;

                case 0x80: //indirect memory access => Using FSR on bank1
                    register[GetRegister(Registers.FSR)] = data; // FST address on bank0
                    break;
                case 0x81: //OPTION register on bank1
                    SetPrePostscalerRatio();
                    break;
                case 0x82: //PCL on bank1
                    register[Convert.ToInt16(0x02)] = data;
                    SetPCFromBytes(GetRegister(Registers.PCLATH), GetRegister(Registers.PCL));
                    break;
                case 0x83: //STATUS on bank1
                    register[Convert.ToInt16(0x03)] = data;
                    break;
                case 0x84: //FSR on bank1
                    register[Convert.ToInt16(0x04)] = data;
                    break;
                case 0x8A: //PCLATH on bank1
                    register[Convert.ToInt16(0x0A)] = data;
                    break;
                case 0x8B: //INTCON on bank1
                    register[Convert.ToInt16(0x0B)] = data;
                    break;
            }

        }
        public static void SetSingleRegisterBit(byte address, int bit, bool value)
        {
            SetRegister(address, SetSingleBit(GetRegister(address), bit, value));
        }
        public static byte GetRegister(byte address)
        {
            return address switch
            {
                0x00 => register[GetRegister(Registers.FSR)], //0x00 is INDF Register and uses content of FSR
                _ => register[Convert.ToInt16(address)], //when address is not 0x00
            };
        }
        public static void SetPCFromBytes(byte bHigh, byte bLow)
        {
            // We need to get the value of PCLATH, in order to get the correct PC value
            pc = BitConverter.ToUInt16(new byte[] { bLow, bHigh }, 0); // LittleEndian, (lowbyte first, then highbyte)
        }
        private static byte SetSingleBit(byte byteToModify, int bitIndex, bool value)
        {
            // We create a Mask with 0 and one value 1 on the bit that we want to change
            byte mask = (byte)(1 << bitIndex); //create a mask and shift 1 as far left as bitIndex indicates

            if (value)
                return byteToModify |= mask; // Change to true => logical OR
            else
                return byteToModify &= (byte)~mask; // Change to false => logical AND with negated mask (~ negates the mask)
        }
        #endregion


        #region Pre- and Postscaler
        public static void ResetPrePostScaler()
        {
            prePostscaler = 0;
        }
        public static void SetPrePostscalerRatio() { }
        #endregion



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
