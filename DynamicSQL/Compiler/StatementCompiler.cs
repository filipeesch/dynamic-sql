namespace DynamicSQL.Compiler;

using System;
using System.Linq.Expressions;
using DynamicSQL.Parser;

public class StatementCompiler
{
    public static CompiledStatement<TInput> Compile<TInput>(Expression<Func<TInput, FormattableString>> exp)
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

        return new CompiledStatement<TInput>(
            renderMethod,
            input => getValuesMethod(input).GetArguments(),
            format.Length);
    }
}
