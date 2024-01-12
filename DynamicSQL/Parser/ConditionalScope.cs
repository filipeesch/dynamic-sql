namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class ConditionalScope(
    int conditionValueIndex,
    string trueTemplate,
    string falseTemplate,
    IReadOnlyList<IScope> children) : IScope
{
    public int ConditionValueIndex { get; } = conditionValueIndex;

    public string TrueTemplate { get; } = trueTemplate;

    public string FalseTemplate { get; } = falseTemplate;

    public IReadOnlyList<IScope> Children { get; } = children;
}