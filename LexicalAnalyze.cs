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
    
    public static List<Token>[] Analyze(string code)
    {
        var temp = code;
        
        var separatorsRegex = new Regex(@"[{}():,;+\-*/=]");
        var keyWordsRegex = new Regex(@"\b(program|var|begin|end|switch|case|for|each|in|while|do|readln|writeln)\b");
        var keyWordsList = new List<string>()
        {
            "program", "var", 
            "begin", "end", 
            "switch", "case", "for", "each", "in", "while", "do",
            "readln", "writeln", 
            "integer", "real", "boolean", "char", "string", "const"
        };
        var identifierRegex = new Regex(@"[a-zA-Z][a-zA-Z0-9]*");
        var digitsRegex = new Regex(@"[0-9]{1,8}(?:[.,][0-9])?");
        
        
        var matches = separatorsRegex.Matches(temp);
        foreach (Match match in matches)
        {
            separators.Add(new Token(match.Value, Token.TokenType.Separator));
            temp = temp.Replace(match.Value, ""); //починить проблему с разделением integer на in и teger
        }
        
        matches = keyWordsRegex.Matches(temp);
        foreach (Match match in matches)
        {
            keyWords.Add(new Token(match.Value, Token.TokenType.KeyWord));
            temp = temp.Replace(match.Value, "");
        }

        matches = identifierRegex.Matches(temp);
        foreach (Match match in matches)
        {
            if (!keyWordsList.Contains(match.Value))
            {
                variables.Add(new Token(match.Value, Token.TokenType.Variable));
                temp = temp.Replace(match.Value, "");
            }
        }

        //temp = string.Join(" ", tempList);
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
}