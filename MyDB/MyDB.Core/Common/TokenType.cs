namespace MyDB.Common
{
    public enum TokenType
    {
        // Keywords
        CREATE,
        TABLE,
        INSERT,
        INTO,
        VALUES,
        SELECT,
        FROM,
        WHERE,
        UPDATE,
        SET,
        DELETE,
        JOIN,
        ON,
        PRIMARY,
        KEY,
        UNIQUE,
        INNER,


        // Data types
        INT,
        TEXT,

        // Identifiers & literals
        IDENTIFIER,
        NUMBER,
        STRING,

        // Operators
        EQUALS,        // =
        LESS,          // <
        GREATER,       // >
        LESS_EQUAL,    // <=
        GREATER_EQUAL, // >=

        // Symbols
        LEFT_PAREN,    // (
        RIGHT_PAREN,   // )
        COMMA,         // ,
        SEMICOLON,     // ;
        STAR,          // *

        DOT,

        // Logical
        AND,
        OR,
        NOT_EQUALS,

        ORDER,
        BY,
        ASC,
        DESC,
        LIMIT,

        // Special
        EOF
    }
}
