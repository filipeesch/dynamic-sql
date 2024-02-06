namespace DynamicSQL;

using System.Collections.Generic;
using System.Data.Common;

public class StatementParameters(DbCommand command)
{
    public DbParameter this[int index] => command.Parameters[index];

    public DbParameter this[string name] => command.Parameters[name];

    public DbParameter Create() => command.CreateParameter();

    public int Add(DbParameter parameter) => command.Parameters.Add(parameter);

    public void AddRange(IEnumerable<DbParameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            Add(parameter);
        }
    }

    public DbParameter Add(string name, object value)
    {
        var parameter = Create();

        parameter.ParameterName = name;
        parameter.Value = value;

        Add(parameter);

        return parameter;
    }
}
