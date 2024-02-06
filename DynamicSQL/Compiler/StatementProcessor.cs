namespace DynamicSQL.Compiler;

using System;
using System.Collections;
using System.Linq;
using DynamicSQL.Parser.Expressions;

internal class StatementProcessor<TInput>(
    CommandTextBuilder builder,
    StatementParameters parameters,
    TInput input,
    object[] values)
    : IStatementProcessor
{
    public void RenderText(string text) =>
        builder
            .Append(' ')
            .Append(text)
            .Append(' ');

    public void RenderInterpolationExpression(InterpolationExpression expression)
    {
        var value = values[expression.Index];

        switch (value)
        {
            case SegmentRenderer<TInput> renderer:
                renderer.Render(new(builder, parameters, input, expression.Index));
                break;

            default:
                CreateParameter(expression, value);
                break;
        }
    }

    public void RenderInArrayExpression(InArrayExpression expression)
    {
        var value = values[expression.InterpolationIndex];

        if (value is not IEnumerable enumerable)
        {
            throw new ArgumentException(
                $"The IN operator must be used only in collection types. Index: {expression.InterpolationIndex} Value: {value}");
        }

        builder.Append("IN(");

        var index = 0;

        var baseParameterName = new ValueStringBuilder(stackalloc char[16]);
        baseParameterName.Append('p');
        baseParameterName.Append(expression.InterpolationIndex);

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

                parameters.Add(parameterName.ToString(), enumerator.Current!);
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

    private void CreateParameter(InterpolationExpression expression, object value)
    {
        var parameterName = new ValueStringBuilder(stackalloc char[16]);
        parameterName.Append('p');
        parameterName.Append(expression.Index);

        builder
            .Append('@')
            .Append(parameterName);

        parameters.Add(parameterName.ToString(), value);
    }
}
