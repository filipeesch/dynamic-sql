namespace DynamicSQL;

using System;

public readonly struct SegmentRenderer<TInput>(Action<SegmentRendererContext<TInput>> handler)
{
    public Action<SegmentRendererContext<TInput>> Render => handler;
}
