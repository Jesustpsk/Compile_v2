using System;
using System.Collections.Generic;
using System.Windows;

namespace Compile_v2;

public class SyntaxAnalyze
{
    private static int currentIndex = 0;
    private static string analyze;

    public static string AnalyzeProgram()
    {
        analyze = "";
        if (Match("program"))
        {
            if (MatchIdentifier())
            {
                if (Match("var"))
                {
                    while (!Match("begin"))
                    {
                        AnalyzeDescription();
                    }
                    currentIndex--;
                    if (Match("begin"))
                    {
                        while (!Match("end."))
                        {
                            AnalyzeStatement();
                        }

                        currentIndex--;
                        if (Match("end."))
                        {
                            analyze += "Program syntax is correct.\n";
                        }
                        else
                        {
                            analyze += "Expected 'end.'.\n";
                        }
                    }
                    else
                    {
                        analyze += "Expected 'begin'.\n";
                    }
                }
                else
                {
                    analyze += "Expected 'var'.\n";
                }
            }
            else
            {
                analyze += "Expected identifier.\n";
            }
        }
        else
        {
            analyze += "Expected 'program'.\n";
        }

        return analyze;
    }

    private static void AnalyzeDescription()
    {
        if (MatchIdentifier())
        {
            while (Match(","))
            {
                if (MatchIdentifier()) continue;
                analyze += "Expected identifier.\n";
                break;
            }

            if (Match(":"))
            {
                if (MatchType())
                {
                    if (Match(";"))
                    {
                        return;
                    }
                }
            }
        }
        analyze += "Invalid variable declaration.\n";
    }

    private static void AnalyzeStatement()
    {
        if (Match("switch"))
        {
            if (Match("("))
            {
                while (!Match("{"))
                    MatchExpression();
                currentIndex--;
                if (Match("{"))
                {
                    while (Match("case"))
                    {
                        if (!MatchConstant()) continue;
                        if (Match(":"))
                        {
                            AnalyzeStatement();
                        }
                    }

                    if (Match("}"))
                    {
                        return;
                    }
                }
            }
            analyze += "Invalid switch statement.\n";
        }
        else if (Match("for"))
        {
            if (Match("each") && MatchIdentifier() && Match("in") && Match("("))
            {
                if (MatchConstant())
                {
                    while (Match(",") && MatchConstant())
                    {
                    }

                    if (Match(")"))
                    {
                        AnalyzeStatement();
                    }
                    else
                    {
                        analyze += "Expected ')'.\n";
                    }
                }
                else
                {
                    analyze += "Invalid constant list.\n";
                }
            }
            else
            {
                analyze += "Invalid for statement.\n";
            }
        }
        else if (Match("while"))
        {
            if (MatchExpression() && Match("do"))
            {
                AnalyzeStatement();
                return;
            }
            analyze += "Invalid while statement.\n";
        }
        else if (Match("readln"))
        {
            if (MatchIdentifier())
            {
                while (Match(",") && MatchIdentifier())
                {
                }

                if (Match(";"))
                {
                    return;
                }
            }
            analyze += "Invalid readln statement.\n";
        }
        else if (Match("writeln"))
        {
            if (Match("("))
            {
                while (!Match(";"))
                {
                    MatchExpression();
                }

                currentIndex--;
                if (Match(";"))
                {
                    return;
                }
            }
            analyze += "Invalid writeln statement.\n";
        }
        else if (MatchIdentifier() && Match("="))
        {
            while (!Match(";"))
            {
                MatchExpression();
                if (currentIndex != LexicalAnalyze.Tokens.Count) continue;
                analyze += "Expected ';'.\n";
                return;
            }
            return;
        }
        else
        {
            analyze += "Invalid statement.\n";
        }
    }

    private static bool Match(string expected)
    {
        if (currentIndex >= LexicalAnalyze.Tokens.Count || LexicalAnalyze.Tokens[currentIndex].Value != expected) return false;
        currentIndex++;
        return true;
    }

    private static bool MatchIdentifier()
    {
        if (currentIndex >= LexicalAnalyze.Tokens.Count || LexicalAnalyze.Tokens[currentIndex].Type != Token.TokenType.Variable) return false;
        currentIndex++;
        return true;
    }

    private static bool MatchType()
    {
        if (currentIndex >= LexicalAnalyze.Tokens.Count || (LexicalAnalyze.Tokens[currentIndex].Value != "integer" &&
                                                            LexicalAnalyze.Tokens[currentIndex].Value != "real" &&
                                                            LexicalAnalyze.Tokens[currentIndex].Value != "boolean")) return false;
        currentIndex++;
        return true;
    }

    private static bool MatchExpression()
    {
        return MatchIdentifier() || MatchConstant() || Match("+") || Match("-") || Match("/") || Match("*") || Match("(") || Match(")") || Match("==") || Match(">") || Match("<") || Match(">=") || Match("<=") || Match("!=");
    }

    private static bool MatchConstant()
    {
        // Пример: Разбор целых чисел
        if (currentIndex < LexicalAnalyze.Tokens.Count && LexicalAnalyze.Tokens[currentIndex].Type == Token.TokenType.Variable &&
            IsInteger(LexicalAnalyze.Tokens[currentIndex].Value))
        {
            currentIndex++;
            return true;
        }

        // Пример: Разбор действительных чисел
        if (currentIndex < LexicalAnalyze.Tokens.Count && LexicalAnalyze.Tokens[currentIndex].Type == Token.TokenType.Variable &&
            IsReal(LexicalAnalyze.Tokens[currentIndex].Value))
        {
            currentIndex++;
            return true;
        }

        // Пример: Разбор логических значений
        if (currentIndex < LexicalAnalyze.Tokens.Count && LexicalAnalyze.Tokens[currentIndex].Type == Token.TokenType.Variable &&
            IsBoolean(LexicalAnalyze.Tokens[currentIndex].Value))
        {
            currentIndex++;
            return true;
        }

        if (currentIndex < LexicalAnalyze.Tokens.Count && LexicalAnalyze.Tokens[currentIndex].Type == Token.TokenType.Variable &&
            IsChar(LexicalAnalyze.Tokens[currentIndex].Value))
        {
            currentIndex++;
            return true;
        }

        if (currentIndex < LexicalAnalyze.Tokens.Count && LexicalAnalyze.Tokens[currentIndex].Type == Token.TokenType.Variable &&
            IsString(LexicalAnalyze.Tokens[currentIndex].Value))
        {
            currentIndex++;
            return true;
        }

        // Добавьте другие правила разбора констант согласно вашей грамматике
        return false;
    }

    private static bool IsInteger(string word)
    {
        return int.TryParse(word, out _);
    }

    private static bool IsReal(string word)
    {
        return double.TryParse(word.Replace('.', ','), out _);
    }

    private static bool IsBoolean(string word)
    {
        return word == "true" || word == "false";
    }

    private static bool IsChar(string word)
    {
        return word[0] == '\'' && word[^1] == '\'' && word.Length == 3;
    }

    private static bool IsString(string word)
    {
        return word[0] == '\"' && word[^1] == '\"' && word.Length >= 3;
    }
}