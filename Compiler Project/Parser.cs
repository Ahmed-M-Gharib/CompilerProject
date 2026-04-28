using System;
using System.Collections.Generic;
using TinyLanguageScanner;

namespace TinyScanner
{
    public class TinyParser
    {
        private List<Token> tokens;
        private int index = 0;
        private Token current => index < tokens.Count ? tokens[index] : null;

        public TinyParser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private void Match(TokenType expected)
        {
            if (current != null && current.Type == expected)
                index++;
            else
                throw new Exception($"Syntax Error at line {current?.Line}: Expected '{expected}' but found '{current?.Value}' ({current?.Type})");
        }

        private void MatchValue(string val)
        {
            if (current != null && current.Value == val)
                index++;
            else
                throw new Exception($"Syntax Error at line {current?.Line}: Expected '{val}' but found '{current?.Value}'");
        }

        // Program → Function* MainFunction
        public void ParseProgram()
        {
            // skip comments
            SkipComments();

            while (index < tokens.Count)
            {
                SkipComments();
                if (index >= tokens.Count) break;

                // peek: datatype identifier ( ... ) { => function
                // datatype "main" ( ) { => main function
                if (IsDatatype(current))
                {
                    int saved = index;
                    index++; // skip datatype
                    SkipComments();
                    if (current?.Type == TokenType.IDENTIFIER || current?.Type == TokenType.MAIN)
                    {
                        string name = current.Value;
                        index++; // skip name
                        SkipComments();
                        if (current?.Type == TokenType.LEFT_PAREN)
                        {
                            index = saved; // restore and parse properly
                            if (name == "main")
                                ParseMainFunction();
                            else
                                ParseFunctionStatement();
                            continue;
                        }
                    }
                    index = saved;
                }
                break;
            }

            SkipComments();
            if (index < tokens.Count)
                throw new Exception($"Syntax Error: Unexpected token '{current?.Value}' at line {current?.Line}");
        }

        // FunctionStatement → FunctionDeclaration FunctionBody
        private void ParseFunctionStatement()
        {
            ParseFunctionDeclaration();
            SkipComments();
            ParseFunctionBody();
        }

        // FunctionDeclaration → Datatype FunctionName ( Params )
        private void ParseFunctionDeclaration()
        {
            ParseDatatype();
            SkipComments();
            if (current?.Type != TokenType.IDENTIFIER)
                throw new Exception($"Syntax Error at line {current?.Line}: Expected function name");
            index++; // function name
            SkipComments();
            Match(TokenType.LEFT_PAREN);
            SkipComments();
            ParseParams();
            SkipComments();
            Match(TokenType.RIGHT_PAREN);
        }

        // Params → (Datatype Identifier (, Datatype Identifier)*)?
        private void ParseParams()
        {
            if (current?.Type == TokenType.RIGHT_PAREN) return;
            if (!IsDatatype(current)) return;
            ParseDatatype();
            SkipComments();
            Match(TokenType.IDENTIFIER);
            SkipComments();
            while (current?.Type == TokenType.COMMA)
            {
                index++; // comma
                SkipComments();
                ParseDatatype();
                SkipComments();
                Match(TokenType.IDENTIFIER);
                SkipComments();
            }
        }

        // FunctionBody → { Statements ReturnStatement }
        private void ParseFunctionBody()
        {
            Match(TokenType.LEFT_BRACE);
            SkipComments();
            ParseStatements();
            SkipComments();
            ParseReturnStatement();
            SkipComments();
            Match(TokenType.RIGHT_BRACE);
        }

        // MainFunction → Datatype main () FunctionBody
        private void ParseMainFunction()
        {
            ParseDatatype();
            SkipComments();
            Match(TokenType.MAIN);
            SkipComments();
            Match(TokenType.LEFT_PAREN);
            SkipComments();
            Match(TokenType.RIGHT_PAREN);
            SkipComments();
            ParseFunctionBody();
        }

        // Statements → Statement*
        private void ParseStatements()
        {
            SkipComments();
            while (index < tokens.Count && !IsStatementsEnd())
            {
                SkipComments();
                if (index >= tokens.Count || IsStatementsEnd()) break;
                ParseStatement();
                SkipComments();
            }
        }

        private bool IsStatementsEnd()
        {
            if (current == null) return true;
            return current.Type == TokenType.RIGHT_BRACE
                || current.Type == TokenType.RETURN
                || current.Type == TokenType.UNTIL
                || current.Type == TokenType.ELSE
                || current.Type == TokenType.ELSEIF
                || (current.Value == "end");
        }

        // Statement → Declaration | Assignment | Write | Read | If | Repeat
        private void ParseStatement()
        {
            SkipComments();
            if (current == null) return;

            if (IsDatatype(current))
            {
                // could be declaration
                ParseDeclarationStatement();
            }
            else if (current.Type == TokenType.IDENTIFIER)
            {
                ParseAssignmentStatement();
            }
            else if (current.Type == TokenType.WRITE)
            {
                ParseWriteStatement();
            }
            else if (current.Type == TokenType.READ)
            {
                ParseReadStatement();
            }
            else if (current.Type == TokenType.IF)
            {
                ParseIfStatement();
            }
            else if (current.Type == TokenType.REPEAT)
            {
                ParseRepeatStatement();
            }
            else
            {
                throw new Exception($"Syntax Error at line {current.Line}: Unexpected token '{current.Value}' ({current.Type})");
            }
        }

        // Declaration → Datatype Identifier (:= Expression)? (, Identifier (:= Expression)?)* ;
        private void ParseDeclarationStatement()
        {
            ParseDatatype();
            SkipComments();
            if (current?.Type != TokenType.IDENTIFIER)
                throw new Exception($"Syntax Error at line {current?.Line}: Expected identifier after datatype");
            index++;
            SkipComments();
            if (current?.Type == TokenType.ASSIGN_OP)
            {
                index++;
                SkipComments();
                ParseExpression();
                SkipComments();
            }
            while (current?.Type == TokenType.COMMA)
            {
                index++;
                SkipComments();
                Match(TokenType.IDENTIFIER);
                SkipComments();
                if (current?.Type == TokenType.ASSIGN_OP)
                {
                    index++;
                    SkipComments();
                    ParseExpression();
                    SkipComments();
                }
            }
            Match(TokenType.SEMICOLON);
        }

        // Assignment → Identifier := Expression ;
        private void ParseAssignmentStatement()
        {
            Match(TokenType.IDENTIFIER);
            SkipComments();
            Match(TokenType.ASSIGN_OP);
            SkipComments();
            ParseExpression();
            SkipComments();
            Match(TokenType.SEMICOLON);
        }

        // Write → write (Expression | endl) ;
        private void ParseWriteStatement()
        {
            Match(TokenType.WRITE);
            SkipComments();
            if (current?.Type == TokenType.ENDL)
                index++;
            else
                ParseExpression();
            SkipComments();
            Match(TokenType.SEMICOLON);
        }

        // Read → read Identifier ;
        private void ParseReadStatement()
        {
            Match(TokenType.READ);
            SkipComments();
            Match(TokenType.IDENTIFIER);
            SkipComments();
            Match(TokenType.SEMICOLON);
        }

        // Return → return Expression ;
        private void ParseReturnStatement()
        {
            Match(TokenType.RETURN);
            SkipComments();
            ParseExpression();
            SkipComments();
            Match(TokenType.SEMICOLON);
        }

        // If → if ConditionStatement then Statements (elseif ... | else ... | end)
        private void ParseIfStatement()
        {
            Match(TokenType.IF);
            SkipComments();
            ParseConditionStatement();
            SkipComments();
            Match(TokenType.THEN);
            SkipComments();
            ParseStatements();
            SkipComments();

            while (current?.Type == TokenType.ELSEIF)
            {
                index++;
                SkipComments();
                ParseConditionStatement();
                SkipComments();
                Match(TokenType.THEN);
                SkipComments();
                ParseStatements();
                SkipComments();
            }

            if (current?.Type == TokenType.ELSE)
            {
                index++;
                SkipComments();
                ParseStatements();
                SkipComments();
            }

            if (current?.Value == "end")
                index++;
            else
                throw new Exception($"Syntax Error at line {current?.Line}: Expected 'end' to close if statement");
        }

        // Repeat → repeat Statements until ConditionStatement
        private void ParseRepeatStatement()
        {
            Match(TokenType.REPEAT);
            SkipComments();
            ParseStatements();
            SkipComments();
            Match(TokenType.UNTIL);
            SkipComments();
            ParseConditionStatement();
        }

        // ConditionStatement → Condition ((&& | ||) Condition)*
        private void ParseConditionStatement()
        {
            ParseCondition();
            SkipComments();
            while (current?.Type == TokenType.BOOLEAN_OP)
            {
                index++;
                SkipComments();
                ParseCondition();
                SkipComments();
            }
        }

        // Condition → Identifier CondOp Term
        private void ParseCondition()
        {
            if (current?.Type != TokenType.IDENTIFIER)
                throw new Exception($"Syntax Error at line {current?.Line}: Expected identifier in condition, found '{current?.Value}'");
            index++;
            SkipComments();
            if (current?.Type != TokenType.CONDITION_OP)
                throw new Exception($"Syntax Error at line {current?.Line}: Expected condition operator, found '{current?.Value}'");
            index++;
            SkipComments();
            ParseTerm();
        }

        // Expression → Term ((+|-) Term)*
        private void ParseExpression()
        {
            if (current?.Type == TokenType.STRING)
            {
                index++;
                return;
            }
            ParseTerm();
            SkipComments();
            while (current?.Type == TokenType.ARITHMETIC_OP &&
                   (current.Value == "+" || current.Value == "-"))
            {
                index++;
                SkipComments();
                ParseTerm();
                SkipComments();
            }
        }

        // Term → Factor ((*|/) Factor)*
        private void ParseTerm()
        {
            ParseFactor();
            SkipComments();
            while (current?.Type == TokenType.ARITHMETIC_OP &&
                   (current.Value == "*" || current.Value == "/"))
            {
                index++;
                SkipComments();
                ParseFactor();
                SkipComments();
            }
        }

        // Factor → Number | Identifier (( Args ))? | ( Expression )
        private void ParseFactor()
        {
            if (current?.Type == TokenType.NUMBER)
            {
                index++;
            }
            else if (current?.Type == TokenType.IDENTIFIER)
            {
                index++;
                SkipComments();
                // function call
                if (current?.Type == TokenType.LEFT_PAREN)
                {
                    index++;
                    SkipComments();
                    if (current?.Type != TokenType.RIGHT_PAREN)
                    {
                        ParseExpression();
                        SkipComments();
                        while (current?.Type == TokenType.COMMA)
                        {
                            index++;
                            SkipComments();
                            ParseExpression();
                            SkipComments();
                        }
                    }
                    Match(TokenType.RIGHT_PAREN);
                }
            }
            else if (current?.Type == TokenType.LEFT_PAREN)
            {
                index++;
                SkipComments();
                ParseExpression();
                SkipComments();
                Match(TokenType.RIGHT_PAREN);
            }
            else
            {
                throw new Exception($"Syntax Error at line {current?.Line}: Expected identifier or number, found '{current?.Value}'");
            }
        }

        private void ParseDatatype()
        {
            if (IsDatatype(current))
                index++;
            else
                throw new Exception($"Syntax Error at line {current?.Line}: Expected datatype (int/float/string), found '{current?.Value}'");
        }

        private bool IsDatatype(Token t)
        {
            if (t == null) return false;
            return t.Type == TokenType.INT || t.Type == TokenType.FLOAT || t.Type == TokenType.STRING_KW;
        }

        private void SkipComments()
        {
            while (index < tokens.Count && tokens[index].Type == TokenType.COMMENT)
                index++;
        }
    }
}
