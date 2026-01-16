using System;
using System.Collections.Generic;
using MyDB.Common;

namespace MyDB.Core.Tokenizer
{
    public class Tokenizer
    {
        private readonly string _source;
        private readonly List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;

        public Tokenizer(string source)
        {
            _source = source;
        }

        public List<Token> Tokenize()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _current));
            return _tokens;
        }

        private bool IsAtEnd() => _current >= _source.Length;

        private void ScanToken()
        {
            char c = Advance();

            switch (c)
            {
                case ' ':
                case '\r':
                case '\t':
                case '\n':
                    break; // Ignore whitespace

                case '(':
                    AddToken(TokenType.LEFT_PAREN);
                    break;

                case ')':
                    AddToken(TokenType.RIGHT_PAREN);
                    break;

                case ',':
                    AddToken(TokenType.COMMA);
                    break;

                case '.':
                    AddToken(TokenType.DOT, "."); Advance(); break;

                case ';':
                    AddToken(TokenType.SEMICOLON);
                    break;

                case '*':
                    AddToken(TokenType.STAR);
                    break;

                case '=':
                    AddToken(TokenType.EQUALS);
                    break;

                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;

                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;

                case '\'':
                    StringLiteral();
                    break;
                case '!':
                    if (Match('='))
                        AddToken(TokenType.NOT_EQUALS);
                    else
                        throw new Exception($"Unexpected character '!' at position {_current - 1}");
                    break;

                default:
                    if (IsDigit(c))
                        NumberLiteral();
                    else if (IsAlpha(c))
                        IdentifierOrKeyword();
                    else
                        throw new Exception($"Unexpected character '{c}' at position {_current - 1}");
                    break;
            }
        }

        private char Advance() => _source[_current++];

        private bool Match(char expected)
        {
            if (IsAtEnd()) return false;
            if (_source[_current] != expected) return false;
            _current++;
            return true;
        }

        private void AddToken(TokenType type, object? literal = null)
        {
            string lexeme = _source[_start.._current];
            _tokens.Add(new Token(type, lexeme, literal, _start));
        }

        private bool IsDigit(char c) => c >= '0' && c <= '9';
        private bool IsAlpha(char c) => char.IsLetter(c) || c == '_';
        private bool IsAlphaNumeric(char c) => IsAlpha(c) || IsDigit(c);

        private void NumberLiteral()
        {
            while (!IsAtEnd() && IsDigit(_source[_current]))
                _current++;

            string number = _source[_start.._current];
            AddToken(TokenType.NUMBER, int.Parse(number));
        }

        private void StringLiteral()
        {
            while (!IsAtEnd() && _source[_current] != '\'')
                _current++;

            if (IsAtEnd())
                throw new Exception("Unterminated string literal");

            _current++; // Consume closing '

            string value = _source[(_start + 1)..(_current - 1)];
            AddToken(TokenType.STRING, value);
        }

        private void IdentifierOrKeyword()
        {
            while (!IsAtEnd() && IsAlphaNumeric(_source[_current]))
                _current++;

            string text = _source[_start.._current].ToUpper();

            // Map keywords and data types
            TokenType type = text switch
            {
                "CREATE" => TokenType.CREATE,
                "TABLE" => TokenType.TABLE,
                "INSERT" => TokenType.INSERT,
                "INTO" => TokenType.INTO,
                "VALUES" => TokenType.VALUES,
                "SELECT" => TokenType.SELECT,
                "FROM" => TokenType.FROM,
                "WHERE" => TokenType.WHERE,
                "UPDATE" => TokenType.UPDATE,
                "SET" => TokenType.SET,
                "DELETE" => TokenType.DELETE,
                "JOIN" => TokenType.JOIN,
                "ON" => TokenType.ON,
                "PRIMARY" => TokenType.PRIMARY,
                "KEY" => TokenType.KEY,
                "UNIQUE" => TokenType.UNIQUE,
                "INT" => TokenType.INT,
                "TEXT" => TokenType.TEXT,
                _ => TokenType.IDENTIFIER
            };

            AddToken(type);
        }
    }
}
