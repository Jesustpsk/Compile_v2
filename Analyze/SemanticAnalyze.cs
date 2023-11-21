using System;
using System.Collections.Generic;
using static Compile_v2.LexicalAnalyzer;

namespace Compile_v2.Analyze;

public class SemanticAnalyze
{
    private static int currentIndex;

    private static readonly Token.TokenType[] NumericTypes =
        { Token.TokenType.Integer, Token.TokenType.Real, Token.TokenType.Boolean };

    private static readonly Token.TokenType[] StringTypes = { Token.TokenType.Char, Token.TokenType.String };

    public static string AnalyzeSemantic(List<Token> tokens)
    {
        currentIndex = 0;

        try
        {
            ParseProgram(tokens);
            return "Семантический анализ успешно завершен.";
        }
        catch (Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }
    }

    private static void ParseProgram(List<Token> tokens)
    {
        MatchKeyword(tokens, "program");
        MatchIdentifier(tokens);
        MatchKeyword(tokens, "var");
        ParseDescription(tokens);
        MatchKeyword(tokens, "begin");

        while (CurrentToken(tokens).Value != "end")
        {
            ParseStatement(tokens);
            if (CurrentToken(tokens).Value == ";")
                Match(tokens, ";");
        }

        MatchKeyword(tokens, "end");
    }

    private static void ParseDescription(List<Token> tokens)
    {
        while (IsIdentifier(CurrentToken(tokens).Value) && !IsKeyWord(CurrentToken(tokens).Value))
        {
            ParseVariableDeclaration(tokens);
            Match(tokens, ";");
        }
    }

    private static void ParseVariableDeclaration(List<Token> tokens)
    {
        var identifier = CurrentToken(tokens).Value;
        MatchIdentifier(tokens);

        while (CurrentToken(tokens).Value == ",")
        {
            Match(tokens, ",");
            identifier += ',' + CurrentToken(tokens).Value;
            MatchIdentifier(tokens);
        }

        Match(tokens, ":");
        var type = CurrentToken(tokens).Value;
        MatchType(tokens);

        // Сохраняем информацию о переменных и их типах
        foreach (var id in identifier.Split(','))
        {
            if (!SymbolTable.AddVariable(id, type))
            {
                throw new Exception($"Переменная '{id}' уже была объявлена.");
            }
        }
    }

    private static void ParseStatement(List<Token> tokens)
    {
        switch (CurrentToken(tokens).Value)
        {
            case "switch":
                ParseSwitchStatement(tokens);
                break;
            case "for" when LookAhead(tokens, 1).Value == "each":
                ParseForEachStatement(tokens);
                break;
            case "while":
                ParseWhileStatement(tokens);
                break;
            case "readln":
                ParseReadlnStatement(tokens);
                break;
            case "writeln":
                ParseWritelnStatement(tokens);
                break;
            default:
                ParseExpression(tokens);
                break;
        }
    }

    private static void ParseSwitchStatement(List<Token> tokens)
    {
        MatchKeyword(tokens, "switch");
        ParseExpression(tokens);
        Match(tokens, "{");

        while (CurrentToken(tokens).Value != "}")
        {
            if (CurrentToken(tokens).Value == "case")
            {
                MatchKeyword(tokens, "case");
                ParseExpression(tokens);
                Match(tokens, ":");
                ParseBlock(tokens);
            }
        }

        Match(tokens, "}");
    }

    private static void ParseForEachStatement(List<Token> tokens)
    {
        MatchKeyword(tokens, "for");
        MatchKeyword(tokens, "each");
        MatchIdentifier(tokens);
        MatchKeyword(tokens, "in");
        Match(tokens, "(");
        while (CurrentToken(tokens).Value != ")")
        {
            ParseConstant(tokens);
            if (CurrentToken(tokens).Value == ",")
            {
                Match(tokens, ",");
            }
        }

        Match(tokens, ")");
        ParseBlock(tokens);
    }

    private static void ParseWhileStatement(List<Token> tokens)
    {
        MatchKeyword(tokens, "while");
        ParseExpression(tokens);
        ParseBlock(tokens);
    }

    private static void ParseReadlnStatement(List<Token> tokens)
    {
        MatchKeyword(tokens, "readln");
        Match(tokens, "(");
        MatchIdentifier(tokens);
        while (CurrentToken(tokens).Value == ",")
        {
            Match(tokens, ",");
            MatchIdentifier(tokens);
        }

        Match(tokens, ")");
    }

    private static void ParseWritelnStatement(List<Token> tokens)
    {
        MatchKeyword(tokens, "writeln");
        Match(tokens, "(");
        ParseExpression(tokens);
        while (CurrentToken(tokens).Value == ",")
        {
            Match(tokens, ",");
            ParseExpression(tokens);
        }

        Match(tokens, ")");
    }

    private static void ParseBlock(List<Token> tokens)
    {
        Match(tokens, "{");
        while (CurrentToken(tokens).Value != "}")
        {
            ParseStatement(tokens);
            if (CurrentToken(tokens).Value == ";")
                Match(tokens, ";");
        }

        Match(tokens, "}");
    }

    private static void ParseConstant(List<Token> tokens)
    {
        MatchIdentifier(tokens);
        if (!IsNumericConstant(CurrentToken(tokens).Value))
        {
            throw new Exception($"Ожидается числовая константа, но получено '{CurrentToken(tokens).Value}'");
        }

        ConsumeToken(tokens);
    }

    private static void ParseExpression(List<Token> tokens)
    {
        ParseTerm(tokens);
        while (IsOperator(CurrentToken(tokens).Value))
        {
            MatchOperator(tokens, CurrentToken(tokens).Value);
            ParseTerm(tokens);
        }
    }

    private static void ParseTerm(List<Token> tokens)
    {
        if (CurrentToken(tokens).Type == Token.TokenType.Variable)
        {
            var variableType = IsNumericConstant(CurrentToken(tokens).Value) ? CurrentToken(tokens).Value : 
                IsKeyWord(CurrentToken(tokens).Value) ? CurrentToken(tokens).Value : 
                IsString(CurrentToken(tokens).Value) ? CurrentToken(tokens).Value : SymbolTable.GetVariableType(CurrentToken(tokens).Value);
            ConsumeToken(tokens);

            if (variableType == null)
            {
                throw new Exception($"Использование необъявленной переменной '{CurrentToken(tokens).Value}'");
            }
        }
        else
        {
            throw new Exception($"Ожидается переменная, но получено '{CurrentToken(tokens).Value}'");
        }
    }

    private static bool IsNumericConstant(string value)
    {
        double result;
        return double.TryParse(value.Replace('.', ','), out result);
    }

    private static bool IsOperator(string value)
    {
        return value == "+" || value == "-" || value == "*" || value == "/" || value == "=" || value == ">" ||
               value == "<";
    }

    private static void MatchOperator(List<Token> tokens, string expected)
    {
        if (IsOperator(CurrentToken(tokens).Value) && CurrentToken(tokens).Value == expected)
        {
            ConsumeToken(tokens);
        }
        else
        {
            throw new Exception($"Ожидается оператор '{expected}', но получено '{CurrentToken(tokens).Value}'");
        }
    }

    private static void Match(List<Token> tokens, string expected)
    {
        if (CurrentToken(tokens).Value == expected)
        {
            ConsumeToken(tokens);
        }
        else
        {
            throw new Exception($"Ожидается '{expected}', но получено '{CurrentToken(tokens).Value}'");
        }
    }

    private static void MatchKeyword(List<Token> tokens, string keyword)
    {
        Match(tokens, keyword);
    }

    private static void MatchIdentifier(List<Token> tokens)
    {
        if (IsIdentifier(CurrentToken(tokens).Value))
        {
            ConsumeToken(tokens);
        }
        else
        {
            throw new Exception($"Ожидается идентификатор, но получено '{CurrentToken(tokens).Value}'");
        }
    }

    private static void MatchType(List<Token> tokens)
    {
        if (IsType(CurrentToken(tokens).Value))
        {
            ConsumeToken(tokens);
        }
        else
        {
            throw new Exception($"Ожидается тип, но получено '{CurrentToken(tokens).Value}'");
        }
    }

    private static void ConsumeToken(List<Token> tokens)
    {
        if (currentIndex < tokens.Count - 1)
        {
            currentIndex++;
        }
    }

    private static Token CurrentToken(List<Token> tokens)
    {
        return tokens[currentIndex];
    }

    private static Token LookAhead(List<Token> tokens, int count)
    {
        if (currentIndex + count < tokens.Count)
        {
            return tokens[currentIndex + count];
        }

        return new Token("", Token.TokenType.Unknown);
    }

    private static bool IsIdentifier(string value)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"[a-zA-Z][a-zA-Z0-9]*");
    }

    private static bool IsString(string value)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(value, @"[a-zA-Z][a-zA-Z] *");
    }

    private static bool IsType(string value)
    {
        return value == "integer" || value == "real" || value == "boolean" || value == "char" || value == "string";
    }
    
    private static bool IsKeyWord(string value)
    {
        return value == "program" || value == "var" || value == "begin" || value == "switch" || value == "case" || 
               value == "for" || value == "each" || value == "in" || value == "while" || value == "do" || 
               value == "readln" || value == "writeln" || value == "integer" || value == "real" || value == "boolean" || 
               value == "char" || value == "string" || value == "true" || value == "false";
    }
}