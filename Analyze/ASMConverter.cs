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
    public static string[] ConvertProgram()
    {
        _tokens = SyntaxAnalyze.SyntaxAnalyzer.tokens;
        ConvertDataStatement();
        MatchKeyword("begin");
        _asmText += "selection .text\nglobal _start\n";
        ConvertProgramStatement();
        return new []{_asmText, "\nПеревод кода в ассемблер завершен."};
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
        _asmText += token.Value + " " + GetTypeAsmDirective(SymbolTable.GetVariableType(token.Value)) + '\n';
        while (CurrentToken.Value == ",")
        {
            Match(",");
            MatchIdentifier();
            token = GetToken();
            _asmText += token.Value + " " + GetTypeAsmDirective(SymbolTable.GetVariableType(token.Value)) + '\n';
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
    private static Token LookAhead(int count)
    {
        if (_currentIndex + count < _tokens.Count)
        {
            return _tokens[_currentIndex + count];
        }
        return new Token("", Token.TokenType.Unknown);
    }

    private static void ParseExpression()
{
    Stack<string> operators = new Stack<string>();
    Queue<string> outputQueue = new Queue<string>();

    while (IsOperator(CurrentToken.Value) || IsType(CurrentToken.Value) || IsIdentifier(CurrentToken.Value) || IsNumericConstant(CurrentToken.Value))
    {
        string currentTokenValue = CurrentToken.Value;

        if (IsIdentifier(currentTokenValue) || IsNumericConstant(currentTokenValue) || IsType(currentTokenValue))
        {
            outputQueue.Enqueue(currentTokenValue);
        }
        else if (IsOperator(currentTokenValue))
        {
            string currentOperator = currentTokenValue;

            while (operators.Count > 0 && GetOperatorPriority(operators.Peek()) >= GetOperatorPriority(currentOperator))
            {
                outputQueue.Enqueue(operators.Pop());
            }

            operators.Push(currentOperator);
        }

        ConsumeToken();
    }

    while (operators.Count > 0)
    {
        outputQueue.Enqueue(operators.Pop());
    }

    foreach (string token in outputQueue)
    {
        if (IsIdentifier(token) || IsNumericConstant(token) || IsType(token))
        {
            GenerateAssemblyCodeForOperand(token);
        }
        else if (IsOperator(token))
        {
            switch (token)
            {
                case "+":
                    GenerateAssemblyCodeForAddition();
                    break;
                case "-":
                    GenerateAssemblyCodeForSubtraction();
                    break;
                case "*":
                    GenerateAssemblyCodeForMultiplication();
                    break;
                case "/":
                    GenerateAssemblyCodeForDivision();
                    break;
                case ">":
                    GenerateAssemblyCodeForGreaterThan();
                    break;
                case "<":
                    GenerateAssemblyCodeForLessThan();
                    break;
                case "=" when LookAhead(1).Value == "=":
                    GenerateAssemblyCodeForEqual();
                    break;
            }
        }
    }
}

private static void GenerateAssemblyCodeForGreaterThan()
{
    _asmText += "POP EAX\n";
    _asmText += "POP EBX\n";
    _asmText += "CMP EBX, EAX\n";
    _asmText += "JG greater_than\n";
    _asmText += "JLE less_than\n";
}

private static void GenerateAssemblyCodeForLessThan()
{
    _asmText += "POP EAX\n";
    _asmText += "POP EBX\n";
    _asmText += "CMP EBX, EAX\n";
    _asmText += "JL less_than\n";
    _asmText += "JGE greater_than\n";
}

private static void GenerateAssemblyCodeForEqual()
{
    _asmText += "POP EAX\n";
    _asmText += "POP EBX\n";
    _asmText += "CMP EBX, EAX\n";
    _asmText += "JE equal\n";
    _asmText += "JNE not_equal\n";
}

    private static int GetOperatorPriority(string op)
    {
        // Устанавливаем приоритет операторов (чем больше число, тем выше приоритет)
        switch (op)
        {
            case "+":
            case "-":
                return 1;
            case "*":
            case "/":
                return 2;
            // Другие операторы
            default:
                return 0; // Предполагаем, что идентификаторы и типы имеют приоритет 0
        }
    }
    
    private static void ParseSwitchStatement()
    {
        // Генерация метки для начала switch
        _asmText += "switch_condition:\n";

        MatchKeyword("switch");

        // Генерация кода для вычисления выражения в switch
        ParseExpression();

        // Генерация кода для сравнения с каждым case
        Match("{");

        while (CurrentToken.Value != "}")
        {
            if (CurrentToken.Value == "case")
            {
                MatchKeyword("case");

                // Генерация кода для сравнения выражения с текущим case
                ParseExpression();

                Match(":");

                // Генерация кода для блока case
                ParseBlock();
            }
            else
            {
                throw new Exception($"Ожидается оператор case или фигурная скобка, но получено '{CurrentToken.Value}'");
            }
        }

        // Генерация метки для завершения switch
        _asmText += "switch_end:\n";

        Match("}");
    }
    
    private static void ParseForEachStatement()
    {
        MatchKeyword("for");
        MatchKeyword("each");

        // Генерация кода для получения итерируемой коллекции
        ParseExpression();

        MatchKeyword("in");
        Match("(");

        while (CurrentToken.Value != ")")
        {
            // Генерация кода для обработки элементов коллекции
            ParseExpression();

            if (CurrentToken.Value == ",")
            {
                Match(",");
            }
        }

        Match(")");

        // Генерация кода для блока цикла
        ParseBlock();
    }
    
    private static void ParseWhileStatement()
    {
        MatchKeyword("while");

        // Генерация метки для начала цикла while
        _asmText += "while_condition:\n";

        // Генерация кода для выражения условия
        ParseExpression();

        // Генерация метки для условия окончания цикла
        _asmText += "while_end:\n";

        // Генерация кода для блока цикла
        ParseBlock();
    }
    private static void ParseReadlnStatement()
    {
        MatchKeyword("readln");
        Match("(");

        // Генерация кода для считывания переменных
        ParseIdentifierList();

        Match(")");
    }
    
    private static void ParseWritelnStatement()
    {
        MatchKeyword("writeln");
        Match("(");

        // Генерация кода для вывода выражений
        ParseExpressionList();

        Match(")");
    }
    
    private static void ParseExpressionList()
    {
        // Вызываем ParseExpression для первого выражения
        ParseExpression();

        // Генерируем код на ассемблере для первого операнда
        //GenerateAssemblyCodeForOperand();

        // Проверяем, есть ли еще выражения
        while (CurrentToken.Value == ",")
        {
            // Пропускаем запятую
            Match(",");

            // Вызываем ParseExpression для следующего выражения
            ParseExpression();

            // Генерируем код на ассемблере для следующего операнда
            //GenerateAssemblyCodeForOperand();

            // Генерируем код на ассемблере для обработки запятой
            GenerateAssemblyCodeForComma();
        }
    }
    
    private static void GenerateAssemblyCodeForOperand()
    {
        // Получаем текущий токен
        var currentToken = CurrentToken;

        // Проверяем тип токена
        switch (currentToken.Type)
        {
            case Token.TokenType.Variable:
                // Генерируем код для переменной, например, MOV instruction
                _asmText += $"MOV eax, {currentToken.Value}\n";
                break;
            case Token.TokenType.Integer:
                // Генерируем код для целого числа, например, MOV instruction
                _asmText += $"MOV eax, {currentToken.Value}\n";
                break;
            case Token.TokenType.Real:
                // Генерируем код для вещественного числа
                // (в зависимости от архитектуры и языка это может потребовать других инструкций)
                _asmText += $"MOV xmm0, {currentToken.Value}\n";
                break;
            // Добавьте обработку других типов, если необходимо
            default:
                throw new Exception($"Unexpected token type: {currentToken.Type}");
        }
    }

    private static void GenerateAssemblyCodeForComma()
    {
        // Простой пример генерации кода для обработки запятой
        _asmText += "; Code for comma processing\n";
    }
    
    private static void ParseIdentifierList()
    {
        while (IsIdentifier(CurrentToken.Value))
        {
            var identifier = CurrentToken.Value;

            var variableType = SymbolTable.GetVariableType(identifier);
            if (variableType == null)
            {
                throw new Exception($"Использование необъявленной переменной '{identifier}'");
            }

            _asmText += $"{identifier} {GetTypeAsmDirective(variableType)}\n";
            _asmText += $"MOV {identifier}, {GetInitialValue(variableType)}\n";

            ConsumeToken();

            if (CurrentToken.Value == ",")
            {
                Match(",");
            }
            else
            {
                break;
            }
        }
    }

    private static string GetTypeAsmDirective(string variableType)
    {
        switch (variableType)
        {
            case "integer":
                return "dd";
            case "real":
                return "dq";
            case "boolean":
            case "char":
            case "string":
                return "db";
            default:
                throw new Exception($"Неизвестный тип переменной: {variableType}");
        }
    }

    private static string GetInitialValue(string variableType)
    {
        switch (variableType)
        {
            case "integer":
            case "real":
                return "0";
            case "boolean":
                return "FALSE";
            case "char":
            case "string":
                return "''";
            default:
                throw new Exception($"Неизвестный тип переменной: {variableType}");
        }
    }
    
    private static bool IsOperator(string value)
    {
        return value == "+" || value == "-" || value == "*" || value == "/" || value == "=" || value == ">" || value == "<";
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
    
    // Пример генерации кода для сложения
    private static void GenerateAssemblyCodeForAddition()
    {
        _asmText += "POP EAX\n";        // Извлекаем первый операнд из стека
        _asmText += "POP EBX\n";        // Извлекаем второй операнд из стека
        _asmText += "ADD EAX, EBX\n";   // Выполняем сложение
        _asmText += "PUSH EAX\n";       // Помещаем результат обратно в стек
    }

// Пример генерации кода для вычитания
    private static void GenerateAssemblyCodeForSubtraction()
    {
        _asmText += "POP EAX\n";        // Извлекаем первый операнд из стека
        _asmText += "POP EBX\n";        // Извлекаем второй операнд из стека
        _asmText += "SUB EAX, EBX\n";   // Выполняем вычитание
        _asmText += "PUSH EAX\n";       // Помещаем результат обратно в стек
    }

// Пример генерации кода для умножения
    private static void GenerateAssemblyCodeForMultiplication()
    {
        _asmText += "POP EAX\n";        // Извлекаем первый операнд из стека
        _asmText += "POP EBX\n";        // Извлекаем второй операнд из стека
        _asmText += "IMUL EAX, EBX\n";  // Выполняем умножение
        _asmText += "PUSH EAX\n";       // Помещаем результат обратно в стек
    }

// Пример генерации кода для деления
    private static void GenerateAssemblyCodeForDivision()
    {
        _asmText += "POP EBX\n";        // Извлекаем делитель из стека
        _asmText += "POP EAX\n";        // Извлекаем делимое из стека
        _asmText += "IDIV EBX\n";       // Выполняем деление
        _asmText += "PUSH EAX\n";       // Помещаем результат обратно в стек
    }

// Пример генерации кода для загрузки значения переменной
    private static void GenerateAssemblyCodeForOperand(string variable)
    {
        // Предположим, что переменные представляют 32-битные значения (DWORD)
        _asmText += $"PUSH DWORD [{variable}]\n";  // Загружаем значение переменной в стек
    }
    private static bool IsNumericConstant(string value)
    {
        return int.TryParse(value, out _) || double.TryParse(value.Replace('.',','), out _);
    }
}