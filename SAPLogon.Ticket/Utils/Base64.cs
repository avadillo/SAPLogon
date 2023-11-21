using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SAPTools.Utils;
public static class Base64 {
    private static readonly char[] toBase64 =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        .ToCharArray();
    private static readonly byte[] fromBase64 = Hex.HexToBytes(
        "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF" +
        "FF3EFFFFFFFFFFFFFFFFFF3EFFFFFF3F3435363738393A3B3C3DFFFFFFFFFFFF" +
        "FF000102030405060708090A0B0C0D0E0F10111213141516171819FFFFFFFFFF" +
        "FF1A1B1C1D1E1F202122232425262728292A2B2C2D2E2F30313233FFFFFFFFFF");

    private const char EQUALS_CHARACTER = '=';
    private const int IIIIIIII = 255;
    private const int IIIIII00 = 252;
    private const int IIIIOOOO = 240;
    private const int IIOOOOOO = 192;
    private const int OOIIIIII = 63;
    private const int OOIIIIOO = 60;
    private const int OOIIOOOO = 48;
    private const int OOOOIIII = 15;
    private const int OOOOOOII = 3;

    public static string Encode(byte[] data) =>
        new(EncodeAsArray(data));

    public static byte[] Decode(string data) {
        char[] tdata = new char[data.Length];
        data.CopyTo(0, tdata, 0, data.Length - 0);
        return Decode(tdata);
    }

    public static char[] EncodeAsArray(byte[] data) {
        int end = data.Length;
        int resultLength = (end + 2) / 3 * 4;
        char[] @out = new char[resultLength];
        int limit = end - 2;
        int pos = 0;

        int i;
        byte a, b;
        for(i = 0; i < limit; i += 3) {
            a = data[i];
            b = data[i + 1];
            sbyte c = (sbyte)data[i + 2];
            @out[pos++] = toBase64[(a & IIIIII00) >> 2];
            @out[pos++] = toBase64[(a & OOOOOOII) << 4 | (b & IIIIOOOO) >> 4];
            @out[pos++] = toBase64[(b & OOOOIIII) << 2 | (c & IIOOOOOO) >> 6];
            @out[pos++] = toBase64[c & OOIIIIII];
        }

        switch(end - i) {
            case 1:
                a = data[i];
                @out[pos++] = toBase64[(a & IIIIII00) >> 2];
                @out[pos++] = toBase64[(a & OOOOOOII) << 4];
                @out[pos++] = EQUALS_CHARACTER;
                @out[pos++] = EQUALS_CHARACTER;
                break;
            case 2:
                a = data[i];
                b = data[i + 1];
                @out[pos++] = toBase64[(a & IIIIII00) >> 2];
                @out[pos++] = toBase64[(a & OOOOOOII) << 4 | (b & IIIIOOOO) >> 4];
                @out[pos++] = toBase64[(b & OOOOIIII) << 2];
                @out[pos++] = EQUALS_CHARACTER;
                break;
        }
        return @out;
    }

    private static byte[] Decode(char[] data) {
        int end;
        for (end = data.Length;
            end > 0 && data[end - 1] == EQUALS_CHARACTER;
            --end) {}

        int resultLength = end / 4 * 3;
        int x = end % 4;
        if (x == 2) ++resultLength;
        else if (x == 3) resultLength += 2;

        byte[] result = new byte[resultLength];
        int i = 0;

        byte a, b, c, d;
        int pos;
        for (pos = 0; end >= 4; end -= 4) {
            a = fromBase64[data[i + 0]];
            b = fromBase64[data[i + 1]];
            c = fromBase64[data[i + 2]];
            d = fromBase64[data[i + 3]];
            result[pos++] = (byte)((a & IIIIIIII) << 2 | (b & OOIIOOOO) >> 4);
            result[pos++] = (byte)((b & OOOOIIII) << 4 | (c & OOIIIIOO) >> 2);
            result[pos++] = (byte)((c & OOOOOOII) << 6 | (d & IIIIIIII) >> 0);
            i += 4;
        }

        if (end >= 2) {
            a = fromBase64[data[i + 0]];
            b = fromBase64[data[i + 1]];
            c = fromBase64[data[i + 2]];
            result[pos] = (byte)((a & IIIIIIII) << 2 | (b & OOIIOOOO) >> 4);
            if (end >= 3) {
                result[pos + 1] = (byte)((b & OOOOIIII) << 4 | (c & OOIIIIOO) >> 2);
            }
        }
        return result;
    }
}