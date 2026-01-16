using MyDB.Common;

namespace MyDB.Core.AST
{
    public class ColumnDefinition
    {
        public string Name { get; }
        public TokenType DataType { get; }
        public bool IsPrimaryKey { get; }
        public bool IsUnique { get; }

        public ColumnDefinition(string name, TokenType dataType, bool isPrimaryKey = false, bool isUnique = false)
        {
            Name = name;
            DataType = dataType;
            IsPrimaryKey = isPrimaryKey;
            IsUnique = isUnique;
        }
    }
}
