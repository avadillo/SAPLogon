namespace SAPTools.Utils;
public static class Base64 {
    private static readonly char[] toBase64 =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"
        .ToCharArray();
    private static readonly byte[] fromBase64 = [
        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
        0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
        0xFF,0x3E,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0x3E,0xFF,0xFF,0xFF,0x3F,
        0x34,0x35,0x36,0x37,0x38,0x39,0x3A,0x3B,0x3C,0x3D,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,
        0xFF,0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0A,0x0B,0x0C,0x0D,0x0E,
        0x0F,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,0xFF,0xFF,0xFF,0xFF,0xFF,
        0xFF,0x1A,0x1B,0x1C,0x1D,0x1E,0x1F,0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,
        0x29,0x2A,0x2B,0x2C,0x2D,0x2E,0x2F,0x30,0x31,0x32,0x33,0xFF,0xFF,0xFF,0xFF,0xFF,
    ];

    private const char EqualsCharacter = '=';
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
                @out[pos++] = EqualsCharacter;
                @out[pos++] = EqualsCharacter;
                break;
            case 2:
                a = data[i];
                b = data[i + 1];
                @out[pos++] = toBase64[(a & IIIIII00) >> 2];
                @out[pos++] = toBase64[(a & OOOOOOII) << 4 | (b & IIIIOOOO) >> 4];
                @out[pos++] = toBase64[(b & OOOOIIII) << 2];
                @out[pos++] = EqualsCharacter;
                break;
        }
        return @out;
    }

    private static byte[] Decode(char[] data) {
        int end;
        for(end = data.Length;
            end > 0 && data[end - 1] == EqualsCharacter;
            --end) { }

        int resultLength = end / 4 * 3;
        int x = end % 4;
        if(x == 2) ++resultLength;
        else if(x == 3) resultLength += 2;

        byte[] result = new byte[resultLength];
        int i = 0;

        byte a, b, c, d;
        int pos;
        for(pos = 0; end >= 4; end -= 4) {
            a = fromBase64[data[i + 0]];
            b = fromBase64[data[i + 1]];
            c = fromBase64[data[i + 2]];
            d = fromBase64[data[i + 3]];
            result[pos++] = (byte)((a & IIIIIIII) << 2 | (b & OOIIOOOO) >> 4);
            result[pos++] = (byte)((b & OOOOIIII) << 4 | (c & OOIIIIOO) >> 2);
            result[pos++] = (byte)((c & OOOOOOII) << 6 | (d & IIIIIIII) >> 0);
            i += 4;
        }

        if(end >= 2) {
            a = fromBase64[data[i + 0]];
            b = fromBase64[data[i + 1]];
            c = fromBase64[data[i + 2]];
            result[pos] = (byte)((a & IIIIIIII) << 2 | (b & OOIIOOOO) >> 4);
            if(end >= 3) {
                result[pos + 1] = (byte)((b & OOOOIIII) << 4 | (c & OOIIIIOO) >> 2);
            }
        }
        return result;
    }
}