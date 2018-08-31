﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using SQLCompare.Core.Entities.Database;
using SQLCompare.Core.Entities.Database.PostgreSql;
using SQLCompare.Core.Entities.Project;

namespace SQLCompare.Infrastructure.SqlScripters
{
    /// <summary>
    /// Sql scripter class specific for PostgreSql database
    /// </summary>
    internal class PostgreSqlScripter : ADatabaseScripter<PostgreSqlScriptHelper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgreSqlScripter"/> class.
        /// </summary>
        /// <param name="logger">The injected logger instance</param>
        /// <param name="options">The project options</param>
        public PostgreSqlScripter(ILogger logger, ProjectOptions options)
            : base(logger, options, new PostgreSqlScriptHelper(options))
        {
        }

        /// <inheritdoc/>
        protected override string ScriptCreateTable(ABaseDbTable table, ABaseDbTable referenceTable = null)
        {
            var ncol = table.Columns.Count;
            var columns = this.GetSortedTableColumns(table, referenceTable);

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {this.ScriptHelper.ScriptObjectName(table)}(");

            var i = 0;
            foreach (var col in columns)
            {
                sb.Append($"{this.Indent}{this.ScriptHelper.ScriptColumn(col)}");
                sb.AppendLine((++i == ncol) ? string.Empty : ",");
            }

            sb.AppendLine(");");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptPrimaryKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var keys in table.PrimaryKeys.GroupBy(x => x.Name))
            {
                var key = (PostgreSqlIndex)keys.First();
                var columnList = keys.OrderBy(x => ((PostgreSqlIndex)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlIndex)x).ColumnName}\"");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT \"{key.Name}\" PRIMARY KEY ({string.Join(",", columnList)});");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptForeignKeysAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var keys in table.ForeignKeys.GroupBy(x => x.Name))
            {
                var key = (PostgreSqlForeignKey)keys.First();
                var columnList = keys.OrderBy(x => ((PostgreSqlForeignKey)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlForeignKey)x).ColumnName}\"");
                var referencedColumnList = keys.OrderBy(x => ((PostgreSqlForeignKey)x).OrdinalPosition).Select(x => $"\"{((PostgreSqlForeignKey)x).ReferencedColumnName}\"");

                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT \"{key.Name}\" FOREIGN KEY ({string.Join(",", columnList)})");
                sb.AppendLine($"REFERENCES {this.ScriptHelper.ScriptObjectName(key.ReferencedTableSchema, key.ReferencedTableName)} ({string.Join(",", referencedColumnList)}) {PostgreSqlScriptHelper.ScriptForeignKeyMatchOption(key.MatchOption)}");
                sb.AppendLine($"ON DELETE {key.DeleteRule}");
                sb.AppendLine($"ON UPDATE {key.UpdateRule}");

                sb.AppendLine(key.IsDeferrable ? "DEFERRABLE" : "NOT DEFERRABLE");

                sb.AppendLine(key.IsInitiallyDeferred ? "INITIALLY DEFERRED;" : "INITIALLY IMMEDIATE;");

                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptConstraintsAlterTable(ABaseDbTable table)
        {
            var sb = new StringBuilder();
            foreach (var keys in table.Constraints.GroupBy(x => x.Name))
            {
                var key = keys.First();
                sb.AppendLine($"ALTER TABLE {this.ScriptHelper.ScriptObjectName(table)}");
                sb.AppendLine($"ADD CONSTRAINT \"{key.Name}\" {key.Definition};");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateIndexes(ABaseDbTable table)
        {
            var sb = new StringBuilder();

            foreach (var indexes in table.Indexes.Cast<PostgreSqlIndex>().GroupBy(x => x.Name))
            {
                var index = indexes.First();

                // If there is a column with descending order, specify the order on all columns
                var scriptOrder = indexes.Any(x => x.IsDescending);
                var columnList = indexes.OrderBy(x => x.OrdinalPosition).Select(x =>
                    scriptOrder ? $"\"{x.ColumnName}\" {(x.IsDescending ? "DESC" : "ASC")}" : $"\"{x.ColumnName}\"");

                sb.Append("CREATE ");
                if (index.IsUnique)
                {
                    sb.Append("UNIQUE ");
                }

                sb.Append($"INDEX {index.Name} ON {this.ScriptHelper.ScriptObjectName(table)} ");

                switch (index.Type)
                {
                    case "btree":
                        // If not specified it will use the BTREE
                        break;

                    case "gist":
                        sb.Append("USING gist ");
                        break;

                    case "hash":
                        sb.Append("USING hash ");
                        break;

                    default:
                        throw new NotSupportedException($"Index of type '{index.Type}' is not supported");
                }

                sb.AppendLine($"({string.Join(",", columnList)});");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateView(ABaseDbView view)
        {
            var sb = new StringBuilder();

            var checkOption = ((PostgreSqlView)view).CheckOption;
            if (checkOption == "NONE")
            {
                sb.AppendLine($"CREATE VIEW {this.ScriptHelper.ScriptObjectName(view)} AS");
            }
            else
            {
                sb.AppendLine($"CREATE VIEW {this.ScriptHelper.ScriptObjectName(view)}");
                sb.AppendLine("WITH(");
                sb.AppendLine($"{this.Indent}CHECK_OPTION = {checkOption}");
                sb.AppendLine(") AS");
            }

            sb.AppendLine($"{view.ViewDefinition}");
            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateFunction(ABaseDbFunction sqlFunction, IEnumerable<ABaseDbObject> dataTypes)
        {
            var sb = new StringBuilder();

            var function = (PostgreSqlFunction)sqlFunction;

            var args = function.AllArgTypes != null ? function.AllArgTypes.ToArray() : function.ArgTypes.ToArray();

            sb.Append($"CREATE FUNCTION {this.ScriptHelper.ScriptObjectName(function)}(");

            for (var i = 0; i < args.Length; i++)
            {
                var argType = args[i];
                var argMode = function.ArgModes?.ToArray()[i] ?? 'i';
                var argName = function.ArgNames?.ToArray()[i] ?? string.Empty;
                sb.AppendLine();
                sb.Append($"{this.Indent}{PostgreSqlScriptHelper.ScriptFunctionArgument(argType, argMode, argName, dataTypes)}");
                if (i != args.Length - 1)
                {
                    sb.Append(",");
                }
            }

            sb.AppendLine(")");

            var setOfString = function.ReturnSet ? "SETOF " : string.Empty;

            sb.AppendLine($"{this.Indent}RETURNS {setOfString}{PostgreSqlScriptHelper.ScriptFunctionArgumentType(function.ReturnType, dataTypes)}");
            sb.AppendLine($"{this.Indent}LANGUAGE {function.ExternalLanguage}");
            sb.AppendLine();
            sb.AppendLine($"{this.Indent}COST {function.Cost}");

            if (function.Rows > 0)
            {
                sb.AppendLine($"{this.Indent}ROWS {function.Rows}");
            }

            sb.AppendLine($"{this.Indent}{PostgreSqlScriptHelper.ScriptFunctionAttributes(function)}");
            sb.Append("AS $BODY$");
            sb.Append(function.Definition);
            sb.AppendLine("$BODY$;");
            sb.AppendLine();

            return sb.ToString();
        }

        /// <inheritdoc/>
        protected override string ScriptCreateStoredProcedure(ABaseDbStoredProcedure storedProcedure)
        {
            throw new NotSupportedException("PostgreSQL doesn't have stored procedures, only functions");
        }

        /// <inheritdoc/>
        protected override string ScriptCreateSequence(ABaseDbSequence sequence)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE SEQUENCE {this.ScriptHelper.ScriptObjectName(sequence)}");
            sb.AppendLine($"    AS {sequence.DataType}");
            sb.AppendLine($"    START WITH {sequence.StartValue}");
            sb.AppendLine($"    INCREMENT BY {sequence.Increment}");

            // TODO: Handle min/max values correctly
            sb.AppendLine("    NO MINVALUE");
            sb.AppendLine("    NO MAXVALUE");
            sb.AppendLine("    CACHE 1;");
            return sb.ToString();
        }

        /// <inheritdoc />
        protected override string ScriptCreateType(ABaseDbDataType type)
        {
            return string.Empty;
        }

        /// <inheritdoc/>
        protected override IEnumerable<ABaseDbColumn> OrderColumnsByOrdinalPosition(ABaseDbTable table)
        {
            return table.Columns.OrderBy(x => ((PostgreSqlColumn)x).OrdinalPosition);
        }
    }
}