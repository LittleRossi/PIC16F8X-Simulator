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
        private static bool sleeping; // bool if PIC is SLEEPING

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
        public static bool GetRegisterBit(byte address, int bit)
        {
            // we take the value of the register and shift it "bit" times right until the bit is on the lowest bit position
            // Now we can & it with 0000 0001 and check if the result is 1 => if its one, the bit is set => we return true
            return (1 == ((GetRegister(address) >> bit ) & 1));
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
        public static byte BankAddressResolution(byte address)
        {
            // Check which bank is selected
            if (GetRegisterBit(Registers.STATUS, Flags.Status.RP0)) // Check if RP0 is set
            {
                //RP0 set => bank 1 is selected
                return (byte)(address + 0x80); // we add 0x80 in order to get to the correct address in bank1
            }
            if (address >= register.Length)
            {
                throw new IndexOutOfRangeException(); // throw error is address is out of range
            }

            //RP0 not set => bank0 selected
            return address;
        }
        public static void DirectionalWrite(byte d, byte f, byte res)
        {
            // Check Destinationbit to know if we want to write result in w register or to address f
            if(d == 0)
            {
                SetRegisterW(res); // DestinationBit = 0 => result in w register
            }
            else if (d == 128)
            {
                SetRegister(BankAddressResolution(f), res); // DestinationBit = 1 => result in f register
            }
        }
        public static void ToggleSingleRegisterBit(byte address, int bit)
        {
            if (GetRegisterBit(address, bit))
                SetSingleRegisterBit(address, bit, false); // if bit is set => reset it
            else SetSingleRegisterBit(address, bit, true); // if bit is not set => set it
        }
        #endregion

        #region Programmcounter
        public static void IncreasePC()
        {
            if (pc < 1023) pc++;
            else pc = 0;

            SetPCLfromPC();
        }
        public static void SetPCLfromPC()
        {
            //We need to also set the PCL Register which represents the lower 8 Bits of the PC
            byte pcl = BitConverter.GetBytes(pc)[0];
            register[Registers.PCL] = pcl;
            register[Registers.PCL2] = pcl;
        }

        public static void SetPC(int newValue)
        {
            pc = newValue;
            SetPCLfromPC();
        }
        #endregion

        #region Pre- and Postscaler
        public static void ResetPrePostScaler()
        {
            prePostscaler = 0;
        }
        public static void SetPrePostscalerRatio() { }
        #endregion

        #region Watchdog
        public static void WatchDogTimerReset()
        {
            // Set Watchdog to 0
            ResetWatchdog();

            // RESET CONDITION FOR PROGRAM COUNTER AND THE STATUS REGISTER

            if (IsSleeping())
            {
                // WDT Reset Wake-up (during SLEEP)
               
                //STATUS Register (uuu0 0uuu)
                SetSingleRegisterBit(Registers.STATUS, Flags.Status.PD, false);
                SetSingleRegisterBit(Registers.STATUS, Flags.Status.TO, false);

                // Increase Programmcounter
                IncreasePC(); 

                // Wake-up PIC
                SetSleeping(false);
            }
            else
            {
                // WDT Reset during normal operation

                //Reset Programmcounter
                SetPC(0);

                // Reset W-Register
                SetRegisterW(0);

                //STATUS Register (0000 1uuu)
                SetRegister(Registers.STATUS, (byte)((GetRegister(Registers.STATUS) & 7) + 0x08));

                //OPTION Register (1111 1111)
                SetRegister(Registers.OPTION, 0xFF);

                //PCLATH Register (0000 0000)
                SetRegister(Registers.PCLATH, 0x00);
            }



        }
        #endregion

        #region Runtime
        public static void IncreaseRuntime()
        {
            // Increase the Runtime of the amount of one Execution
            runtime += GetSingleExecutionTime();
        }

        private static long GetSingleExecutionTime()
        {
            // calculate the ExecutionTime for the current clockspeed
            // quarzfrequenzy of 4MHz => results in 1μs ExecutionTime
            return (4000000 / clockspeed); //4000000 / 4000000 => 1, if clockspeed is changed the ExecutionTime is different
        }

        #endregion

        #region Interrupts
        public static bool CheckForInterrupts()
        {
            // Check Global Interrups Enabled (GIE) is true
            if (GetRegisterBit(Registers.INTCON, Flags.Intcon.GIE))
            {
                // Check the INTERRUPT LOGIC
                if (
                GetRegisterBit(Registers.INTCON, Flags.Intcon.T0IF) && GetRegisterBit(Registers.INTCON, Flags.Intcon.T0IE) ||
                GetRegisterBit(Registers.INTCON, Flags.Intcon.INTF) && GetRegisterBit(Registers.INTCON, Flags.Intcon.INTE) ||
                GetRegisterBit(Registers.INTCON, Flags.Intcon.RBIF) && GetRegisterBit(Registers.INTCON, Flags.Intcon.RBIE)
                )
                {
                    return true;
                }
            }
            // Return false, if GIE is false => Interrupts are not enabled
            return false;
        }
        #endregion

        #region Stack
        public static void PushOnStack()
        {
            //push the current PC on the Stack
            stack[stackPointer] = pc;

            if (stackPointer == 7) stackPointer = 0;
            else stackPointer++;
        }
        public static int PopStack()
        {
            if (stackPointer == 0) stackPointer = 7;
            else stackPointer--;

            // Return PC from Stack on current Pointer index
            return stack[stackPointer]
        }
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

        public static void SetSleeping(bool newStatus)
        {
            sleeping = newStatus;
        }

        public static bool IsSleeping()
        {
            return sleeping;
        }

        #endregion
    }
}
