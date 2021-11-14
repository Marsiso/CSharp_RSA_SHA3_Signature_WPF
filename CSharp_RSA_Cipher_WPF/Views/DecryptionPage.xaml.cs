using CSharp_RSA_Cipher_WPF.ViewModels;
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

namespace CSharp_RSA_Cipher_WPF.Views
{
    /// <summary>
    /// Interaction logic for DecryptionPage.xaml
    /// </summary>
    public partial class DecryptionPage : Page
    {
        readonly MainWindowViewModel mainWindowViewModel;

        public DecryptionPage(MainWindowViewModel instance)
        {
            mainWindowViewModel = instance;
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                TextBox? textBox = sender as TextBox;
                textBox?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            }
        }
    }
}
