using MyDB.Core.AST.Expressions;

namespace MyDB.Core.AST.Statements
{
    public class DeleteStatement : Statement
    {
        public string TableName { get; }
        public Expression? WhereClause { get; }

        public DeleteStatement(string tableName, Expression? whereClause = null)
        {
            TableName = tableName;
            WhereClause = whereClause;
        }
    }
}
