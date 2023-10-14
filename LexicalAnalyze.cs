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
        return double.TryParse(word, out double value);
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
    public static List<string> Tokenize(string input)
    {
        var allList = separatorsList.Concat(keyWordsList).ToArray();
        var lexemes = new List<string>();
            
        var words = input.Split(new []{"\r\n", " "}.Concat(separatorsList).ToArray(), StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (IsKeyWord(word))
            {
                lexemes.Add("KEY WORD: " + word);
            }
            else if (IsSeparator(word))
            {
                lexemes.Add("SEPARATOR: " + word);
            }
            else if (IsIdentifier(word))
            {
                lexemes.Add("IDENTIFIER: " + word);
            }
            else if (IsInteger(word))
            {
                lexemes.Add("INTEGER: " + word);
            }
            else if (IsReal(word))
            {
                lexemes.Add("REAL: " + word);
            }
            else if (IsBoolean(word))
            {
                lexemes.Add("BOOLEAN: " + word);
            }
            else if (IsChar(word))
            {
                lexemes.Add("CHAR: " + word);
            }
            else if (IsString(word))
            {
                lexemes.Add("STRING: " + word);
            }
            else
            {
                lexemes.Add("UNKNOWN: " + word);
            }
        }

        return lexemes;
    }
}