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
            input = LexicalAnalyze.RemoveComments(input);

            // Разделение на лексемы
            //var lexemes = Analyze(input);
            //Tokenize(input);
            // Вывод результатов
            AsmOutput.Text = "";
            /*foreach (var lexeme in lexemes)
            {
                AsmOutput.Text += lexeme + Environment.NewLine;
            }*/
            var separators = LexicalAnalyze.LexAnalyze(input)[0].Aggregate("", (current, i) => current + (i.Value + "\n"));
            var keywords = LexicalAnalyze.LexAnalyze(input)[1].Aggregate("", (current, i) => current + (i.Value + "\n"));
            var variable = LexicalAnalyze.LexAnalyze(input)[2].Aggregate("", (current, i) => current + (i.Value + "\n"));
            TbOutSeparatorsWords.Text = separators;
            TbOutKeyWords.Text = keywords;
            TbOutVariableWords.Text = variable;
            AsmOutput.Text = String.Join("\r\n",LexicalAnalyze.Tokenize(input));
        }
    }
}