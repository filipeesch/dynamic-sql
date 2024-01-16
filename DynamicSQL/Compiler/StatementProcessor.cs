namespace DynamicSQL.Compiler;

using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Text;
using DynamicSQL.Parser.Expressions;

internal class StatementProcessor(StringBuilder builder, DbCommand command, object[] values) : IStatementProcessor
{
    public void RenderTextExpression(TextExpression expression) => builder.Append($" {expression.Text} ");

    public void RenderParameterExpression(ParameterExpression expression)
    {
        var parameterName = $"p{expression.ParameterIndex}";
        var value = values[expression.ParameterIndex];

        builder.Append($"@{parameterName}");

        var parameter = command.CreateParameter();

        parameter.ParameterName = parameterName;
        parameter.Value = value;

        command.Parameters.Add(parameter);
    }

    public void RenderInArrayExpression(InArrayExpression expression)
    {
        var value = values[expression.ParameterIndex];

        if (value is not IEnumerable enumerable)
        {
            throw new ArgumentException(
                $"The IN operator must be used only in collection types. Index: {expression.ParameterIndex} Value: {value}");
        }

        builder.Append("IN(");

        using var enumerator = enumerable
            .Cast<object>()
            .GetEnumerator();

        var index = 0;

        var baseParameterName = $"p{expression.ParameterIndex}";

        while (enumerator.MoveNext())
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            var parameterName = $"{baseParameterName}_{index++}";

            builder.Append($"@{parameterName}");

            var parameter = command.CreateParameter();

            parameter.ParameterName = parameterName;
            parameter.Value = enumerator.Current;

            command.Parameters.Add(parameter);
        }

        builder.Append(")");
    }

    public bool ConditionValueTest(int conditionValueIndex)
    {
        return values[conditionValueIndex] switch
        {
            null => false,
            bool boolValue => boolValue,
            ICollection collection => collection.Count > 0,
            IEnumerable enumerable => enumerable.Cast<object>().Any(),
            _ => true
        };
    }
}
