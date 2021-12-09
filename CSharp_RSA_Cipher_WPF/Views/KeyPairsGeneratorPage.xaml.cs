using CSharp_RSA_Cipher_WPF.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CSharp_RSA_Cipher_WPF.Views
{
    /// <summary>
    /// Interaction logic for GeneratorPage.xaml
    /// </summary>
    public partial class KeyPairsGeneratorPage : Page
    {
        readonly MainWindowViewModel mainWindowViewModel;

        public KeyPairsGeneratorPage(MainWindowViewModel instance)
        {
            mainWindowViewModel = instance;
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock? textBlock = sender as TextBlock;
            Clipboard.SetText(textBlock?.Text ?? string.Empty);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox is null || e.Key.Equals(Key.Enter) is false)
            {
                return;
            }
            _ = LblPublicKey.Focus();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void NumberValidationTextBox(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                Regex regex = new Regex("[^0-9]+");
                if (regex.IsMatch(text) is true)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
