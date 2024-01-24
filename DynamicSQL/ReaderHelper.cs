namespace DynamicSQL;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

internal static class ReaderHelper
{
    private static readonly IReadOnlyDictionary<Type, MethodInfo> GetValueMethodMap = new Dictionary<Type, MethodInfo>
    {
        { typeof(byte), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetByte)) },
        { typeof(bool), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetBoolean)) },
        { typeof(char), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetChar)) },
        { typeof(short), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt16)) },
        { typeof(int), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt32)) },
        { typeof(long), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetInt64)) },
        { typeof(float), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetFloat)) },
        { typeof(double), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDouble)) },
        { typeof(decimal), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDecimal)) },
        { typeof(Guid), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetGuid)) },
        { typeof(DateTime), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetDateTime)) },
        { typeof(string), typeof(DbDataReader).GetMethod(nameof(DbDataReader.GetString)) },
    };

    private static readonly MethodInfo IsDbNullMethod = typeof(DbDataReader).GetMethod(nameof(DbDataReader.IsDBNull))!;

    public static Expression CreateDataReaderGetValueExpression(Expression dataReaderExp, Type fieldType, int? columnOrdinal)
    {
        var defaultValueExp = Expression.Default(fieldType);

        if (columnOrdinal is null)
        {
            return defaultValueExp;
        }

        var innerType = Nullable.GetUnderlyingType(fieldType);

        var callGetValueExp = Expression.Call(
            dataReaderExp,
            GetValueMethodMap[innerType ?? fieldType],
            Expression.Constant(columnOrdinal));

        if (innerType is null && fieldType != typeof(string))
        {
            return callGetValueExp;
        }

        return Expression.Condition(
            Expression.IsTrue(Expression.Call(dataReaderExp, IsDbNullMethod, Expression.Constant(columnOrdinal))),
            defaultValueExp,
            Expression.Convert(callGetValueExp, fieldType));
    }
}
