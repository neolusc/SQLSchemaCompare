﻿using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.MySql;
using SQLCompare.Core.Entities.Project;
using System;
using System.Collections.Generic;
using System.Text;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Script helper class specific for MySql database
    /// </summary>
    public class MySqlScriptHelper : AScriptHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlScriptHelper"/> class.
        /// </summary>
        /// <param name="options">The project options</param>
        public MySqlScriptHelper(ProjectOptions options)
            : base(options)
        {
        }

        ///// <summary>
        ///// Script the foreign key reference action
        ///// </summary>
        ///// <param name="action">The reference action</param>
        ///// <returns>The scripted action</returns>
        // public static string ScriptForeignKeyAction(MySqlForeignKey.ReferentialAction action)
        // {
        //    switch (action)
        //    {
        //        case MySqlForeignKey.ReferentialAction.NOACTION: return "NO ACTION";
        //        case MySqlForeignKey.ReferentialAction.CASCADE: return "CASCADE";
        //        case MySqlForeignKey.ReferentialAction.SETDEFAULT: return "SET DEFAULT";ed
        //        case MySqlForeignKey.ReferentialAction.SETNULL: return "SET NULL";
        //        default:
        //            throw new ArgumentException("Invalid referential action: " + action.ToString(), nameof(action));
        //    }
        // }

        /// <inheritdoc/>
        public override string ScriptTableName(string tableSchema, string tableName)
        {
            if (this.Options.Scripting.UseSchemaName)
            {
                return $"`{tableSchema}`.`{tableName}`";
            }
            else
            {
                return $"`{tableName}`";
            }
        }

        /// <inheritdoc/>
        public override string ScriptColumn(ABaseDbColumn column)
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            var col = column as MySqlColumn;

            var sb = new StringBuilder();

            sb.Append($"`{col.Name}` {this.ScriptDataType(col)} ");

            if (!col.Extra.Equals("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase) && !col.Extra.Equals("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(col.IsNullable, "YES", StringComparison.Ordinal))
                {
                    sb.Append($"NULL ");
                }
                else
                {
                    sb.Append($"NOT NULL ");
                }

                if (col.ColumnDefault != null)
                {
                    sb.Append($"DEFAULT {col.ColumnDefault} ");
                }

                if (col.Extra != null)
                {
                    sb.Append($"{col.Extra}");
                }
            }

            return sb.ToString();
        }

        private string ScriptDataType(MySqlColumn column)
        {
            if (column.Extra.Equals("VIRTUAL GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                return $"{column.ColumnType} AS {column.GenerationExpression} VIRTUAL";
            }
            else if (column.Extra.Equals("STORED GENERATED", StringComparison.OrdinalIgnoreCase))
            {
                return $"{column.ColumnType} AS {column.GenerationExpression} PERSISTENT";
            }

            switch (column.DataType)
            {
                // Exact numerics
                case "bit":
                case "tinyint":
                case "smallint":
                case "mediumint":
                case "int":
                case "integer":
                case "bigint":
                case "numeric":
                case "decimal":

                // Approximate numerics
                case "real":
                case "double":
                case "float":

                // Date and time
                case "date":
                case "year":
                case "time":
                case "timestamp":
                case "datetime":

                // Binary strings
                case "binary":
                case "varbinary":
                case "tinyblob":
                case "mediumblob":
                case "longblob":

                // Other data types
                case "enum":
                case "set":
                case "json":
                case "geometry":
                case "point":
                case "linestring":
                case "polygon":
                case "multipoint":
                case "multilinestring":
                case "multipolygon":
                case "geometrycollection":
                    return $"{column.ColumnType}";

                // Character strings
                case "char":
                case "varchar":
                case "text":
                case "tinytext":
                case "mediumtext":
                case "longtext":
                    {
                        string collate = this.Options.Scripting.IgnoreCollate ? string.Empty : $" COLLATE {column.CollationName}";
                        string charachterSet = $" CHARACTER SET {column.CharacterSetName}";
                        return $"{column.ColumnType}({column.CharacterMaxLenght}){charachterSet}{collate}";
                    }

                default: throw new ArgumentException($"Unknown column data type: {column.DataType}");
            }
        }
    }
}