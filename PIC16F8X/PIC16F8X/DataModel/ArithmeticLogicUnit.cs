namespace PIC16F8X.DataModel
{
    public static class ArithmeticLogicUnit
    {
        public static byte BitwiseAdd(byte b1, byte b2)
        {
            // calculate bitwiseAdd operation
            byte result = (byte)(b1 + b2);


            // Set CarryFlag
            // if result is lower than either b1 or b2, the operation caused a overflow
            if (result < b1 || result < b2)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, true); // overflow => set CarryFlag
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, false); // no overflow => reset CarryFlag
            }

            // Set DigitCarryFlag if 4th low order bit overflows
            // DigitCarry means, that an overflow of the 4th low order bit happens => we can check that by testing the 5th bit
            // if the 5th bit is 1 => we have to set DC to true
            if ( (( (b1 & 15) + (b2 & 15) ) & 16) == 16 ) // W AND 0000 1111 to b1 and b2 and AND the result with 0001 0000 and if res = 16 overflow happend
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.DC, true);
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.DC, false);
            }

            // Set ZeroFlag if result = 0
            Flags.CheckZFlag(result);

            return result;
        }
        public static byte BitwiseSubstract(byte b1, byte b2)
        {
            // build the two´s complement
            b2 = (byte)(~b2 + 1);

            // calculate result
            byte result = (byte)(b1 + b2);

            // set carry Flag if byte overflows or either b1 or b2 are zero
            if (result < b1 || result < b2 || b1 == 0 || b2 == 0)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, true);
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.C, false);
            }

            // set DigitCarry flag if either b1 or b2 are zero or the 4th low order bit overflows
            if (((((b1 & 15) + (b2 & 15)) & 16) == 16) || b1 == 0 || b2 == 0)
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.DC, true);
            }
            else
            {
                Fields.SetSingleRegisterBit(Registers.STATUS, Flags.Status.DC, false);
            }

            //Set Zeroflag if result is zero
            Flags.CheckZFlag(result);

            return result;
        }
    }
}