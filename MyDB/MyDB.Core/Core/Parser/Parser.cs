using System;
using System.Collections.Generic;
using MyDB.Common;
using MyDB.Core.AST;
using MyDB.Core.AST.Expressions;
using MyDB.Core.AST.Statements;

namespace MyDB.Core.Parser
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        // Entry point to parse a single statement
        public Statement ParseStatement()
        {
            if (Match(TokenType.CREATE)) return ParseCreateTable();
            if (Match(TokenType.INSERT)) return ParseInsert();
            if (Match(TokenType.SELECT)) return ParseSelect();
            if (Match(TokenType.UPDATE)) return ParseUpdate();
            if (Match(TokenType.DELETE)) return ParseDelete();

            throw new ParserException(Peek(), "Expected a statement");
        }

        // Helper methods
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool Match(TokenType type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd()) return false;
            return Peek().Type == type;
        }

        private bool IsAtEnd() => Peek().Type == TokenType.EOF;

        private Token Peek() => _tokens[_current];

        private Token Previous() => _tokens[_current - 1];

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();
            throw new ParserException(Peek(), message);
        }

        //FUNCTIONS
        private Statement ParseCreateTable()
        {
            // Already consumed CREATE
            Consume(TokenType.TABLE, "Expected TABLE after CREATE");

            // Table name
            Token tableNameToken = Consume(TokenType.IDENTIFIER, "Expected table name");
            string tableName = tableNameToken.Lexeme;

            // Opening parenthesis
            Consume(TokenType.LEFT_PAREN, "Expected '(' after table name");

            // Parse columns
            List<ColumnDefinition> columns = new List<ColumnDefinition>();
            do
            {
                columns.Add(ParseColumnDefinition());
            } while (Match(TokenType.COMMA));

            // Closing parenthesis
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after columns");

            return new CreateTableStatement(tableName, columns);
        }

        private ColumnDefinition ParseColumnDefinition()
        {
            // Column name
            Token nameToken = Consume(TokenType.IDENTIFIER, "Expected column name");
            string columnName = nameToken.Lexeme;

            // Data type (INT or TEXT)
            Token typeToken;

            // Try INT first
            try
            {
                typeToken = Consume(TokenType.INT, "Expected data type INT or TEXT");
            }
            catch (ParserException)
            {
                // If not INT, try TEXT
                typeToken = Consume(TokenType.TEXT, "Expected data type INT or TEXT");
            }

            TokenType dataType = typeToken.Type;

            // Optional constraints
            bool isPrimaryKey = false;
            bool isUnique = false;

            if (Match(TokenType.PRIMARY))
            {
                Consume(TokenType.KEY, "Expected KEY after PRIMARY");
                isPrimaryKey = true;
            }
            else if (Match(TokenType.UNIQUE))
            {
                isUnique = true;
            }

            return new ColumnDefinition(columnName, dataType, isPrimaryKey, isUnique);
        }


        private Statement ParseInsert()
        {
            // Already consumed INSERT
            Consume(TokenType.INTO, "Expected INTO after INSERT");

            // Table name
            Token tableToken = Consume(TokenType.IDENTIFIER, "Expected table name after INTO");
            string tableName = tableToken.Lexeme;

            // Optional column list
            List<string> columns = new List<string>();
            if (Match(TokenType.LEFT_PAREN))
            {
                do
                {
                    Token colToken = Consume(TokenType.IDENTIFIER, "Expected column name");
                    columns.Add(colToken.Lexeme);
                } while (Match(TokenType.COMMA));

                Consume(TokenType.RIGHT_PAREN, "Expected ')' after column list");
            }

            // VALUES keyword
            Consume(TokenType.VALUES, "Expected VALUES keyword");

            // Parse values list
            List<Expression> values = new List<Expression>();
            Consume(TokenType.LEFT_PAREN, "Expected '(' before values");
            do
            {
                values.Add(ParseValue());
            } while (Match(TokenType.COMMA));
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after values");

            return new InsertStatement(tableName, columns, values);
        }
        private Expression ParseValue()
        {
            Token token = Peek();

            if (Match(TokenType.NUMBER))
            {
                return new LiteralExpression(token.Literal!);
            }
            else if (Match(TokenType.STRING))
            {
                return new LiteralExpression(token.Literal!);
            }
            else
            {
                throw new ParserException(token, "Expected a value (number or string)");
            }
        }
        // ==========================
        // Parse SELECT statement
        // ==========================
        private Statement ParseSelect()
        {
            // 1️⃣ Columns
            List<Expression> columns = new List<Expression>();
            if (Match(TokenType.STAR))
            {
                columns.Add(new ColumnExpression("*"));
            }
            else
            {
                do
                {
                    Token colToken = Consume(TokenType.IDENTIFIER, "Expected column name");
                    columns.Add(new ColumnExpression(colToken.Lexeme));
                } while (Match(TokenType.COMMA));
            }

            // 2️⃣ FROM table
            Consume(TokenType.FROM, "Expected FROM after column list");
            Token tableToken = Consume(TokenType.IDENTIFIER, "Expected table name after FROM");
            string tableName = tableToken.Lexeme;

            // 3️⃣ Optional INNER JOIN
            List<JoinClause>? joins = null;
            while (Match(TokenType.INNER))
            {
                Consume(TokenType.JOIN, "Expected JOIN after INNER");

                if (joins == null) joins = new List<JoinClause>();

                // Join table
                Token joinTableToken = Consume(TokenType.IDENTIFIER, "Expected table name to join");
                string joinTable = joinTableToken.Lexeme;

                // Optional alias
                string alias = joinTable;
                if (Match(TokenType.IDENTIFIER))
                {
                    Token aliasToken = Previous();
                    alias = aliasToken.Lexeme;
                }

                Consume(TokenType.ON, "Expected ON after JOIN table");

                // ON leftCol = rightCol
                Token leftColToken = Consume(TokenType.IDENTIFIER, "Expected left column in JOIN condition");
                Consume(TokenType.EQUALS, "Expected = in JOIN condition");
                Token rightColToken = Consume(TokenType.IDENTIFIER, "Expected right column in JOIN condition");

                var joinClause = new JoinClause(joinTable, alias,
                    new ColumnExpression(leftColToken.Lexeme),
                    new ColumnExpression(rightColToken.Lexeme));

                joins.Add(joinClause);
            }

            // 4️⃣ Optional WHERE
            Expression? whereClause = null;
            if (Match(TokenType.WHERE))
            {
                whereClause = ParseLogicalExpression(); // supports AND, OR, !=, >, <, etc.
            }

            // 5️⃣ Optional ORDER BY
            List<(string Column, bool Ascending)>? orderBy = null;
            if (Match(TokenType.ORDER))
            {
                Consume(TokenType.BY, "Expected BY after ORDER");
                orderBy = new List<(string Column, bool Ascending)>();
                do
                {
                    Token colToken = Consume(TokenType.IDENTIFIER, "Expected column name in ORDER BY");
                    bool ascending = true;
                    if (Match(TokenType.ASC)) ascending = true;
                    else if (Match(TokenType.DESC)) ascending = false;
                    orderBy.Add((colToken.Lexeme, ascending));
                } while (Match(TokenType.COMMA));
            }

            // 6️⃣ Optional LIMIT
            int? limit = null;
            if (Match(TokenType.LIMIT))
            {
                Token numToken = Consume(TokenType.NUMBER, "Expected number after LIMIT");
                limit = Convert.ToInt32(numToken.Literal);
            }

            // 7️⃣ Return SelectStatement with optional JOINs
            var selectStmt = new SelectStatement(columns, tableName, whereClause, orderBy, limit, joins);

            return selectStmt;
        }


        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        // ==========================
        // Parse logical expression for WHERE
        // Supports multiple conditions with AND/OR
        // ==========================
        private Expression ParseLogicalExpression()
        {
            Expression left = ParseExpression();

            while (Match(TokenType.AND, TokenType.OR))
            {
                Token opToken = Previous();
                Expression right = ParseExpression();
                left = new LogicalExpression(left, opToken.Type, right);
            }

            return left;
        }

        // ==========================
        // Parse simple expression (column operator value)
        // ==========================
        private Expression ParseExpression()
        {
            Token leftToken = Consume(TokenType.IDENTIFIER, "Expected column name");
            Expression left = new ColumnExpression(leftToken.Lexeme);

            Token opToken = ConsumeAny(new[]
            {
        TokenType.EQUALS, TokenType.LESS, TokenType.GREATER,
        TokenType.LESS_EQUAL, TokenType.GREATER_EQUAL, TokenType.NOT_EQUALS
    }, "Expected operator in expression");

            Token next = Peek();
            Expression right;
            if (Match(TokenType.NUMBER)) right = new LiteralExpression(next.Literal!);
            else if (Match(TokenType.STRING)) right = new LiteralExpression(next.Literal!);
            else throw new ParserException(next, "Expected literal value in expression");

            return new BinaryExpression(left, opToken.Type, right);
        }

        // Helper to consume one of several token types
        private Token ConsumeAny(TokenType[] types, string message)
        {
            foreach (var t in types)
            {
                if (Check(t)) return Advance();
            }
            throw new ParserException(Peek(), message);
        }

        private Statement ParseUpdate()
        {
            // Table name
            Token tableToken = Consume(TokenType.IDENTIFIER, "Expected table name after UPDATE");
            string tableName = tableToken.Lexeme;

            // SET keyword
            Consume(TokenType.SET, "Expected SET after table name");

            // Parse set clauses
            Dictionary<string, Expression> setClauses = new Dictionary<string, Expression>();
            do
            {
                Token colToken = Consume(TokenType.IDENTIFIER, "Expected column name in SET clause");
                Consume(TokenType.EQUALS, "Expected '=' after column name in SET clause");
                Expression value = ParseValue();
                setClauses[colToken.Lexeme] = value;
            } while (Match(TokenType.COMMA));

            // Optional WHERE clause
            Expression? whereClause = null;
            if (Match(TokenType.WHERE))
            {
                whereClause = ParseExpression();
            }

            return new UpdateStatement(tableName, setClauses, whereClause);
        }
        private Statement ParseDelete()
        {
            // Already consumed DELETE
            Consume(TokenType.FROM, "Expected FROM after DELETE");

            // Table name
            Token tableToken = Consume(TokenType.IDENTIFIER, "Expected table name after FROM");
            string tableName = tableToken.Lexeme;

            // Optional WHERE clause
            Expression? whereClause = null;
            if (Match(TokenType.WHERE))
            {
                whereClause = ParseExpression();
            }

            return new DeleteStatement(tableName, whereClause);
        }


    }

    // Custom parser exception
    public class ParserException : Exception
    {
        public Token Token { get; }
        public ParserException(Token token, string message) : base($"{message} at position {token.Position}")
        {
            Token = token;
        }
    }
}
