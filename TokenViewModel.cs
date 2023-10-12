using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Compile_v2;

public class TokenViewModel : INotifyPropertyChanged
{
    private Token _token;
    public ObservableCollection<Token> Tokens { get; set; }

    public Token _Token
    {
        get { return _token; }
        set
        {
            _token = value; 
            OnPropertyChanged("Token");
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName]string prop = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}