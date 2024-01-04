namespace DynamicSQL.Parser;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class CodeNode : IParsedSqlNode
{
    private CodeNode(string code, IReadOnlyList<Parameter> parameters)
    {
        Code = code;
        Parameters = parameters;
    }

    public static CodeNode Create(string code)
    {
        return new(
            code,
            RegexDefinitions.Parameters
                .Matches(code)
                .Cast<Match>()
                .Select(match => new Parameter(match.Groups[1].Value, int.Parse(match.Groups[2].Value)))
                .ToList()
                .AsReadOnly());
    }

    public IReadOnlyList<Parameter> Parameters { get; }

    public string Code { get; }

    public class Parameter(string op, int parameterValueIndex)
    {
        public string Operator { get; } = op;

        public int ParameterValueIndex { get; } = parameterValueIndex;
    }
}
