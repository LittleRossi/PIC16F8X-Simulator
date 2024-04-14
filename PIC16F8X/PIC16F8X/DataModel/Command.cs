using PIC16F8X.helpfunctions;

namespace PIC16F8X.DataModel
{
    public class Command
    {
        private readonly byte high; //high-byte of Command
        private readonly byte low;  //low-byte of Command

        public Command(string hex)
        {
            high = (byte)HelpFunctions.ConvertHexToInt(hex.Substring(0, 2)); //First two Digits of 4 Char Hex (xx00)
            low = (byte)HelpFunctions.ConvertHexToInt(hex.Substring(2, 2)); //Last two Digits of 4 Char Hex (00xx)
        }

        public Command(byte high, byte low)
        {
            this.high = high;
            this.low = low;
        }

        public byte GetHighByte()
        {
            return high;
        }

        public byte GetLowByte()
        {
            return low;
        }
    }
}
