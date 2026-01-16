using System.Collections.Generic;
using MyDB.Core.AST.Expressions;

namespace MyDB.Core.AST.Statements
{
    public class InsertStatement : Statement
    {
        public string TableName { get; }
        public List<string> Columns { get; }
        public List<Expression> Values { get; }

        public InsertStatement(string tableName, List<string> columns, List<Expression> values)
        {
            TableName = tableName;
            Columns = columns;
            Values = values;
        }
    }
}
