namespace PIC16F8X.helpfunctions
{
    public static class HelpFunctions
    {

        public static int ConvertHexToInt(string hex)
        {
            if (hex.Length > 2) return -1;

            return ( 16 * HexLookup(hex[0]) ) + HexLookup(hex[1]);
        }

        public static int HexLookup(char c)
        {
            return c switch
            {
                '0' => 0,
                '1' => 1,
                '2' => 2,
                '3' => 3,
                '4' => 4,
                '5' => 5,
                '6' => 6,
                '7' => 7,
                '8' => 8,
                '9' => 9,
                'a' => 10,
                'b' => 11,
                'c' => 12,
                'd' => 13,
                'e' => 14,
                'f' => 15,
                'A' => 10,
                'B' => 11,
                'C' => 12,
                'D' => 13,
                'E' => 14,
                'F' => 15,
                _ => -1
            };
        }
    }
}
