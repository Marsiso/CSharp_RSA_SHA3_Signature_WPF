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
        }

        private void ButtonExit_OnClick(object sender, RoutedEventArgs e) => Close();

        private void ButtonMinimize_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ButtonMaximize_OnClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Equals(WindowState.Normal)
            ? WindowState.Maximized
            : WindowState.Normal;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!e.ChangedButton.Equals(MouseButton.Left))
            {
                return;
            }
            DragMove();
        }
    }
}
