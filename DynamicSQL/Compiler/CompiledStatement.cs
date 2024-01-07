namespace DynamicSQL.Compiler;

using System;
using System.Data.Common;
using System.Text;

public class CompiledStatement<TInput>
{
    private readonly Action<IStatementProcessor> _renderMethod;
    private readonly Func<TInput, object[]> _getValuesMethod;
    private readonly int _statementLengthPrediction;

    public CompiledStatement(
        Action<IStatementProcessor> renderMethod,
        Func<TInput, object[]> getValuesMethod,
        int statementLengthPrediction)
    {
        _renderMethod = renderMethod;
        _getValuesMethod = getValuesMethod;
        _statementLengthPrediction = statementLengthPrediction;
    }

    public void Render(TInput input, DbCommand command)
    {
        var builder = new StringBuilder(_statementLengthPrediction);
        var processor = new StatementProcessor(builder, command, _getValuesMethod(input));

        _renderMethod(processor);

        command.CommandText = builder.ToString();
    }
}
