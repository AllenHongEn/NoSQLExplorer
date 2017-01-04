﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NoSqlExplorer.DAL.Common;

namespace NoSqlExplorer.Crate.DAL.Util
{
  internal static class StatementHelper
  {
    internal static string CreateTableStatement(Type type, int? shards = null, int? replicas = null)
    {
      var props = type.GetProperties();
      if (props.Length == 0)
      {
        throw new InvalidOperationException($"Class {type.Name} does not contain any valid properties");
      }

      var statement = new StringBuilder($"create table {GetTableName(type)} (");
      foreach (var column in props)
      {
        var crateType = GetCrateColumnType(column.PropertyType);
        statement.Append($"{column.Name} {crateType}");

        if (column.GetCustomAttribute(typeof(PrimaryKeyAttribute)) != null)
        {
          statement.Append(" primary key");
        }

        if (column.Name != props.Last().Name)
        {
          statement.Append(", ");
        }
      }

      statement.Append(")");

      if (shards != null)
      {
        statement.Append($" clustered into {shards} shards");
      }

      if (replicas != null)
      {
        statement.Append($" with (number_of_replicas = {replicas})");
      }

      return statement.ToString();
    }

    private static string GetTableName(Type type)
    {
      var tableNameAttr = type.GetCustomAttribute<TableNameAttribute>();
      if (tableNameAttr != null)
      {
        return tableNameAttr.Name;
      }
      else
      {
        return type.Name.ToLower();
      }
    }

    private static string GetCrateColumnType(Type propertyType)
    {
      var type = "string";
      switch (propertyType.Name)
      {
        case nameof(Int32):
          type = "integer";
          break;
        case nameof(Boolean):
          type = "boolean";
          break;
        case nameof(Double):
          type = "double";
          break;
        case nameof(Single):
          type = "float";
          break;
        default:
          break;
      }

      return type;
    }
  }
}
