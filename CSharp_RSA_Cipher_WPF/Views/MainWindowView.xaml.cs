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
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            Pages.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            Pages.Content = new EncryptionPage(mainWindowViewModel);
        }

        private void Exit_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is MouseButton.Left)
                Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.ChangedButton.Equals(MouseButton.Left))
            {
                return;
            }
            DragMove();
        }

        private void BtnPageEncryption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindowViewModel.Input = String.Empty;
            mainWindowViewModel.Output = String.Empty;
            Pages.Content = new EncryptionPage(mainWindowViewModel);
        }

        private void BtnPageDecryption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindowViewModel.Input = String.Empty;
            mainWindowViewModel.Output = String.Empty;
            Pages.Content = new DecryptionPage(mainWindowViewModel);
        }

        private void BtnPageGenerator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Pages.Content = new GeneratorPage(mainWindowViewModel);
        }
    }
}
