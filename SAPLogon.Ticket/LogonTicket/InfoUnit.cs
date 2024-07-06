using System.Text;

namespace SAPTools.LogonTicket;

public class InfoUnit(int id, byte[] data) {
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

    public int ID { get; set; } = id;
    private byte[] Content { get; set; } = data;

    public string ToString(Encoding encoding) => BytesToString(Content, encoding);
    public int ToInt => BytesToInt(Content);

    public virtual void WriteTo(Stream @out) {
        int totalLength = 3 + Content.Length; // 1 byte for ID and 2 bytes for Content.Length
        byte[] buffer = new byte[totalLength];

        buffer[0] = (byte)ID;
        buffer[1] = (byte)(Content.Length >> 8 & 255);
        buffer[2] = (byte)(Content.Length & 255);
        Array.Copy(Content, 0, buffer, 3, Content.Length);

        @out.Write(buffer, 0, buffer.Length);
    }

    public static int BytesToInt(byte[] buffer) => BytesToInt(buffer, 0, buffer.Length);

    public static int BytesToInt(byte[] buffer, int offset, int length) {
        int result = 0;
        for(int i = 0; i < length; i++) {
            result = (result << 8) + (buffer[offset + i] & 0xFF);
        }
        return result;
    }

    public static byte[] Int32ToBytes(int n) {
        byte[] bytes = BitConverter.GetBytes(n);
        if(BitConverter.IsLittleEndian)  Array.Reverse(bytes);
        return bytes;
    }

    public static string BytesToString(byte[] buffer, Encoding encoding) =>
        BytesToString(buffer, 0, buffer.Length, encoding);

    public static string BytesToString(byte[] buffer, int offset, int length, Encoding encoding) => 
        encoding.GetString(buffer, offset, length);
}
