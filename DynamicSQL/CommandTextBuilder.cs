namespace DynamicSQL;

using System;
using System.Buffers;

public class CommandTextBuilder : IDisposable
{
    private char[] _buffer;
    private int _position = 0;

    public CommandTextBuilder(int predictedLength)
    {
        _buffer = ArrayPool<char>.Shared.Rent(predictedLength);
    }

    public CommandTextBuilder Append(string value) => Append(value.AsSpan());

    public CommandTextBuilder Append(ValueStringBuilder value) => Append(value.Span);

    public CommandTextBuilder Append(char value)
    {
        EnsureCapacity(1);
        _buffer[_position++] = value;
        return this;
    }

    public CommandTextBuilder Append(ReadOnlySpan<char> value)
    {
        EnsureCapacity(value.Length);

        value.CopyTo(_buffer.AsSpan().Slice(_position));
        _position += value.Length;
        return this;
    }

    public override string ToString() =>
        _buffer
            .AsSpan()
            .Slice(0, _position)
            .ToString();

    public CommandTextBuilder Append(int value)
    {
        Span<char> buffer = stackalloc char[16];

        var used = PrimitiveParser.Parse(value, buffer);

        EnsureCapacity(used);

        buffer.Slice(0, used).CopyTo(_buffer.AsSpan().Slice(_position));

        _position += used;
        return this;
    }

    private void EnsureCapacity(int appendLength)
    {
        if (_buffer.Length - _position >= appendLength)
        {
            return;
        }

        var newBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length + appendLength) * 2);
        _buffer.AsSpan().Slice(0, _position).CopyTo(newBuffer);

        ArrayPool<char>.Shared.Return(_buffer);
        _buffer = newBuffer;
    }

    public void Dispose()
    {
        ArrayPool<char>.Shared.Return(_buffer);
    }
}
