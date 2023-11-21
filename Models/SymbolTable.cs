using System.Collections.Generic;

namespace Compile_v2;

public class SymbolTable
{
    private static readonly Dictionary<string, string> VariableTable = new();

    public static bool AddVariable(string identifier, string type)
    {
        if (VariableTable.ContainsKey(identifier)) return false;
        VariableTable.Add(identifier, type);
        return true;
    }

    public static string GetVariableType(string identifier)
    {
        return VariableTable.ContainsKey(identifier) ? VariableTable[identifier] : null;
    }
}