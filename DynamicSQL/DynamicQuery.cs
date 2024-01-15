using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;

namespace DynamicSQL;

[Obsolete("Use StatementCompiler class instead")]
public class DynamicQuery
{
    private static readonly Regex QueryPartRegex = new(
        @"<<\s*{(\d+)}\s*\?\s*(.*?)\s*(?::\s*(.*?))?\>>",
        RegexOptions.Singleline);

    private static readonly Regex ParameterRegex = new(
        @"(?:\s*(=|<>|>|<|>=|<=|\w+)\s+)?{(\d+)}",
        RegexOptions.Singleline);

    private readonly FormattableString queryTemplate;

    public DynamicQuery(FormattableString queryTemplate)
    {
        this.queryTemplate = queryTemplate;
    }

    public void RenderOn(DbCommand command)
    {
        var query = this.ReplaceQueryParts();

        query = this.SetupParameters(query, command);

        command.CommandText = query;
    }

    private string ReplaceQueryParts()
    {
        return QueryPartRegex.Replace(
            this.queryTemplate.Format,
            match =>
            {
                if (!int.TryParse(match.Groups[1].Value, out var conditionIndex))
                {
                    throw new DynamicQueryException("Malformed dynamic query", match.Value);
                }

                var argument = this.queryTemplate.GetArgument(conditionIndex);

                var condition = argument switch
                {
                    null => false,
                    bool boolValue => boolValue,
                    IEnumerable enumerable => enumerable.Cast<object>().Any(),
                    _ => true
                };

                var trueQueryPart = match.Groups[2].Value;
                var falseQueryPart = match.Groups[3].Value;

                return condition ? trueQueryPart : falseQueryPart;
            });
    }

    private string SetupParameters(string queryPart, DbCommand command)
    {
        return ParameterRegex.Replace(
            queryPart,
            match =>
            {
                if (!int.TryParse(match.Groups[2].Value, out var parameterIndex))
                {
                    return match.Value;
                }

                var parameterValue = this.queryTemplate.GetArgument(parameterIndex);

                var op = match.Groups[1].Value;

                if (op == "IN")
                {
                    return SetupInOperatorParameter(command, parameterValue, parameterIndex, match.Value);
                }

                return SetupSingleValueOperatorParameter(command, op, parameterIndex, parameterValue);
            });
    }

    private static string SetupInOperatorParameter(
        DbCommand command,
        object? parameterValue,
        int parameterIndex,
        string queryPart)
    {
        if (parameterValue is not IEnumerable values)
        {
            throw new DynamicQueryException("IN operator must be applied to a collection", queryPart);
        }

        var parameters = values
            .Cast<object>()
            .Select(
                (value, index) => new
                {
                    Name = $"@p{parameterIndex}_{index}",
                    Value = value,
                })
            .ToList();

        foreach (var parameter in parameters)
        {
            var commandParameter = command.CreateParameter();

            commandParameter.ParameterName = parameter.Name;
            commandParameter.Value = parameter.Value;

            command.Parameters.Add(commandParameter);
        }

        return $" IN ({string.Join(",", parameters.Select(x => x.Name))}) ";
    }

    private static string SetupSingleValueOperatorParameter(
        DbCommand command,
        string op,
        int parameterIndex,
        object? parameterValue)
    {
        var parameterName = $"@p{parameterIndex}";

        var parameter = command.CreateParameter();

        parameter.ParameterName = parameterName;
        parameter.Value = parameterValue;

        command.Parameters.Add(parameter);

        return $" {op} {parameterName} ";
    }
}
