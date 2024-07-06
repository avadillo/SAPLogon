using System.Text;

namespace SAPTools.Utils;

public static class Hex {
    private static readonly char[] HEXARRAY = "0123456789ABCDEF".ToCharArray();
    public static string BytesToHex(byte[] bytes) {
        char[] hexChars = new char[bytes.Length * 2];
        for (int j = 0; j < bytes.Length; j++) {
            int v = bytes[j] & 0xFF;
            hexChars[j * 2] = HEXARRAY[v >> 4];
            hexChars[j * 2 + 1] = HEXARRAY[v & 0x0F];
        }
        return new string(hexChars);
    }

    // function to convert an hex string to byte array
    public static byte[] HexToBytes(string s) {
        int len = s.Length;
        byte[] data = new byte[len / 2];
        for(int i = 0; i < len; i += 2) {
            data[i / 2] = (byte)((GetHexVal(s[i]) << 4) | GetHexVal(s[i + 1]));
        }
        return data;
    }

    private static int GetHexVal(char hex) {
        int val = (int)hex;
        // Return directly the value for 0-9 and A-F (both uppercase and lowercase)
        return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
    }

    public static int ExtractNumber(byte[] input, int init, int length) =>
        BitConverter.ToInt32(input.Skip(init).Take(length).Reverse().ToArray());

    public static byte[] ExtractBytes(byte[] input, int init, int length) =>
        input.Skip(init).Take(length).ToArray();

}