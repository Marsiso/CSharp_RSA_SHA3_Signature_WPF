using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string input;
        private string output;

        public string Input
        {
            get => input;
            set => SetProperty(ref input, value);
        }

        public string Output 
        { 
            get => output; 
            set => SetProperty(ref output, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand CommandOpenFromFile
        {
            get => new CommandHandler(() =>
            {
                OpenFileDialog openFileDialog = new();
                openFileDialog.Filter = "Text file (*.txt)|*.txt|Data file (*.dat)|*.dat";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                    Input = File.ReadAllText(openFileDialog.FileName);
            }, () => true);
        }

        public ICommand CommandSaveToFile
        {
            get => new CommandHandler(() =>
            {
                SaveFileDialog saveFileDialog = new();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt|Data file (*.dat)|*.dat";
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, Output);
            }, () => true);
        }

        public ICommand CommanSwapInputOutput => new CommandHandler(() => (Input, Output) = (Output, Input), () => true);

        public void SetProperty<T>(ref T store, T value, [CallerMemberName] string name = null)
        {
            if (Equals(store, value) is false)
            {
                store = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
