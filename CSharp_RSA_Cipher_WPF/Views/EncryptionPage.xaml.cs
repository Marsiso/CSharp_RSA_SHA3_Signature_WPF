using CSharp_RSA_Cipher_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace CSharp_RSA_Cipher_WPF.Views
{
    /// <summary>
    /// Interaction logic for EncryptionPage.xaml
    /// </summary>
    public partial class EncryptionPage : Page
    {
        readonly MainWindowViewModel mainWindowViewModel;

        public EncryptionPage(MainWindowViewModel instance)
        {
            mainWindowViewModel = instance;
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox? textBox = sender as TextBox;
            if (textBox is null || e.Key.Equals(Key.Enter) is false)
            {
                return;
            }
            _ = LblOutput.Focus();
        }
    }
}
