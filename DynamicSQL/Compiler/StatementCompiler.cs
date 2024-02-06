namespace DynamicSQL.Compiler;

using System;
using System.Linq.Expressions;
using DynamicSQL.Parser;

public static class StatementCompiler
{
    [Obsolete("This method was discontinued, please use it's overload", true)]
    public static Statement<TInput> Compile<TInput>(Expression<Func<TInput, FormattableString>> exp) => throw new NotImplementedException();

    public static Statement<TInput> Compile<TInput>(Expression<Func<TInput, StatementComposer<TInput>, FormattableString>> exp)
    {
        if (
            exp.Body is not MethodCallExpression methodExp ||
            methodExp.Arguments[0] is not ConstantExpression { Value: string format })
        {
            throw new ArgumentException("The Compile method should return a single expression with an interpolated string");
        }

        var parser = new StatementParser();

        var parsed = parser.Parse(format);
        var getValuesMethod = exp.Compile();

        var compiler = new CodeCompiler(parsed);

        var renderMethod = compiler.Compile();

        var statementBuilder = new StatementComposer<TInput>();

        return new Statement<TInput>(
            renderMethod,
            input => getValuesMethod(input, statementBuilder).GetArguments(),
            format.Length);
    }
}
