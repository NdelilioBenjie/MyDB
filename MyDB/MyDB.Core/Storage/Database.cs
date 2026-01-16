using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MyDB.Core.AST.Expressions;
using MyDB.Core.AST.Statements;
using MyDB.Common;
using MyDB.Core.AST;

namespace MyDB.Core.Storage
{
    public class Database
    {
        private readonly Dictionary<string, Table> _tables = new Dictionary<string, Table>(StringComparer.OrdinalIgnoreCase);
        private readonly string _dataFolder;

        public Database(string dataFolder = "data")
        {
            _dataFolder = dataFolder;

            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            LoadTablesFromDisk();
        }

        private void LoadTablesFromDisk()
        {
            string schemaPath = Path.Combine(_dataFolder, "schema.json");
            if (!File.Exists(schemaPath))
            {
                File.WriteAllText(schemaPath, "{}");
                return;
            }

            var schemaJson = File.ReadAllText(schemaPath);
            var tableNames = JsonSerializer.Deserialize<Dictionary<string, object>>(schemaJson);

            if (tableNames == null) return;

            foreach (var kvp in tableNames)
            {
                string tableName = kvp.Key;
                string tablePath = Path.Combine(_dataFolder, $"{tableName}.json");
                if (!File.Exists(tablePath))
                {
                    _tables[tableName] = new Table(tableName, new List<ColumnDefinition>());
                    continue;
                }

                string tableJson = File.ReadAllText(tablePath);
                var tableData = JsonSerializer.Deserialize<TableSerialized>(tableJson);
                if (tableData != null)
                {
                    var table = new Table(tableData.Name, tableData.Columns);
                    table.Rows.AddRange(tableData.Rows.Select(r => new Dictionary<string, object>(r)));
                    _tables[tableName] = table;
                }
            }
        }

        private void SaveTableToDisk(Table table)
        {
            string tablePath = Path.Combine(_dataFolder, $"{table.Name}.json");

            var tableSerialized = new TableSerialized
            {
                Name = table.Name,
                Columns = table.Columns,
                Rows = table.Rows
            };

            string json = JsonSerializer.Serialize(tableSerialized, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(tablePath, json);

            string schemaPath = Path.Combine(_dataFolder, "schema.json");
            var schemaDict = new Dictionary<string, object>();
            if (File.Exists(schemaPath))
            {
                schemaDict = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(schemaPath)) ?? new();
            }
            schemaDict[table.Name] = new { };
            File.WriteAllText(schemaPath, JsonSerializer.Serialize(schemaDict, new JsonSerializerOptions { WriteIndented = true }));
        }

        public void CreateTable(CreateTableStatement stmt)
        {
            if (_tables.ContainsKey(stmt.TableName))
                throw new Exception($"Table '{stmt.TableName}' already exists.");

            var table = new Table(stmt.TableName, stmt.Columns);
            _tables[stmt.TableName] = table;

            SaveTableToDisk(table);
        }

        public Table GetTable(string name)
        {
            if (!_tables.TryGetValue(name, out var table))
                throw new Exception($"Table '{name}' does not exist.");
            return table;
        }

        public void Insert(InsertStatement stmt)
        {
            var table = GetTable(stmt.TableName);

            List<string> columnsToInsert = stmt.Columns.Count > 0
                ? stmt.Columns
                : table.Columns.Select(c => c.Name).ToList();

            if (stmt.Values.Count != columnsToInsert.Count)
                throw new Exception("Number of values does not match number of columns.");

            var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < columnsToInsert.Count; i++)
            {
                var col = table.GetColumn(columnsToInsert[i]);
                var valueExpr = stmt.Values[i];
                object value = valueExpr switch
                {
                    LiteralExpression lit => lit.Value!,
                    _ => throw new Exception($"Unsupported expression type for INSERT: {valueExpr.GetType().Name}")
                };

                if (col.IsPrimaryKey || col.IsUnique)
                {
                    foreach (var existingRow in table.Rows)
                    {
                        if (existingRow.TryGetValue(col.Name, out var existingValue) && Equals(existingValue, value))
                            throw new Exception($"Constraint violation: column '{col.Name}' must be unique.");
                    }
                }

                row[col.Name] = value;
            }

            table.Rows.Add(row);

            SaveTableToDisk(table);
            Console.WriteLine($"Inserted row into table '{table.Name}' successfully.");
        }

        public List<Dictionary<string, object>> Select(SelectStatement stmt)
        {
            var mainTable = GetTable(stmt.TableName);
            var workingRows = mainTable.Rows.Select(r => new Dictionary<string, object>(r)).ToList();

            if (stmt.Joins != null)
            {
                foreach (var join in stmt.Joins)
                {
                    var joinTable = GetTable(join.TableName);
                    var newRows = new List<Dictionary<string, object>>();

                    foreach (var row in workingRows)
                    {
                        foreach (var joinRow in joinTable.Rows)
                        {
                            if (!row.ContainsKey(join.LeftColumn.Name) || !joinRow.ContainsKey(join.RightColumn.Name))
                                continue;

                            if (row[join.LeftColumn.Name].Equals(joinRow[join.RightColumn.Name]))
                            {
                                var mergedRow = new Dictionary<string, object>(row);
                                foreach (var kvp in joinRow)
                                    mergedRow[$"{join.TableName}.{kvp.Key}"] = kvp.Value;
                                newRows.Add(mergedRow);
                            }
                        }
                    }
                    workingRows = newRows;
                }
            }

            if (stmt.WhereClause != null)
                workingRows = workingRows.Where(row => EvaluateExpression(row, stmt.WhereClause!)).ToList();

            List<string> columnsToSelect = stmt.Columns.Count == 1 && stmt.Columns[0] is ColumnExpression colExpr && colExpr.Name == "*"
                ? workingRows.FirstOrDefault()?.Keys.ToList() ?? new List<string>()
                : stmt.Columns.OfType<ColumnExpression>().Select(c => c.Name).ToList();

            var results = workingRows.Select(row =>
            {
                var resultRow = new Dictionary<string, object>();
                foreach (var col in columnsToSelect)
                    if (row.ContainsKey(col))
                        resultRow[col] = row[col];
                return resultRow;
            }).ToList();

            if (stmt.OrderBy != null)
            {
                foreach (var (col, asc) in stmt.OrderBy.AsEnumerable().Reverse())
                {
                    results = asc
                        ? results.OrderBy(r => r.ContainsKey(col) ? r[col] : null).ToList()
                        : results.OrderByDescending(r => r.ContainsKey(col) ? r[col] : null).ToList();
                }
            }

            if (stmt.Limit.HasValue)
                results = results.Take(stmt.Limit.Value).ToList();

            return results;
        }

        public int Update(UpdateStatement stmt)
        {
            var table = GetTable(stmt.TableName);
            int updatedCount = 0;

            foreach (var row in table.Rows)
            {
                bool match = stmt.WhereClause != null && !EvaluateExpression(row, stmt.WhereClause!) ? false : true;

                if (match)
                {
                    foreach (var kvp in stmt.SetClauses)
                    {
                        if (kvp.Value is LiteralExpression lit)
                            row[kvp.Key] = lit.Value!;
                        else
                            throw new Exception("Only literal values are supported in SET clause for now.");
                    }
                    updatedCount++;
                }
            }

            SaveTableToDisk(table);
            return updatedCount;
        }

        public int Delete(DeleteStatement stmt)
        {
            var table = GetTable(stmt.TableName);
            List<Dictionary<string, object>> rowsToDelete = stmt.WhereClause != null
                ? table.Rows.Where(row => EvaluateExpression(row, stmt.WhereClause)).ToList()
                : table.Rows.ToList();

            foreach (var row in rowsToDelete)
                table.Rows.Remove(row);

            SaveTableToDisk(table);
            return rowsToDelete.Count;
        }

        private bool EvaluateExpression(Dictionary<string, object> row, Expression expr)
        {
            if (expr is BinaryExpression binExpr &&
                binExpr.Left is ColumnExpression leftCol &&
                binExpr.Right is LiteralExpression rightLit)
            {
                object rowValue = row[leftCol.Name];
                object literalValue = rightLit.Value!;

                if (rowValue is int ri && literalValue is int li)
                {
                    return binExpr.Operator switch
                    {
                        TokenType.EQUALS => ri == li,
                        TokenType.NOT_EQUALS => ri != li,
                        TokenType.GREATER => ri > li,
                        TokenType.LESS => ri < li,
                        TokenType.GREATER_EQUAL => ri >= li,
                        TokenType.LESS_EQUAL => ri <= li,
                        _ => throw new Exception("Unsupported operator in WHERE clause")
                    };
                }
                else
                {
                    string rs = rowValue.ToString()!;
                    string ls = literalValue.ToString()!;
                    return binExpr.Operator switch
                    {
                        TokenType.EQUALS => rs == ls,
                        TokenType.NOT_EQUALS => rs != ls,
                        _ => throw new Exception("Unsupported operator for TEXT values")
                    };
                }
            }
            else if (expr is LogicalExpression logExpr)
            {
                bool left = EvaluateExpression(row, logExpr.Left);
                bool right = EvaluateExpression(row, logExpr.Right);

                return logExpr.Operator switch
                {
                    TokenType.AND => left && right,
                    TokenType.OR => left || right,
                    _ => throw new Exception("Unsupported logical operator")
                };
            }
            else
                throw new Exception("Unsupported WHERE expression type");
        }

        private bool EvaluateExpression(Expression expr, Dictionary<string, object> row)
        {
            switch (expr)
            {
                case BinaryExpression binExpr:
                    var left = GetExpressionValue(binExpr.Left, row);
                    var right = GetExpressionValue(binExpr.Right, row);
                    return binExpr.Operator switch
                    {
                        TokenType.EQUALS => Equals(left, right),
                        TokenType.NOT_EQUALS => !Equals(left, right),
                        TokenType.GREATER => (int)left > (int)right,
                        TokenType.LESS => (int)left < (int)right,
                        TokenType.GREATER_EQUAL => (int)left >= (int)right,
                        TokenType.LESS_EQUAL => (int)left <= (int)right,
                        _ => throw new Exception($"Unsupported operator: {binExpr.Operator}")
                    };
                default:
                    throw new Exception($"Unsupported expression type: {expr.GetType().Name}");
            }
        }

        private object GetExpressionValue(Expression expr, Dictionary<string, object> row)
        {
            return expr switch
            {
                ColumnExpression colExpr => row[colExpr.Name]!,
                LiteralExpression litExpr => litExpr.Value!,
                _ => throw new Exception($"Unsupported expression type: {expr.GetType().Name}")
            };
        }

        private class TableSerialized
        {
            public string Name { get; set; } = "";
            public List<ColumnDefinition> Columns { get; set; } = new List<ColumnDefinition>();
            public List<Dictionary<string, object>> Rows { get; set; } = new List<Dictionary<string, object>>();
        }
    }
}
