using System;
using System.Collections.Generic;
using System.Linq;

namespace PIC16F8X.DataModel
{
    public static class Fields
    {
        //ProgrammCode with List of Class Command
        private static List<Command> program = new List<Command>(1024);

        //Data Register
        private static byte[] register = new byte[256];
        private static int[] stack = new int[8];
        private static int stackPointer;
        private static byte w;
        private static int pc;
        private static byte trisALatch;
        private static byte trisBLatch;
        private static byte dataLatchA;
        private static byte dataLatchB;

        // Status Variables
        private static decimal runtime, watchdog; //in ms
        private static int clockspeed = 4000000;  //in Hz
        private static bool sleeping; // bool if PIC is SLEEPING

        //Watchdog
        private static readonly int watchdogLimit = 18000; //18ms watchdog time
        private static bool watchDogEnabled;

        // Watchdog postscaler and Timer prescaler
        private static int prePostscalerRatio; // multiplier for timer
        private static int prePostscaler;

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
            trisALatch = 0x1F;
            trisBLatch = 0xFF;

            SetPrePostscalerRatio();
        }

        public static void SetRegister(byte address, byte data)
        {
            // Set the data in the correct Register

            if (address != 0x05 || address != 0x06)         // Not for PORTA and PORTB because they need to be handled seperatly
                register[Convert.ToInt16(address)] = data;



            //Handling special functions
            switch (address)
            {
                case 0x00: //indirect memory access => Using FSR
                    register[GetRegister(Registers.FSR)] = data;
                    break;
                case 0x01:
                    ResetPrePostScaler();
                    InstructionProcessor.SkipOneCycle();
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
                    CheckIfWeCanSetPortADependingOnTrisALatch(data); // Set PORTA Depending on TrisLatch
                    break;
                case 0x85:
                    trisALatch = data; // when writing to TRISA also set TRISA Latch
                    CheckIfWeNeedToUpdatePortA(); // CHeck if we need to change PORTA
                    break;


                case 0x06:
                    CheckIfWeCanSetPortBDependingOnTrisBLatch(data); // Set PORTB Depending on TrisLatch
                    break;
                case 0x86:
                    trisBLatch = data; // when writing to TRISB also set TRISB Latch
                    CheckIfWeNeedToUpdatePortB(); // CHeck if we need to change PORTB



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
        public static void SetPrePostscalerRatio()
        {
            // Get the PSA0, PS1, PS2 Bits (PS2-0)
            byte PS = (byte)(GetRegister(Registers.OPTION) & 7);

            // Check if PrePostScaler is assigned to TMR0 or WatchDogTimer
            if(GetRegisterBit(Registers.OPTION, Flags.Option.PSA) == false)
            {
                // Prescaler assigned to TMR0
                prePostscalerRatio = (int)Math.Pow(2, PS + 1); //set muliplier for timer
            }
            else
            {
                // Prescaler assigned to WatchDogTimer
                prePostscalerRatio = (int)Math.Pow(2, PS); //set muliplier for timer
            }

            // Calculation WatchDogTimer:
            // PS2:0    multiplier
            // 000      1 : 1
            // 001      1 : 2
            // 010      1 : 4
        }
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
        public static void ProcessWatchDogTimer()
        {
            if (watchDogEnabled)
            {
                // add Executiontime to watchdog
                watchdog += GetSingleExecutionTime();
                int limit = watchdogLimit;

                if (GetRegisterBit(Registers.OPTION, Flags.Option.PSA) == true) // Assigned to WatchDogTimer
                {
                    // calculate Limit with current PreScaler limit
                    limit *= GetPrePostScalerRatio();
                }
                if (watchdog >= limit) // to force an earlyer (instead of 2.3s interrupt) => (limit - 2303000)
                {
                    // execute a RESET caused by the WatchDog
                    WatchDogTimerReset();
                }

                // USE WHEN TESTING WATCHDOGTIMER RESET TO AVOID WAITING A LONG AMOUNT OF TIME!!!
                //if (watchdog >= (limit - 2303000)) // to force an earlyer (instead of 2.3s interrupt) => (limit - 2303000)
                //{
                //    // execute a RESET caused by the WatchDog
                //    WatchDogTimerReset();
                //}
            }
        }
        #endregion



        #region LATCH
        private static void CheckIfWeNeedToUpdatePortA()
        {
            // when TrisALatch Bit is 0 the result bit has to be the same as dataLatchA bit
            // when TrisALatch Bit is 1 the result bit has to be 0
            byte result = (byte)~(~dataLatchA | trisALatch);

            // safe result in PORTA Register
            register[Convert.ToInt16(0x05)] = result;
        }
        private static void CheckIfWeNeedToUpdatePortB()
        {
            // when TrisBLatch Bit is 0 the result bit has to be the same as dataLatchB bit
            // when TrisBLatch Bit is 1 the result bit has to be 0
            byte result = (byte)~(~dataLatchB | trisBLatch);

            // safe result in PORTB Register
            register[Convert.ToInt16(0x06)] = result;
        }

        private static void CheckIfWeCanSetPortADependingOnTrisALatch(byte newDataForPortA)
        {

            // befor seting PortA we need to check if TrisALatch is Input or Output

            // Set dataLatch to new value
            dataLatchA = newDataForPortA;


            // when TrisALatch Bit is 0 the result bit has to be the same as dataLatchA bit
            // when TrisALatch Bit is 1 the result bit has to be 0
            byte result = (byte)~(~dataLatchA | trisALatch);


            // safe result in PORTA Register
            register[Convert.ToInt16(0x05)] = result;
        }
        private static void CheckIfWeCanSetPortBDependingOnTrisBLatch(byte newDataForPortB)
        {

            // befor seting PortB we need to check if TrisBLatch is Input or Output

            // Set dataLatch to new value
            dataLatchB = newDataForPortB;


            // when TrisBLatch Bit is 0 the result bit has to be the same as dataLatchB bit
            // when TrisBLatch Bit is 1 the result bit has to be 0
            byte result = (byte)~(~dataLatchB | trisBLatch);


            // safe result in PORTB Register
            register[Convert.ToInt16(0x06)] = result;
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
        public static void CallInterrupt()
        {
            // Push the current PC on Stack
            PushOnStack();

            // Set PC on interrupt routine address
            SetPC(0x04);

            // disable Interrupts
            SetSingleRegisterBit(Registers.INTCON, Flags.Intcon.GIE, false);

            if (IsSleeping())
            {
                SetSingleRegisterBit(Registers.STATUS, Flags.Status.PD, false);
                SetSingleRegisterBit(Registers.STATUS, Flags.Status.TO, true);
                IncreasePC();
                SetSleeping(false);
            }
        }
        public static void ProcessRBInterrupts()
        {
            // Get Register PORTB<7:4>, to check for input change
            byte RB = (byte)(GetRegister(Registers.PORTB) & 0xF0);

            // exclusive OR last RB state with current TRISB => check for an input change
            if (((RBIntLastState ^ RB) & GetRegister(Registers.TRISB)) != 0)
            {
                // if RB has change => set RBIF flag
                SetSingleRegisterBit(Registers.INTCON, Flags.Intcon.RBIF, true);
            }
            // Set Last RB state to current state
            RBIntLastState = RB;


            //RB0 Interrupts depending of Flankchange

            // Get the RB0 Bit
            byte RB0 = (byte)(GetRegister(Registers.PORTB) & 0x01);

            // INTEDG indicates if a rising or falling flank is active
            // INTEDG = 1 => rising flank: check if RBO is bigger than RB0IntLastState
            // OR
            // INTEDG = 0 => falling flank: check if RBO is lower than RB0IntLastState
            if (GetRegisterBit(Registers.OPTION, Flags.Option.INTEDG) && RB0 > RB0IntLastState || !GetRegisterBit(Registers.OPTION, Flags.Option.INTEDG) && RB0 < RB0IntLastState)
            {
                SetSingleRegisterBit(Registers.INTCON, Flags.Intcon.INTF, true);
            }

            // Set Last RB0 state to current state
            RB0IntLastState = RB0;
        }
        public static void ProcessTMR0()
        {
            //Timer0 Overflow interrupt

            // Get the TMR0 input: RA4 bit
            byte RA4 = (byte)(GetRegister(Registers.PORTA) >> 4 & 0x01);

            // the condition for an TMR0 Interrupt are out of the datasheet
            if
            (
                GetRegisterBit(Registers.OPTION, Flags.Option.T0CS) == false || // internal clock source
                GetRegisterBit(Registers.OPTION, Flags.Option.T0SE) && RA4 < RA4TimerLastState || // external Clock source RA4 and falling flank 
                !GetRegisterBit(Registers.OPTION, Flags.Option.T0SE) && RA4 > RA4TimerLastState   // external Clock source RA4 and rising flank
            )
            {
                bool increment = true;

                if (GetRegisterBit(Registers.OPTION, Flags.Option.PSA) == false) // the prescaler is assigned to to TMR0
                {
                    prePostscaler++;
                    if (prePostscaler >= GetPrePostScalerRatio())
                    {
                        ResetPrePostScaler();
                        increment = true;
                    }
                    else
                    {
                        increment = false;
                    }
                }

                if (increment)
                {
                    byte tmr0 = (byte)(GetRegister(Registers.TRM0) + 1); // Increment the TMR0 Register
                    register[Registers.TRM0] = tmr0; // direct access to register to avoid a prescaler reset

                    if (tmr0 == 0)
                    {
                        SetSingleRegisterBit(Registers.INTCON, Flags.Intcon.T0IF, true);
                    }
                }
            }
            RA4TimerLastState = RA4;
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
            return stack[stackPointer];
        }
        #endregion

        #region Getter and Setter
        public static void SetProgramm(List<string> commandList)
        {
            //Create a list of Commands out of list of read hexadecimal commands

            if (commandList == null) throw new ArgumentNullException();


            // Füllen mit instructions !!!!!!

            // Erstellen einer Liste mit 1024 Commands mit 0 als High und Low Byte als Dummy

            program = new List<Command>(1024);

            for (int i = 0; i < 1024; i++)
                program.Add(new Command("0000"));



            //List<string> hexadecimalCommand = Enumerable.Repeat("#", 1024).ToList();

            for (int i = 0; i < commandList.Count; i++)
            {
                if(commandList[i] != "#")
                {
                    Command line = new Command(commandList[i]);
                    program.Insert(i, line);
                }
            }

            ProgrammInitialized = true;
        }
        public static List<Command> GetProgramm()
        {
            return program;
        }

        public static int[] GetStack()
        {
            return stack;
        }

        public static int GetPc()
        {
            return pc;
        }

        public static void SetClockSpeed(int speed)
        {
            clockspeed = speed;
        }

        public static void SetWatchdogEnabled(bool wd)
        {
            watchDogEnabled = wd;
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

        public static int GetPrePostScalerRatio()
        {
            return prePostscalerRatio;
        }

        public static bool IsProgrammInitialized()
        {
            return ProgrammInitialized;
        }

        #endregion
    }
}
