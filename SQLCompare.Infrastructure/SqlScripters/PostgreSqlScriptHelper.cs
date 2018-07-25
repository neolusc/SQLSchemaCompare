﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Script helper class specific for PostgreSql database
    /// </summary>
    public class PostgreSqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public PostgreSqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Scripts the function argument data type
        /// </summary>
        /// <param name="argType">The argument data type</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The scripted function argument data type</returns>
        public static string ScriptFunctionArgumentType(uint argType, IEnumerable<ABaseDbObject> dataTypes)
        {
            var type = (PostgreSqlDataType)dataTypes.FirstOrDefault(x => ((PostgreSqlDataType)x).TypeId == argType);
            if (type == null)
            {
                throw new ArgumentException($"Unknown argument data type: {argType}");
            }

            if (type.IsArray && type.ArrayType != null)
            {
                return $"{ScriptDataTypeName(type.ArrayType.Name)}[]";
            }

            return ScriptDataTypeName(type.Name);
        }

        /// <summary>
        /// Scripts the function attributes
        /// </summary>
        /// <param name="function">The function</param>
        /// <returns>The scripted function attributes</returns>
        public static string ScriptFunctionAttributes(PostgreSqlFunction function)
        {
            StringBuilder sb = new StringBuilder();

            switch (function.Volatile)
            {
                case 'i':
                    sb.Append("IMMUTABLE");
                    break;
                case 's':
                    sb.Append("STABLE");
                    break;
                case 'v':
                    sb.Append("VOLATILE");
                    break;
                default: throw new ArgumentException($"Unknown function volatile: {function.Volatile}");
            }

            if (function.SecurityType == "DEFINER")
            {
                sb.Append(" SECURITY DEFINER");
            }

            if (function.IsStrict)
            {
                sb.Append(" STRICT");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Scripts the function arguments
        /// </summary>
        /// <param name="argType">The argument type</param>
        /// <param name="argMode">The argument mode</param>
        /// <param name="argName">The argument name</param>
        /// <param name="dataTypes">The list of database data types</param>
        /// <returns>The scripted function argument</returns>
        public static string ScriptFunctionArgument(uint argType, char argMode, string argName, IEnumerable<ABaseDbObject> dataTypes)
        {
            StringBuilder sb = new StringBuilder();

            switch (argMode)
            {
                case 'o':
                    sb.Append("OUT ");
                    break;
                case 'b':
                    sb.Append("INOUT ");
                    break;
                case 'v':
                    sb.Append("VARIADIC ");
                    break;
                case 'i':
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(argName))
            {
                sb.Append($"{argName} ");
            }

            sb.Append(PostgreSqlScriptHelper.ScriptFunctionArgumentType(argType, dataTypes));
            return sb.ToString();
        }

        /// <inheritdoc/>
        public override string ScriptObjectName(string objectSchema, string objectName)
        {
            if (this.Options.Scripting.UseSchemaName)
            {
                return $"\"{objectSchema}\".\"{objectName}\"";
            }
            else
            {
                return $"\"{objectName}\"";
            }
        }

        /// <inheritdoc/>
        public override string ScriptColumn(ABaseDbColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = column as PostgreSqlColumn;

            var sb = new StringBuilder();

            sb.Append($"\"{col.Name}\" {this.ScriptDataType(col)}");

            if (col.IsNullable)
            {
                sb.Append($" NULL");
            }
            else
            {
                sb.Append($" NOT NULL");
            }

            if (col.ColumnDefault != null)
            {
                sb.Append($" DEFAULT {col.ColumnDefault}");
            }

            return sb.ToString();
        }

        private static string ScriptDataTypeName(string dataTypeName)
        {
            switch (dataTypeName)
            {
                case "int8": return "bigint";
                case "serial8": return "bigserial";
                case "varbit": return "bit varying";
                case "bool": return "boolean";
                case "char": return "character";
                case "varchar": return "character varying";
                case "float8": return "double precision";
                case "int":
                case "int4": return "integer";
                case "decimal": return "numeric";
                case "float4": return "real";
                case "int2": return "smallint";
                case "serial2": return "smallserial";
                case "serial4": return "serial";
                case "time": return "time without time zone";
                case "timetz": return "time with time zone";
                case "timestamp": return "timestamp without time zone";
                case "timestamptz": return "timestamp with time zone";
                default:
                    return dataTypeName;
            }
        }

        private string ScriptDataType(PostgreSqlColumn column)
        {
            switch (column.DataType)
            {
                // Exact numerics
                case "smallint":
                case "integer":
                case "bigint":
                case "decimal":
                    return $"{column.DataType}";

                case "numeric":
                    return $"{column.DataType}({column.NumericPrecision},{column.NumericScale})"; // TODO: check if it's possible to only specify numeric without params

                // Approximate numerics
                case "real":
                case "double precision":

                // TODO: check if float can be specified
                // case "float":
                // return $"{column.DataType}({column.NumericPrecision})";

                // Money
                case "money":
                    return $"{column.DataType}";

                // Character strings
                case "character":
                case "character varying":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{column.DataType}({column.CharacterMaxLenght}){collate}";
                    }

                case "text":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"{column.DataType}{collate}";
                    }

                // Date and time
                case "date":
                    return $"{column.DataType}";
                case "time with time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"time{precision} with time zone";
                    }

                case "time without time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"time{precision} without time zone";
                    }

                case "timestamp with time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"timestamp{precision} with time zone";
                    }

                case "timestamp without time zone":
                    {
                        var precision = (column.DateTimePrecision != 6) ? $"({column.DateTimePrecision})" : string.Empty;
                        return $"timestamp{precision} without time zone";
                    }

                case "interval":
                    {
                        var precision = string.Empty;
                        if (column.IntervalType != null)
                        {
                            precision = $" ({column.IntervalType})";
                        }
                        else if (column.DateTimePrecision != 6)
                        {
                            precision = $"({column.DateTimePrecision})";
                        }

                        return $"interval{precision}";
                    }

                // Binary strings
                case "bit":
                case "bit varying":
                    {
                        var precision = (column.CharacterMaxLenght != null) ? $"({column.CharacterMaxLenght})" : string.Empty;
                        return $"{column.DataType}{precision}";
                    }

                case "bytea":

                // Boolean
                case "boolean":
                    return $"{column.DataType}";

                // User defined types
                case "USER-DEFINED":
                    return $"{column.UdtName}";

                // Other data types
                case "point":
                case "line":
                case "lseg":
                case "box":
                case "path":
                case "polygon":
                case "circle":
                case "inet":
                case "cidr":
                case "macaddr":
                case "macaddr8":
                case "tsvector":
                case "tsquery":

                case "uuid":
                case "xml":
                case "json":
                case "jsonb":
                case "pg_lsn":
                case "txid_snapshot":

                    return $"{column.DataType}";
                case "ARRAY":
                    // TODO: Array type initialization does not consider parameters and no collation considered for string types
                    // E.g.: "myColumnName character varying(55)[4][4]"
                    // ==> currently will result in "myColumnName varchar[]"
                    return $"{column.UdtName.Replace("_", string.Empty)}[]";

                default: throw new ArgumentException($"Unknown column data type: {column.DataType}");
            }
        }
    }
}
