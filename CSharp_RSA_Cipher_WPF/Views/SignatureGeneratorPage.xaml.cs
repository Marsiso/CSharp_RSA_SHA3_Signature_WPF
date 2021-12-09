using CSharp_RSA_Cipher_WPF.ViewModels;
using System.Windows.Controls;

namespace CSharp_RSA_Cipher_WPF.Views
{
    /// <summary>
    /// Interaction logic for EncryptionPage.xaml
    /// </summary>
    public partial class SignatureGeneratorPage : Page
    {
        readonly MainWindowViewModel mainWindowViewModel;

        public SignatureGeneratorPage(MainWindowViewModel instance)
        {
            mainWindowViewModel = instance;
            DataContext = mainWindowViewModel;
            InitializeComponent();
        }
    }
}
