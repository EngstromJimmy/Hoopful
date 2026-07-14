namespace Hoopful.Formats;

/// <summary>
/// Sequential reader over an in-memory file, ported from the original application's
/// <c>Binary</c> class (which loaded the whole file into a byte array too — this port
/// simply drops the file handle so it can run in the browser).
/// </summary>
internal sealed class ByteReader(byte[] data)
{
    private int _position;

    public byte[] Bytes { get; } = data;

    public int Position => _position;

    public long Length => Bytes.Length;

    public void SetPosition(int position) => _position = position;

    public byte[] ReadBytes(int position, int length)
    {
        _position = position;
        return ReadNextBytes(length);
    }

    public byte[] ReadNextBytes(int length)
    {
        byte[] result = new byte[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = Bytes[_position++];
        }

        return result;
    }

    public char[] ReadChars(int position, int length)
    {
        _position = position;
        char[] result = new char[length];
        for (int i = 0; i < length; i++)
        {
            result[i] = (char)Bytes[_position++];
        }

        return result;
    }
}
