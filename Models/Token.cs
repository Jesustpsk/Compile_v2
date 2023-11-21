using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Compile_v2;

public class Token : INotifyPropertyChanged
{
    private string _value;
    private TokenType _type;
    public Token(string matchValue, TokenType separator)
    {
        Value = matchValue;
        Type = separator;
    }

    public string Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged("Value");
        }
    }

    public TokenType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged("Type");
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName]string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public enum TokenType
    {
        Separator,
        KeyWord,
        Variable,
        Integer,
        Real,
        Boolean,
        Char,
        String,
        Unknown
    }
}