using System;
using System.Collections.Generic;
using MyDB.Core.AST.Statements;
using MyDB.Core.AST.Expressions;
using MyDB.Common;
using MyDB.Core.AST;


namespace MyDB.Core.Storage
{
    public class Table
    {
        public string Name { get; }
        public List<ColumnDefinition> Columns { get; }
        public List<Dictionary<string, object>> Rows { get; } = new List<Dictionary<string, object>>();

        public Table(string name, List<ColumnDefinition> columns)
        {
            Name = name;
            Columns = columns;
        }

        // Helper to get a column by name
        public ColumnDefinition GetColumn(string name)
        {
            foreach (var col in Columns)
            {
                if (col.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return col;
            }
            throw new Exception($"Column '{name}' does not exist in table '{Name}'.");
        }
    }
}
