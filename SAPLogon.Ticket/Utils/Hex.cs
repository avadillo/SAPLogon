using System.Text;

namespace SAPTools.Utils
{
    public static class Hex
    {
        private static readonly char[] HEXARRAY = "0123456789ABCDEF".ToCharArray();
        public static string BytesToHex(byte[] bytes)
        {
            char[] hexChars = new char[bytes.Length * 2];
            for (int j = 0; j < bytes.Length; j++) {
                int v = bytes[j] & 0xFF;
                hexChars[j * 2] = HEXARRAY[v >>> 4];
                hexChars[j * 2 + 1] = HEXARRAY[v & 0x0F];
            }
            return new string(hexChars);
        }

        // function to convert an hex string to byte array
        public static byte[] HexToBytes(string s) {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
                data[i / 2] = (byte)((Char2Byte(s[i]) << 4) | Char2Byte(s[i + 1]));
            return data;
        }

        private static byte Char2Byte(char c) =>
            c is >= '0' and <= '9' ? (byte)(c - '0')
            : c is >= 'A' and <= 'F' ? (byte)(c - 'A' + 10)
            : c is >= 'a' and <= 'f' ? (byte)(c - 'a' + 10)
            : throw new ArgumentException("Invalid hex char");

        public static int extractNumber(byte[] input, int init, int length) =>
            BitConverter.ToInt32(input.Skip(init).Take(length).Reverse().ToArray());

        public static byte[] extractBytes(byte[] input, int init, int length) =>
            input.Skip(init).Take(length).ToArray();

        // source
        // https://www.codeproject.com/articles/36747/quick-and-dirty-hexdump-of-a-byte-array
        public static string HexDump(byte[] bytes, int bytesPerLine = 16) {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength) {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    } else {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = b is < 32 or > 127 ? '·' : (char)b;
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                _ = result.Append(line);
            }
            return result.ToString();
        }
    }


}
