using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TinyLanguageScanner
{
    public enum TokenType
    {
        NUMBER,
        STRING,

        INT, FLOAT, STRING_KW, READ, WRITE, REPEAT, UNTIL,
        IF, ELSEIF, ELSE, THEN, RETURN, ENDL, MAIN,

        IDENTIFIER,

        ARITHMETIC_OP,
        ASSIGN_OP,
        CONDITION_OP,
        BOOLEAN_OP,

        SEMICOLON,
        COMMA,
        LEFT_PAREN,
        RIGHT_PAREN,
        LEFT_BRACE,
        RIGHT_BRACE,

        COMMENT,
        UNKNOWN
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }

        public Token(TokenType type, string value, int line)
        {
            Type = type;
            Value = value;
            Line = line;
        }

        public override string ToString()
        {
            return $"[Line {Line,3}]  {Type,-18}  \"{Value}\"";
        }
    }

    public class Scanner
    {
        private static readonly List<(TokenType Type, Regex Pattern)> TokenPatterns = new()
        {
            (TokenType.UNKNOWN,       new Regex(@"^\s+",            RegexOptions.Compiled)),
            (TokenType.COMMENT,       new Regex(@"^/\*[\s\S]*?\*/", RegexOptions.Compiled)),
            (TokenType.STRING,        new Regex(@"^""[^""]*""",     RegexOptions.Compiled)),
            (TokenType.CONDITION_OP,  new Regex(@"^<>",             RegexOptions.Compiled)),
            (TokenType.BOOLEAN_OP,    new Regex(@"^(&&|\|\|)",      RegexOptions.Compiled)),
            (TokenType.ASSIGN_OP,     new Regex(@"^:=",             RegexOptions.Compiled)),
            (TokenType.CONDITION_OP,  new Regex(@"^[<>=]",          RegexOptions.Compiled)),
            (TokenType.ARITHMETIC_OP, new Regex(@"^[+\-*/]",        RegexOptions.Compiled)),
            (TokenType.SEMICOLON,     new Regex(@"^;",              RegexOptions.Compiled)),
            (TokenType.COMMA,         new Regex(@"^,",              RegexOptions.Compiled)),
            (TokenType.LEFT_PAREN,    new Regex(@"^\(",             RegexOptions.Compiled)),
            (TokenType.RIGHT_PAREN,   new Regex(@"^\)",             RegexOptions.Compiled)),
            (TokenType.LEFT_BRACE,    new Regex(@"^\{",             RegexOptions.Compiled)),
            (TokenType.RIGHT_BRACE,   new Regex(@"^\}",             RegexOptions.Compiled)),
            (TokenType.NUMBER,        new Regex(@"^\d+(\.\d+)?",    RegexOptions.Compiled)),
            (TokenType.IDENTIFIER,    new Regex(@"^[a-zA-Z][a-zA-Z0-9]*", RegexOptions.Compiled)),
        };

        private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.Ordinal)
        {
            {"int",    TokenType.INT},
            {"float",  TokenType.FLOAT},
            {"string", TokenType.STRING_KW},
            {"read",   TokenType.READ},
            {"write",  TokenType.WRITE},
            {"repeat", TokenType.REPEAT},
            {"until",  TokenType.UNTIL},
            {"if",     TokenType.IF},
            {"elseif", TokenType.ELSEIF},
            {"else",   TokenType.ELSE},
            {"then",   TokenType.THEN},
            {"return", TokenType.RETURN},
            {"endl",   TokenType.ENDL},
            {"main",   TokenType.MAIN},
        };

        public List<Token> Tokenize(string source)
        {
            var tokens = new List<Token>();
            int pos = 0;
            int line = 1;

            while (pos < source.Length)
            {
                string remaining = source[pos..];
                bool matched = false;

                foreach (var (type, pattern) in TokenPatterns)
                {
                    var m = pattern.Match(remaining);
                    if (!m.Success) continue;

                    string value = m.Value;

                    int newlines = 0;
                    foreach (char c in value)
                        if (c == '\n') newlines++;

                    if (type == TokenType.UNKNOWN)
                    {
                        line += newlines;
                        pos += value.Length;
                        matched = true;
                        break;
                    }

                    if (type == TokenType.COMMENT)
                    {
                        tokens.Add(new Token(TokenType.COMMENT, value, line));
                        line += newlines;
                        pos += value.Length;
                        matched = true;
                        break;
                    }

                    if (type == TokenType.IDENTIFIER)
                    {
                        if (Keywords.TryGetValue(value, out TokenType kwType))
                            tokens.Add(new Token(kwType, value, line));
                        else
                            tokens.Add(new Token(TokenType.IDENTIFIER, value, line));

                        pos += value.Length;
                        matched = true;
                        break;
                    }

                    tokens.Add(new Token(type, value, line));
                    line += newlines;
                    pos += value.Length;
                    matched = true;
                    break;
                }

                if (!matched)
                {
                    tokens.Add(new Token(TokenType.UNKNOWN, source[pos].ToString(), line));
                    pos++;
                }
            }

            return tokens;
        }
    }
}