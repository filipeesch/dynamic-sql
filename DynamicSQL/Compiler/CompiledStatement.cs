namespace DynamicSQL.Compiler;

using System;
using System.Data.Common;
using System.Text;

public class CompiledStatement<TInput>(
    Action<IStatementProcessor> renderMethod,
    Func<TInput, object[]> getValuesMethod,
    int predictedStatementLength)
{
    public void Render(TInput input, DbCommand command)
    {
        var builder = new StringBuilder(predictedStatementLength);
        var processor = new StatementProcessor(builder, command, getValuesMethod(input));

        renderMethod(processor);

        command.CommandText = builder.ToString();
    }
}
