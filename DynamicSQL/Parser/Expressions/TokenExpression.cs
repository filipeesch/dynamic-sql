namespace DynamicSQL.Parser.Expressions;

using System;

internal abstract class TokenExpression(int startIndex, int endIndex) : IParsedExpression
{
    public int StartIndex { get; } = startIndex;

    public int EndIndex { get; } = endIndex;

    public virtual bool TryReduce(ExpressionStack stack) => false;

    public static TokenExpression FromSymbol(string symbol, int startIndex, int endIndex)
    {
        return symbol switch
        {
            TokenSymbols.OpenTag => new OpenTagExpression(startIndex, endIndex),
            TokenSymbols.CloseTag => new CloseTagExpression(startIndex, endIndex),
            TokenSymbols.OpenBraces => new OpenBracesExpression(startIndex, endIndex),
            TokenSymbols.CloseBraces => new CloseBracesExpression(startIndex, endIndex),
            TokenSymbols.QuestionMark => new QuestionMarkExpression(startIndex, endIndex),
            TokenSymbols.Colon => new ColonExpression(startIndex, endIndex),
            TokenSymbols.InOperator => new InOperatorExpression(startIndex, endIndex),
            _ => throw new ArgumentOutOfRangeException($"Symbol '{symbol}' is not supported")
        };
    }
}
