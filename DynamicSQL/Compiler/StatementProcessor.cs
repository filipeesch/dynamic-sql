namespace DynamicSQL.Compiler;

using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Text;
using DynamicSQL.Parser;

public class StatementProcessor(StringBuilder builder, DbCommand command, object[] values) : IStatementProcessor
{
    public void RenderCodeNode(SqlExpressionNode node) => builder.Append(node.Expression);

    public void RenderParameterNode(ParameterNode node)
    {
        var parameterName = $"p{node.ParameterValueIndex}";
        var value = values[node.ParameterValueIndex];

        if (node.Operator.Equals("IN", StringComparison.InvariantCultureIgnoreCase))
        {
            if (value is not IEnumerable enumerable)
            {
                throw new ArgumentException(
                    $"The IN operator must be used only in collection types. Index: {node.ParameterValueIndex} Value: {value}");
            }

            builder.Append($" {node.Operator} (");

            using var enumerator = enumerable
                .Cast<object>()
                .GetEnumerator();

            var index = 0;

            while (enumerator.MoveNext())
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                var subParameterName = $"{parameterName}_{index++}";

                builder.Append($"@{subParameterName}");

                var parameter = command.CreateParameter();

                parameter.ParameterName = subParameterName;
                parameter.Value = enumerator.Current;

                command.Parameters.Add(parameter);
            }

            builder.Append(") ");
        }
        else
        {
            builder.Append($" {node.Operator} @{parameterName} ");

            var parameter = command.CreateParameter();

            parameter.ParameterName = parameterName;
            parameter.Value = value;

            command.Parameters.Add(parameter);
        }
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
