using MyDB.Common;

namespace MyDB.Core.AST.Expressions
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; }
        public TokenType Operator { get; }
        public Expression Right { get; }

        public BinaryExpression(Expression left, TokenType op, Expression right)
        {
            Left = left;
            Operator = op;
            Right = right;
        }
    }
}
