using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compile_v2
{
    public class LexicalAnalyzer
    {
        private static List<Token> separators = new();
        private static List<Token> keywords = new();
        private static List<Token> variables = new();
        public static List<Token> Tokens = new();

        private static readonly Regex separatorsRegex = new Regex(@"[{}():,;+\-*/=]");
        private static readonly Regex keywordsRegex = new Regex(@"\b(program|var|begin|end|switch|case|for|each|in|while|do|readln|writeln|integer|real|boolean|char|string)\b");
        private static readonly Regex identifierRegex = new Regex(@"[a-zA-Z][a-zA-Z0-9]*");
        private static readonly Regex digitsRegex = new Regex(@"[0-9]{1,8}(?:\.[0-9])?");

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

        public static List<Token>[] LexicalAnalyze(string code)
        {
            var temp = code;

            var matches = separatorsRegex.Matches(temp);
            foreach (Match match in matches)
            {
                separators.Add(new Token(match.Value, Token.TokenType.Separator));
                temp = temp.Replace(match.Value, "");
            }

            matches = keywordsRegex.Matches(temp);
            foreach (Match match in matches)
            {
                keywords.Add(new Token(match.Value, Token.TokenType.KeyWord));
                temp = Regex.Replace(temp, @"\b" + Regex.Escape(match.Value) + @"\b", " ");
            }

            matches = identifierRegex.Matches(temp);
            foreach (Match match in matches)
            {
                variables.Add(new Token(match.Value, Token.TokenType.Variable));
                temp = temp.Replace(match.Value, "");
            }

            matches = digitsRegex.Matches(temp);
            foreach (Match match in matches)
            {
                variables.Add(new Token(match.Value, Token.TokenType.Variable));
            }

            return new List<Token>[] { RemoveDuplicates(separators), RemoveDuplicates(keywords), RemoveDuplicates(variables) };
        }

        private static List<Token> RemoveDuplicates(List<Token> list)
        {
            var distinctList = new List<Token> { list[0] };
            for (var i = 1; i < list.Count; i++)
            {
                if (!distinctList.Exists(t => t.Value == list[i].Value))
                    distinctList.Add(list[i]);
            }
            return distinctList;
        }

        public static void Tokenize(string input)
        {
            Tokens.Clear();
            var separators = new HashSet<string> { "{", "}", "(", ")", ":", ",", ";", "+", "-", "*", "/", "=" };

            var currentToken = "";

            foreach (var character in input)
            {
                if (char.IsWhiteSpace(character))
                {
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
            if (keywords.Exists(t => t.Value == word))
            {
                Tokens.Add(new Token(word, Token.TokenType.KeyWord));
            }
            else if (identifierRegex.IsMatch(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else if (digitsRegex.IsMatch(word))
            {
                Tokens.Add(new Token(word, Token.TokenType.Variable));
            }
            else
            {
                Tokens.Add(new Token(word, Token.TokenType.Unknown));
            }
        }

        public static string[] TokensToStringArray()
        {
            var output = new string[Tokens.Count];
            for (var i = 0; i < Tokens.Count; i++)
            {
                output[i] = Tokens[i].Value + " => " + Tokens[i].Type;
            }

            return output;
        }
    }
}