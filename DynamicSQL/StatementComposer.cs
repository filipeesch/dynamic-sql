namespace DynamicSQL;

using System;

public class StatementComposer<TInput>
{
    public SegmentRenderer<TInput> SegmentRenderer(Action<SegmentRendererContext<TInput>> handler) => new(handler);
}
