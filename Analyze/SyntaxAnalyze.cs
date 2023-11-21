using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Compile_v2;

namespace SyntaxAnalyze
{
    public class SyntaxAnalyzer
    {
        public static List<Token> tokens;
        private static int currentIndex;

        public static string ParseProgram()
        {
            currentIndex = 0;
            tokens = LexicalAnalyzer.Tokens;
            MatchKeyword("program");
            MatchIdentifier();
            MatchKeyword("var");

            ParseDescription();

            MatchKeyword("begin");

            while (CurrentToken.Value != "end")
            {
                if(currentIndex == tokens.Count - 1 && CurrentToken.Value != "end")
                    MatchKeyword("end");
                ParseStatement();
                if (CurrentToken.Value == ";")
                    Match(";");
            }

            MatchKeyword("end");

            return "Синтаксический анализ успешно завершен.";
        }

        private static void ParseDescription()
        {
            while (IsIdentifier(CurrentToken.Value))
            {
                if (CurrentToken.Value == "begin") return;
                try
                {
                    ParseVariableDeclaration();
                    Match(";");
                }
                catch (Exception)
                {
                    MatchKeyword("begin");
                }
            }
        }

        private static void ParseVariableDeclaration()
        {
            MatchIdentifier();
            while (CurrentToken.Value == ",")
            {
                Match(",");
                MatchIdentifier();
            }
            Match(":");
            MatchType();
        }

        private static void ParseStatement()
        {
            switch (CurrentToken.Value)
            {
                case "switch":
                    ParseSwitchStatement();
                    break;
                case "for" when LookAhead(1).Value == "each":
                    ParseForEachStatement();
                    break;
                case "while":
                    ParseWhileStatement();
                    break;
                case "readln":
                    ParseReadlnStatement();
                    break;
                case "writeln":
                    ParseWritelnStatement();
                    break;
                default:
                    ParseExpression();
                    break;
            }
        }

        private static void ParseSwitchStatement()
        {
            MatchKeyword("switch");
            ParseExpression();
            Match("{");

            while (CurrentToken.Value != "}")
            {
                if (CurrentToken.Value == "case")
                {
                    MatchKeyword("case");
                    ParseExpression();
                    Match(":");
                    ParseBlock();
                }
            }

            Match("}");
        }

        private static void ParseForEachStatement()
        {
            MatchKeyword("for");
            MatchKeyword("each");
            MatchIdentifier();
            MatchKeyword("in");
            Match("(");
            while (CurrentToken.Value != ")")
            {
                ParseConstant();
                if (CurrentToken.Value == ",")
                {
                    Match(",");
                }
            }

            Match(")");
            ParseBlock();
        }

        private static void ParseWhileStatement()
        {
            MatchKeyword("while");
            ParseExpression();
            ParseBlock();
        }

        private static void ParseReadlnStatement()
        {
            MatchKeyword("readln");
            Match("(");
            MatchIdentifier();
            while (CurrentToken.Value == ",")
            {
                Match(",");
                MatchIdentifier();
            }

            Match(")");
        }

        private static void ParseWritelnStatement()
        {
            MatchKeyword("writeln");
            Match("(");
            ParseExpression();
            while (CurrentToken.Value == ",")
            {
                Match(",");
                ParseExpression();
            }

            Match(")");
        }
        
        private static void ParseBlock()
        {
            Match("{");
            while (CurrentToken.Value != "}")
            {
                ParseStatement();
                if (CurrentToken.Value == ";")
                    Match(";");
            }
            Match("}");
        }

        private static void ParseConstant()
        {
            MatchIdentifier();
            if (IsNumericConstant(CurrentToken.Value))
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается константа, но получено '{CurrentToken.Value}'");
            }
        }

        private static void ParseExpression()
        {
            ParseTerm();
            while (IsOperator(CurrentToken.Value))
            {
                MatchOperator(CurrentToken.Value);
                ParseTerm();
            }
        }

        private static void ParseTerm()
        {
            if (CurrentToken.Type == Token.TokenType.Variable)
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается переменная или константа, но получено '{CurrentToken.Value}'");
            }
        }
        private static bool IsNumericConstant(string value)
        {
            double result;
            return double.TryParse(value, out result);
        }
        private static bool IsOperator(string value)
        {
            return value == "+" || value == "-" || value == "*" || value == "/" || value == "=" || value == ">" || value == "<";
        }

        private static void MatchOperator(string expected)
        {
            if (IsOperator(CurrentToken.Value) && CurrentToken.Value == expected)
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается оператор '{expected}', но получено '{CurrentToken.Value}'");
            }
        }


        private static void Match(string expected)
        {
            if (CurrentToken.Value == expected)
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается '{expected}', но получено '{CurrentToken.Value}'");
            }
        }

        private static void MatchKeyword(string keyword)
        {
            Match(keyword);
        }

        private static void MatchIdentifier()
        {
            if (IsIdentifier(CurrentToken.Value))
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается идентификатор, но получено '{CurrentToken.Value}'");
            }
        }

        private static void MatchType()
        {
            if (IsType(CurrentToken.Value))
            {
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается тип, но получено '{CurrentToken.Value}'");
            }
        }

        private static void ConsumeToken()
        {
            if (currentIndex < tokens.Count - 1)
            {
                currentIndex++;
            }
        }

        private static Token CurrentToken => tokens[currentIndex];

        private static Token LookAhead(int count)
        {
            if (currentIndex + count < tokens.Count)
            {
                return tokens[currentIndex + count];
            }
            return new Token("", Token.TokenType.Unknown);
        }

        private static bool IsIdentifier(string value)
        {
            return Regex.IsMatch(value, @"[a-zA-Z][a-zA-Z0-9]*");
        }

        private static bool IsType(string value)
        {
            return value == "integer" || value == "real" || value == "boolean" || value == "char" || value == "string";
        }
    }
}