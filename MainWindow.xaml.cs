﻿using System;
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
using Compile_v2.Analyze;
using SyntaxAnalyze;

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
            TbOutSyntaxAnalyze.Text = "";
            var input = TbInput.Text;

            input = LexicalAnalyzer.RemoveComments(input);
            
            AsmOutput.Text = "";
            var separators = LexicalAnalyzer.LexicalAnalyze(input)[0].Aggregate("", (current, i) => current + (i.Value + "\n"));
            var keywords = LexicalAnalyzer.LexicalAnalyze(input)[1].Aggregate("", (current, i) => current + (i.Value + "\n"));
            var variable = LexicalAnalyzer.LexicalAnalyze(input)[2].Aggregate("", (current, i) => current + (i.Value + "\n"));
            TbOutSeparatorsWords.Text = separators;
            TbOutKeyWords.Text = keywords;
            TbOutVariableWords.Text = variable;
            LexicalAnalyzer.Tokenize(input);
            AsmOutput.Text = string.Join("\r\n", LexicalAnalyzer.TokensToStringArray());
            try
            {
                TbOutSyntaxAnalyze.Text = SyntaxAnalyzer.ParseProgram() + '\n';
                try
                {
                    TbOutSyntaxAnalyze.Text += SemanticAnalyze.AnalyzeSemantic(SyntaxAnalyzer.tokens);
                    try
                    {
                        var result = ASMConverter.ConvertProgram();
                        AsmOutput.Text = result[0];
                        TbOutSyntaxAnalyze.Text += result[1];
                    }
                    catch (Exception ex)
                    {
                        TbOutSyntaxAnalyze.Text = ex.ToString();
                    }
                }
                catch (Exception ex)
                {
                    TbOutSyntaxAnalyze.Text = ex.ToString();
                }
            }
            catch (Exception ex)
            {
                TbOutSyntaxAnalyze.Text = ex.ToString();
            }
        }
    }
}