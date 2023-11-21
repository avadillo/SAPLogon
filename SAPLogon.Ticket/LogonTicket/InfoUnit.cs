using System.Text;

namespace SAPTools.LogonTicket;

public class InfoUnit {
    public const int ID_USER = 0x1;
    public const int ID_CREATE_CLIENT = 0x2;
    public const int ID_CREATE_NAME = 0x3;
    public const int ID_CREATE_TIME = 0x4;
    public const int ID_VALID_TIME = 0x5;
    public const int ID_RFC = 0x6;
    public const int ID_VALID_TIME_MIN = 0x7;
    public const int ID_FLAGS = 0x8;
    public const int ID_SIGNATURE = 0xFF;
    public const int ID_LANGUAGE = 0x9;
    public const int ID_USER_UTF = 0xA;
    public const int ID_CREATE_CLIENT_UTF = 0xB;
    public const int ID_CREATE_NAME_UTF = 0xC;
    public const int ID_CREATE_TIME_UTF = 0xD;
    public const int ID_LANGUAGE_UTF = 0xE;
    public const int ID_AUTHSCHEME = 0x88;
    public const int ID_RECIPIENT_CLIENT = 0xF;
    public const int ID_RECIPIENT_SID = 0x10;
    public const int ID_PORTAL_USER = 0x20;

    public int ID { get; set; }
    private byte[]  Content { get; set; }

    public InfoUnit(int id, byte[] data) {
        ID = id;
        Content = data;
    }

    public string ToString(Encoding encoding) => 
        BytesToString(Content, encoding);

    public int ToInt => BytesToInt(Content);

    public virtual void WriteTo(Stream @out) {
        @out.WriteByte((byte)ID);
        @out.WriteByte((byte)(Content.Length >> 8 & 255));
        @out.WriteByte((byte)(Content.Length & 255));
        @out.Write(Content, 0, Content.Length);
    }

    public static int BytesToInt(byte[] buffer) =>
        BytesToInt(buffer, 0, buffer.Length);

    public static int BytesToInt(byte[] buffer, int offset, int length) {
        int x = 0;
        for (int i = 0; i < length; ++offset) {
            int z = buffer[offset];
            if (z < 0) z += 256;
            x = (x << 8) + z;
            ++i;
        }
        return x;
    }

    public static byte[] Int32ToBytes(int i) {
        byte[] b = new byte[4];
        b[3] = (byte)(i % 256);
        i /= 256;
        b[2] = (byte)(i % 256);
        i /= 256;
        b[1] = (byte)(i % 256);
        i /= 256;
        b[0] = (byte)(i % 256);
        return b;
    }

    public static string BytesToString(byte[] buffer, Encoding encoding) =>
        BytesToString(buffer, 0, buffer.Length, encoding);

    public static string BytesToString(byte[] buffer, int offset, int length, Encoding encoding) {
        MemoryStream ba = new(buffer, offset, length);
        StreamReader @in;
        @in = new StreamReader(ba, encoding);

        StringBuilder s = new();

        try {
            for (int c = @in.Read(); c > 0; c = @in.Read()) 
                _ = s.Append((char)c);
           
        } catch (IOException e) {
            Console.WriteLine(e.ToString());
            Console.Write(e.StackTrace);
        }
        return s.ToString();
    }
}
