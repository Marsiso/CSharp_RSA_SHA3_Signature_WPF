using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

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
            Pages.Content = new SignatureGeneratorPage(mainWindowViewModel);
        }

        private void Exit_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is MouseButton.Left)
                Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton.Equals(MouseButton.Left) && e.ClickCount is 2)
            {
                if (WindowState is WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    return;
                }
                WindowState = WindowState.Maximized;
                return;
            }
            if (e.ChangedButton.Equals(MouseButton.Left) && WindowState is not WindowState.Maximized)
            {
                DragMove();
            }
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        private void BtnPageEncryption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BrushConverter brushConverter = new BrushConverter();
            BtnPageEncryption.Background = (Brush)brushConverter.ConvertFrom("#4C0816");
            BtnPageDecryption.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");
            BtnPageGenerator.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");

            Pages.Content = new SignatureGeneratorPage(mainWindowViewModel);
        }

        private void BtnPageDecryption_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BrushConverter brushConverter = new BrushConverter();
            BtnPageDecryption.Background = (Brush)brushConverter.ConvertFrom("#4C0816");
            BtnPageEncryption.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");
            BtnPageGenerator.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");

            Pages.Content = new FileVerificationPage(mainWindowViewModel);
        }

        private void BtnPageGenerator_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BrushConverter brushConverter = new BrushConverter();
            BtnPageGenerator.Background = (Brush)brushConverter.ConvertFrom("#4C0816");
            BtnPageEncryption.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");
            BtnPageDecryption.Background = (Brush)brushConverter.ConvertFrom("#5B0A1A");

            Pages.Content = new KeyPairsGeneratorPage(mainWindowViewModel);
        }
    }
}
