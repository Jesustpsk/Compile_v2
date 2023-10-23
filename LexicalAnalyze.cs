using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Compile_v2;

public class LexicalAnalyze 
{
    private static List<Token> separators = new();
    private static List<Token> keyWords = new();
    private static List<Token> variables = new();
    public static List<Token> Tokens = new();
    
    private static Regex separatorsRegex = new (@"[{}():,;+\-*/=]");
    private static Regex keyWordsRegex = new (@"\b(program|var|begin|end|switch|case|for|each|in|while|do|readln|writeln)\b");
    private static Regex identifierRegex = new (@"[a-zA-Z][a-zA-Z0-9]*");
    private static Regex digitsRegex = new (@"[0-9]{1,8}(?:[.][0-9])?");
    
    private static List<string> keyWordsList = new()
    {
        "program", "var", 
        "begin", "end", 
        "switch", "case", "for", "each", "in", "while", "do",
        "readln", "writeln", 
        "integer", "real", "boolean", "char", "string", "const"
    };
    private static List<string> separatorsList = new()
    {
        "{", "}","(", ")",":",",",";","+","\\","-","*","/","="
    };
    
    private static bool IsIdentifier(string word)
    {
        if (string.IsNullOrEmpty(word))
        {
            return false;
        }
        if (keyWordsList.Contains(word))
        {
            return false;
        }
        if (!char.IsLetter(word[0]))
        {
            return false;
        }
        for (var i = 1; i < word.Length; i++)
        {
            if (!char.IsLetterOrDigit(word[i]))
            {
                return false;
            }
        }
        return true;
    }
    private static bool IsKeyWord(string word)
    {
        return keyWordsList.Contains(word);
    }
    private static bool IsSeparator(string word)
    {
        return separatorsList.Contains(word);
    }
    private static bool IsInteger(string word)
    {
        return int.TryParse(word, out int value);
    }
    private static bool IsReal(string word)
    {
        return double.TryParse(word.Replace('.',','), out double value);
    }
    private static bool IsBoolean(string word)
    {
        return word is "true" or "false";
    }
    private static bool IsChar(string word)
    {
        return word[0] == '\'' && word[^1] == '\'' && word.Length == 3;
    }
    private static bool IsString(string word)
    {
        return word[0] == '\"' && word[^1] == '\"' && word.Length >= 3;
    }
    
    public static string RemoveComments(string input)
    {
        var startIndex = input.IndexOf("/*", StringComparison.Ordinal);
        var endIndex = input.IndexOf("*/", StringComparison.Ordinal);

        while (startIndex >= 0 && endIndex > startIndex)
        {
            input = input.Remove(startIndex, endIndex - startIndex + 2);
            startIndex = input.IndexOf("/*", StringComparison.Ordinal);
            endIndex = input.IndexOf("*/", StringComparison.Ordinal);
        }

        return input;
    }
    private static List<Token> SDistinct(IReadOnlyList<Token> list)
    {
        var lOut = new List<Token> { list[0] };
        for (var i = 1; i < list.Count; i++)
        {
            if(lOut.Count(t => t.Value != list[i].Value) == lOut.Count)
                lOut.Add(list[i]);
        }
        return lOut;
    }
    public static List<Token>[] LexAnalyze(string code)
    {
        var temp = code;
        
        var matches = separatorsRegex.Matches(temp);
        foreach (Match match in matches)
        {
            separators.Add(new Token(match.Value, Token.TokenType.Separator));
            temp = temp.Replace(match.Value, "");
        }
        
        matches = keyWordsRegex.Matches(temp);
        foreach (Match match in matches)
        {
            keyWords.Add(new Token(match.Value, Token.TokenType.KeyWord));
            temp = String.Join(" ", temp.Split(new[] { " ", "\r\n" }, 
                StringSplitOptions.RemoveEmptyEntries).ToList().Where(x => x != match.Value).ToList());
        }

        matches = identifierRegex.Matches(temp);
        foreach (Match match in matches)
        {
            if (keyWordsList.Contains(match.Value)) continue;
            variables.Add(new Token(match.Value, Token.TokenType.Variable));
            temp = temp.Replace(match.Value, "");
        }

        matches = digitsRegex.Matches(temp);
        foreach (Match match in matches)
        {
            variables.Add(new Token(match.Value, Token.TokenType.Variable));
        }
        var sep = SDistinct(separators);
        var kw = SDistinct(keyWords);
        var v = SDistinct(variables);
        return new[] {sep, kw, v};
    }
    /*public static void Tokenize(string input)
    {
        Tokens.Clear();
        var words = input.Split(new []{"\r\n", " "}.Concat(separatorsList).ToArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (IsKeyWord(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.KeyWord));
            }
            else if (IsSeparator(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Separator));
            }
            else if (IsIdentifier(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (IsInteger(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (IsReal(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (IsBoolean(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (IsChar(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (IsString(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
        }
    }*/
    public static void Tokenize(string input)
    {
        Tokens.Clear();
        var separators = new[] { "{", "}", "(", ")", ":", ",", ";", "+", "\\", "-", "*", "/", "=" };
    
        var currentToken = "";
    
        foreach (var character in input)
        {
            if (char.IsWhiteSpace(character))
            {
                // Пропускаем пробелы и символы новой строки
                if (!string.IsNullOrEmpty(currentToken))
                {
                    AddToken(currentToken);
                    currentToken = "";
                }
            }
            else if (separators.Contains(character.ToString()))
            {
                if (!string.IsNullOrEmpty(currentToken))
                {
                    AddToken(currentToken);
                    currentToken = "";
                }
            
                Tokens.Add(new Token(character.ToString(), Token.TokenType.Separator));
            }
            else
            {
                currentToken += character;
            }
        }
    
        if (!string.IsNullOrEmpty(currentToken))
        {
            AddToken(currentToken);
        }
    }

    private static void AddToken(string word)
    {
        if (IsKeyWord(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.KeyWord));
        }
        else if (IsIdentifier(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else if (IsInteger(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else if (IsReal(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else if (IsBoolean(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else if (IsChar(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else if (IsString(word))
        {
            Tokens.Add(new Token(word, Token.TokenType.Variable));
        }
        else
        {
            Tokens.Add(new Token(word, Token.TokenType.Unknown));
        }
    }
    public static string[] Token2String()
    {
        var output = new string[Tokens.Count];
        for (var i = 0; i < Tokens.Count; i++)
        {
            output[i] = Tokens[i].Value + " => " + Tokens[i].Type;
        }

        return output;
    }
}