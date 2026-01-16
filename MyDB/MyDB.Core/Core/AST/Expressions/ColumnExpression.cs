namespace MyDB.Core.AST.Expressions
{
    public class ColumnExpression : Expression
    {
        public string Name { get; }

        public ColumnExpression(string name)
        {
            Name = name;
        }
    }
}
