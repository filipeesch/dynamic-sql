namespace DynamicSQL;

using System;
using System.Buffers;

internal class PooledStringBuilder : IDisposable
{
    private char[] _buffer;
    private int _position = 0;

    public PooledStringBuilder(int predictedLength)
    {
        _buffer = ArrayPool<char>.Shared.Rent(predictedLength);
    }

    public PooledStringBuilder Append(string value) => Append(value.AsSpan());

    public PooledStringBuilder Append(ValueStringBuilder value) => Append(value.Span);

    public PooledStringBuilder Append(char value)
    {
        _buffer[_position++] = value;
        return this;
    }

    public PooledStringBuilder Append(ReadOnlySpan<char> value)
    {
        if (_buffer.Length - _position < value.Length)
        {
            var newBuffer = ArrayPool<char>.Shared.Rent((_buffer.Length + value.Length) * 2);
            _buffer.AsSpan().Slice(0, _position).CopyTo(newBuffer);

            ArrayPool<char>.Shared.Return(_buffer);
            _buffer = newBuffer;
        }

        value.CopyTo(_buffer.AsSpan().Slice(_position));
        _position += value.Length;
        return this;
    }

    public override string ToString() =>
        _buffer
            .AsSpan()
            .Slice(0, _position)
            .ToString();

    public void Dispose()
    {
        ArrayPool<char>.Shared.Return(_buffer);
    }
}
