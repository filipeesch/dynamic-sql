namespace DynamicSQL;

public readonly struct SegmentRendererContext<TInput>(
    CommandTextBuilder builder,
    StatementParameters parameters,
    TInput input,
    int interpolationIndex)
{
    public CommandTextBuilder CommandTextBuilder { get; } = builder;

    public StatementParameters Parameters { get; } = parameters;

    public TInput Input { get; } = input;

    public int InterpolationIndex { get; } = interpolationIndex;
}
