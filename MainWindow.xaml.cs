using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compile_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void analyzeButton_Click(object sender, EventArgs e)
        {
            TbOutSeparatorsWords.Text = "";
            TbOutKeyWords.Text = "";
            TbOutVariableWords.Text = "";
            var input = TbInput.Text;

            // Удаление комментариев
            input = RemoveComments(input);

            // Разделение на лексемы
            //var lexemes = Analyze(input);
            //Tokenize(input);
            // Вывод результатов
            AsmOutput.Text = "";
            /*foreach (var lexeme in lexemes)
            {
                AsmOutput.Text += lexeme + Environment.NewLine;
            }*/
            var separators = "";
            var keywords = "";
            var variable = "";
            foreach (var i in LexicalAnalyze.Analyze(input)[0])
            {
                separators += i.Value + "\n";
            }
            foreach (var i in LexicalAnalyze.Analyze(input)[1])
            {
                keywords += i.Value + "\n";
            }
            foreach (var i in LexicalAnalyze.Analyze(input)[2])
            {
                variable += i.Value + "\n";
            }
            TbOutSeparatorsWords.Text = separators;
            TbOutKeyWords.Text = keywords;
            TbOutVariableWords.Text = variable;
        }

        private string RemoveComments(string input)
        {
            var startIndex = input.IndexOf('{');
            var endIndex = input.IndexOf('}');
            while (startIndex >= 0 && endIndex > startIndex)
            {
                var comment = input.Substring(startIndex, endIndex - startIndex + 1);
                input = input.Replace(comment, "");
                startIndex = input.IndexOf('{');
                endIndex = input.IndexOf('}');
            }

            return input;
        }
    }
}