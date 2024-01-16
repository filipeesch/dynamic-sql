namespace DynamicSQL.Parser.Expressions;

internal class EndExpression : TokenExpression
{
    public static readonly EndExpression Instance = new();

    private EndExpression() : base(-1, -1)
    {
    }
}
