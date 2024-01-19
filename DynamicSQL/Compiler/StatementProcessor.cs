namespace DynamicSQL.Compiler;

using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using DynamicSQL.Parser.Expressions;

internal class StatementProcessor(PooledStringBuilder builder, DbCommand command, object[] values) : IStatementProcessor
{
    public void RenderTextExpression(TextExpression expression) =>
        builder
            .Append(' ')
            .Append(expression.Text)
            .Append(' ');

    public void RenderParameterExpression(ParameterExpression expression)
    {
        var parameterName = new ValueStringBuilder(stackalloc char[16]);
        parameterName.Append('p');
        parameterName.Append(expression.ParameterIndex);

        var value = values[expression.ParameterIndex];

        builder
            .Append('@')
            .Append(parameterName);

        var parameter = command.CreateParameter();

        parameter.ParameterName = parameterName.ToString();
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

        var index = 0;

        var baseParameterName = new ValueStringBuilder(stackalloc char[16]);
        baseParameterName.Append('p');
        baseParameterName.Append(expression.ParameterIndex);

        var enumerator = enumerable.GetEnumerator();

        var parameterName = new ValueStringBuilder(stackalloc char[16]);

        try
        {
            while (enumerator.MoveNext())
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                parameterName.Clear();
                parameterName.Append(baseParameterName);
                parameterName.Append('_');
                parameterName.Append(index++);

                builder
                    .Append('@')
                    .Append(parameterName);

                var parameter = command.CreateParameter();

                parameter.ParameterName = parameterName.ToString();
                parameter.Value = enumerator.Current;

                command.Parameters.Add(parameter);
            }
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        builder.Append(')');
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
