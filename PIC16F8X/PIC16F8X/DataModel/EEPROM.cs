using System;
using System.IO;

namespace PIC16F8X.DataModel
{
    public static class EEPROM
    {
        private static byte[] EEPROMData = new byte[64];
        private static long EEPROMWriteTimer;
        private static bool EEPROMTimerEnabled;


        #region EEPROM-File actions
        public static void WriteDataToEEPROM(byte[] data)
        {
            // writes an byteArray to an file
            File.WriteAllBytes(@".\EEPROM_DATA.txt", data);
        }

        public static void LoadDataFromEEPROM()
        {
            // reads the data of the file and safes it to the EEPROMData byteArray
            EEPROMData = File.ReadAllBytes(@".\EEPROM_DATA.txt");
        }
        #endregion


        public static void WriteEEDATAtoEEPROM()
        {
            // Gets the Address, where in EEPROM the data should be stored
            byte EEADR = Fields.GetRegister(Registers.EEADR);

            // Gets the data, that should be written in EEPROM
            byte EEDATA = Fields.GetRegister(Registers.EEDATA);

            // Set the data of EEDATA to EEPROM
            EEPROMData[Convert.ToInt16(EEADR)] = EEDATA;

            // Write Data to actual EEPROM
            WriteDataToEEPROM(EEPROMData);
        }


        public static void IncrementEEPROMTimerAndCheckForReachingLimit(long value)
        {
            InkrementEEPROMTimer(value);

            if (EEPROMWriteTimer >= 1000) // Writing in the EEPROM takes 1ms (1000 mikroS)
            {
                SetWriteProcessCompletedFlag();
                SetEEPROMTimerEnabled(false);
                ResetEEPROMTimer(); // set Timer to 0
            }
        }


        public static void SetWriteProcessCompletedFlag()
        {
            // sets the EEIF-Bit in EECON1 Register, that indicates that the Write process is completed
            Fields.SetSingleRegisterBit(Registers.EECON1, Flags.EECON1.EEIF, true);
        }



        public static void InkrementEEPROMTimer(long value)
        {
            EEPROMWriteTimer += value;
        }

        public static void ResetEEPROMTimer()
        {
            EEPROMWriteTimer = 0;
        }
        public static void SetEEPROMTimerEnabled(bool status)
        {
            EEPROMTimerEnabled = status;
        }
        public static bool CheckIfEEPROMTimerIsEnabled()
        {
            return EEPROMTimerEnabled;
        }

        public static byte GetEEPROMData()
        {
            //return the data in EEPROM on the current EEADR index
            return EEPROMData[Convert.ToInt32(Fields.GetRegister(Registers.EEADR))];
        }
    }
}
