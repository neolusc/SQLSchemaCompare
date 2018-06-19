﻿using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MicrosoftSql;
using SQLCompare.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Script helper class specific for MicrosoftSql database
    /// </summary>
    public class MicrosoftSqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftSqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public MicrosoftSqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Script the foreign key reference action
        /// </summary>
        /// <param name="action">The reference action</param>
        /// <returns>The scripted action</returns>
        public static string ScriptForeignKeyAction(MicrosoftSqlForeignKey.ReferentialAction action)
        {
            switch (action)
            {
                case MicrosoftSqlForeignKey.ReferentialAction.NOACTION: return "NO ACTION";
                case MicrosoftSqlForeignKey.ReferentialAction.CASCADE: return "CASCADE";
                case MicrosoftSqlForeignKey.ReferentialAction.SETDEFAULT: return "SET DEFAULT";
                case MicrosoftSqlForeignKey.ReferentialAction.SETNULL: return "SET NULL";
                default:
                    throw new ArgumentException("Invalid referential action: " + action.ToString(), nameof(action));
            }
        }

        /// <inheritdoc/>
        public override string ScriptTableName(string tableSchema, string tableName)
        {
            if (this.Options.Scripting.UseSchemaName)
            {
                return $"[{tableSchema}].[{tableName}]";
            }
            else
            {
                return $"[{tableName}]";
            }
        }

        /// <inheritdoc/>
        public override string ScriptColumn(ABaseDbColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = column as MicrosoftSqlColumn;

            StringBuilder sb = new StringBuilder();

            sb.Append($"[{col.Name}] {this.ScriptDataType(col)} ");

            if (!col.IsComputed)
            {
                if (col.IsIdentity)
                {
                    sb.Append($"IDENTITY({col.IdentitySeed},{col.IdentityIncrement}) ");
                }

                if (col.ColumnDefault != null)
                {
                    sb.Append($"DEFAULT {col.ColumnDefault} ");
                }

                if (col.IsNullable)
                {
                    sb.Append($"NULL");
                }
                else
                {
                    sb.Append($"NOT NULL");
                }
            }

            return sb.ToString();
        }

        private string ScriptDataType(MicrosoftSqlColumn column)
        {
            // Computed columns return AS + definition
            if (column.IsComputed)
            {
                return $"AS {column.Definition}";
            }

            switch (column.DataType)
            {
                // Exact numerics
                case "bigint":
                case "int":
                case "smallint":
                case "tinyint":
                case "bit":
                case "smallmoney":
                case "money":
                    return $"[{column.DataType}]";
                case "numeric":
                case "decimal":
                    return $"[{column.DataType}]({column.NumericPrecision}, {column.NumericScale})";

                // Approximate numerics
                case "float":
                    return (column.NumericPrecision == 53) ? $"[{column.DataType}]" : $"[{column.DataType}]({column.NumericPrecision})";
                case "real":
                    return $"[{column.DataType}]";

                // Date and time
                case "date":
                case "datetime":
                case "smalldatetime":
                    return $"[{column.DataType}]";
                case "datetimeoffset":
                case "datetime2":
                case "time":
                    return $"[{column.DataType}]({column.DateTimePrecision})";

                // Character strings
                // Unicode character strings
                case "char":
                case "nchar":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"[{column.DataType}]({column.CharacterMaxLenght}){collate}";
                    }

                case "varchar":
                case "nvarchar":
                    {
                        string length = column.CharacterMaxLenght == -1 ? "max" : $"{column.CharacterMaxLenght}";
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";

                        return $"[{column.DataType}]({length}){collate}";
                    }

                case "text":
                case "ntext":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        return $"[{column.DataType}]{collate}";
                    }

                // Binary strings
                case "binary":
                    return $"[{column.DataType}]({column.CharacterMaxLenght})";
                case "varbinary":
                    {
                        string length = column.CharacterMaxLenght == -1 ? "max" : $"{column.CharacterMaxLenght}";
                        return $"[{column.DataType}]({length})";
                    }

                case "image":
                    return $"[{column.DataType}]";

                // Other data types
                case "cursor":
                case "rowversion":
                case "hierarchyid":
                case "uniqueidentifier":
                case "sql_variant":
                case "xml":
                case "geography":
                case "geometry":
                    return $"[{column.DataType}]";
                default: throw new ArgumentException($"Unknown column data type: {column.DataType}");
            }
        }
    }
}