namespace DynamicSQL;

using System;

internal ref struct ValueStringBuilder
{
    private readonly Span<char> _buffer;
    private int _position = 0;

    public ValueStringBuilder(Span<char> buffer) => _buffer = buffer;

    public Span<char> Span => _buffer.Slice(0, _position);

    public override string ToString() => Span.ToString();

    public void Append(string value) => Append(value.AsSpan());

    public void Append(ValueStringBuilder value) => Append(value.Span);

    public void Clear() => _position = 0;

    public void Append(char value)
    {
        _buffer[_position++] = value;
    }

    public void Append(ReadOnlySpan<char> value)
    {
        value.CopyTo(_buffer.Slice(_position));
        _position += value.Length;
    }

    public void Append(int value)
    {
        Span<char> buffer = stackalloc char[16];
        var index = 0;

        do
        {
            buffer[index++] = (char)('0' + value % 10);
            value /= 10;
        } while (value > 0);

        buffer.Slice(0, index).Reverse();

        buffer.Slice(0, index).CopyTo(_buffer.Slice(_position));
        _position += index;
    }
}
