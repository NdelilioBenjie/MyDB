using MyDB.Core.AST.Expressions;
using System.Collections.Generic;

namespace MyDB.Core.AST.Statements
{
    public class JoinClause
    {
        public string TableName { get; }
        public string Alias { get; }
        public ColumnExpression LeftColumn { get; }
        public ColumnExpression RightColumn { get; }

        public JoinClause(string tableName, string alias, ColumnExpression left, ColumnExpression right)
        {
            TableName = tableName;
            Alias = alias;
            LeftColumn = left;
            RightColumn = right;
        }
    }

    public class SelectStatement : Statement
    {
        public List<Expression> Columns { get; }
        public string TableName { get; }
        public Expression? WhereClause { get; }
        public List<(string Column, bool Ascending)>? OrderBy { get; }
        public int? Limit { get; }
        public List<JoinClause>? Joins { get; set; } // ✅ Changed to JoinClause

        public SelectStatement(
            List<Expression> columns,
            string tableName,
            Expression? whereClause = null,
            List<(string Column, bool Ascending)>? orderBy = null,
            int? limit = null,
            List<JoinClause>? joins = null) // ✅ Constructor uses JoinClause
        {
            Columns = columns;
            TableName = tableName;
            WhereClause = whereClause;
            OrderBy = orderBy;
            Limit = limit;
            Joins = joins;
        }
    }
}
