namespace DynamicSQL.Parser.Expressions;

public class TextExpression(string text) : IParsedExpression
{
    public string Text { get; } = text;

    public bool TryReduce(ExpressionStack stack) => false;
}
