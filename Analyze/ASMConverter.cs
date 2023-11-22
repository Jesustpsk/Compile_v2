using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compile_v2.Analyze;

public class ASMConverter
{
    private static string _asmText = "";
    private static int _currentIndex = 0;
    public static List<Token> _tokens;
    private static Token CurrentToken => _tokens[_currentIndex];
    public static void ConvertProgram()
    {
        _tokens = SyntaxAnalyze.SyntaxAnalyzer.tokens;
        ConvertDataStatement();
        MatchKeyword("begin");
        _asmText += "selection .text\nglobal _start\n";
        ConvertProgramStatement();
    }

    private static void ConvertDataStatement()
    {
        MatchKeyword("program");
        MatchIdentifier();
        MatchKeyword("var");
        _asmText += "section .data\n";
        ParseDescription();
    }

    private static void ConvertProgramStatement()
    {
        while (CurrentToken.Value != "end")
        {
            if(_currentIndex == _tokens.Count - 1 && CurrentToken.Value != "end")
                MatchKeyword("end");
            ParseStatement();
            if (CurrentToken.Value == ";")
                Match(";");
        }

        MatchKeyword("end");
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
        var token = GetToken();
        _asmText += token.Value + " " + GetType(token) + '\n';
        while (CurrentToken.Value == ",")
        {
            Match(",");
            MatchIdentifier();
            token = GetToken();
            _asmText += token.Value + " " + GetType(token) + '\n';
        }
        Match(":");
        MatchType();
    }
    
    private static void ParseStatement()
    {
        switch (CurrentToken.Value)
        {
            /*case "switch":
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
                break;*/
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
    private static void MatchKeyword(string keyword)
    {
        Match(keyword);
    }
    private static void Match(string expected)
    {
        if (CurrentToken.Value == expected)
        {
            ConsumeToken();
        }
        else
        {
            throw new Exception($"Ошибка компиляции.");
        }
    }
    
    
    
    private static bool IsType(string value)
    {
        return value == "integer" || value == "real" || value == "boolean" || value == "char" || value == "string";
    }
    private static bool IsIdentifier(string value)
    {
        return Regex.IsMatch(value, @"[a-zA-Z][a-zA-Z0-9]*");
    }
    
    private static void ConsumeToken()
    {
        if (_currentIndex < _tokens.Count - 1)
        {
            _currentIndex++;
        }
    }

    private static Token GetToken()
    {
        return _tokens[_currentIndex - 1];
    }

    private static string GetType(Token token)
    {
        switch (token.Type)
        {
            case Token.TokenType.Integer:
                return "dd";
            case Token.TokenType.Real:
                return "dq";
            case Token.TokenType.Boolean:
            case Token.TokenType.Char:
            case Token.TokenType.String:
                return "db"; // db 255 dup(0)
        }
        throw new Exception($"Ошибка компиляции.");
    }
}