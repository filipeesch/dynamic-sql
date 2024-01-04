namespace DynamicSQL.Compiler;

using System;
using System.Linq.Expressions;
using DynamicSQL.Parser;

public class SqlCompiler
{
    public static CompiledSql<TInput> Compile<TInput>(Expression<Func<TInput, FormattableString>> exp)
    {
        if (
            exp.Body is not MethodCallExpression methodExp ||
            methodExp.Arguments[0] is not ConstantExpression { Value: string sqlTemplate })
        {
            throw new ArgumentException("The Compile method should return a single expression with an interpolated string");
        }

        var parser = new SqlParser();

        var parsedSql = parser.Parse(sqlTemplate);
        var compiledTemplateExp = exp.Compile();

        var compiler = new CodeCompiler<TInput>(parsedSql);

        return compiler.Compile();
    }
}
