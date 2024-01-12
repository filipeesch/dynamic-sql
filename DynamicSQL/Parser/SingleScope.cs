namespace DynamicSQL.Parser;

using System.Collections.Generic;

public class SingleScope(string template, IReadOnlyList<IScope> children) : IScope
{
    public string Template { get; } = template;

    public IReadOnlyList<IScope> Children { get; } = children;
}