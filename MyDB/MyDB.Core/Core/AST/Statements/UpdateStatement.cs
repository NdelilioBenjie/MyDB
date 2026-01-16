using System.Collections.Generic;
using MyDB.Core.AST.Expressions;

namespace MyDB.Core.AST.Statements
{
    public class UpdateStatement : Statement
    {
        public string TableName { get; }
        public Dictionary<string, Expression> SetClauses { get; }
        public Expression? WhereClause { get; }

        public UpdateStatement(string tableName, Dictionary<string, Expression> setClauses, Expression? whereClause = null)
        {
            TableName = tableName;
            SetClauses = setClauses;
            WhereClause = whereClause;
        }
    }
}
