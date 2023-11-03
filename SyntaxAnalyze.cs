using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Compile_v2;

namespace SyntaxAnalyze
{
    public class SyntaxAnalyzer
    {
        private static List<Token> tokens;
        private static int currentIndex;

        public static string ParseProgram()
        {
            currentIndex = 0;
            tokens = LexicalAnalyzer.Tokens;
            // Парсим начало программы
            MatchKeyword("program");
            MatchIdentifier();
            MatchKeyword("var");

            // Парсим описание
            ParseDescription();

            MatchKeyword("begin");

            // Парсим операторы
            while (CurrentToken.Value != "end")
            {
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
                ParseVariableDeclaration();
                Match(";");
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
            // Реализуйте парсинг оператора согласно вашей грамматике
            // Пример:
            if (CurrentToken.Value == "switch")
            {
                ParseSwitchStatement();
            }
            else if (CurrentToken.Value == "for" && LookAhead(1).Value == "each")
            {
                ParseForEachStatement();
            }
            else if (CurrentToken.Value == "while")
            {
                ParseWhileStatement();
            }
            else if (CurrentToken.Value == "readln")
            {
                ParseReadlnStatement();
            }
            else if (CurrentToken.Value == "writeln")
            {
                ParseWritelnStatement();
            }
            else
            {
                ParseExpression();
            }
        }

        private static void ParseSwitchStatement()
        {
            // Реализация парсинга оператора switch
            MatchKeyword("switch");
            // Дополнительный код разбора оператора switch
            // Пример:
            ParseExpression(); // Разбор выражения для выбора ветки
            Match("{");

            while (CurrentToken.Value != "}")
            {
                if (CurrentToken.Value == "case")
                {
                    MatchKeyword("case");
                    ParseExpression(); // Разбор константы для case
                    Match(":");
                    // Разбор операторов для данной ветки case
                    ParseBlock();
                }
                else
                {
                    // Обработка других операторов внутри switch
                    // parseOtherStatement();
                }
            }

            Match("}");
        }

        private static void ParseForEachStatement()
        {
            // Реализация парсинга оператора for each
            MatchKeyword("for");
            MatchKeyword("each");
            // Дополнительный код разбора оператора for each
            // Пример:
            MatchIdentifier(); // Имя переменной
            MatchKeyword("in");
            Match("(");
            while (CurrentToken.Value != ")")
            {
                ParseConstant(); // Разбор констант для цикла for each
                if (CurrentToken.Value == ",")
                {
                    Match(",");
                }
            }

            Match(")");
            // Разбор операторов внутри цикла for each
            ParseBlock();
        }

        private static void ParseWhileStatement()
        {
            // Реализация парсинга оператора while
            MatchKeyword("while");
            // Разбор условия
            ParseExpression();
            // Разбор операторов внутри цикла while
            ParseBlock();
        }

        private static void ParseReadlnStatement()
        {
            // Реализация парсинга оператора readln
            MatchKeyword("readln");
            Match("(");
            MatchIdentifier(); // Идентификатор переменной
            while (CurrentToken.Value == ",")
            {
                Match(",");
                MatchIdentifier(); // Идентификаторы переменных
            }

            Match(")");
        }

        private static void ParseWritelnStatement()
        {
            // Реализация парсинга оператора writeln
            MatchKeyword("writeln");
            Match("(");
            ParseExpression(); // Разбор выражения для вывода
            while (CurrentToken.Value == ",")
            {
                Match(",");
                ParseExpression(); // Дополнительные выражения для вывода
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
                // Парсинг числовой константы
                ConsumeToken();
            }
            else
            {
                throw new Exception($"Ожидается константа, но получено '{CurrentToken.Value}'");
            }
        }

        private static void ParseExpression()
        {
            // Ваш код разбора выражения с учетом приоритетов и синтаксиса вашего языка
            // Пример:
            ParseTerm();
            while (IsOperator(CurrentToken.Value))
            {
                MatchOperator(CurrentToken.Value);
                ParseTerm();
            }
        }

        private static void ParseTerm()
        {
            // Разбор терма
            // Пример:
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
            // Реализация проверки на операторы в вашем языке
            // Например:
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
            // Реализация проверки идентификатора согласно вашей грамматике
            // Пример:
            return Regex.IsMatch(value, @"[a-zA-Z][a-zA-Z0-9]*");
        }

        private static bool IsType(string value)
        {
            // Реализация проверки типа согласно вашей грамматике
            // Пример:
            return value == "integer" || value == "real" || value == "boolean" || value == "char" || value == "string";
        }
    }
}