namespace DynamicSQL;

using System;

internal static class PrimitiveParser
{
    public static int Parse(int value, Span<char> buffer)
    {
        var index = 0;

        do
        {
            buffer[index++] = (char)('0' + value % 10);
            value /= 10;
        } while (value > 0);

        buffer.Slice(0, index).Reverse();

        return index;
    }
}
