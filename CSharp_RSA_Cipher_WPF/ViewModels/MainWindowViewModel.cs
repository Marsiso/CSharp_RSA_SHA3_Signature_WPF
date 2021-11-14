using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Org.BouncyCastle.Math;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private string input;
        private string output;
        private BigInteger n;
        private BigInteger Φn;
        private BigInteger p;
        private BigInteger q;
        private BigInteger e;
        private BigInteger d;

        public MainWindowViewModel()
        {
            p = BigInteger.Zero;
            q = BigInteger.Zero;
            e = BigInteger.Zero;
            d = BigInteger.Zero;
            n = BigInteger.Zero;
            Φn = BigInteger.Zero;
            input = string.Empty;
            output = string.Empty;
            BigIntegerConverter = new BigIntegerConverter();

            //(P, Q) = RSA.GetPQ();
            //N = RSA.GetN(p, q);
            //ΦN = RSA.GetΦ(p, q);
            //E = RSA.GenereatePublicKey(Φn);
            //D = RSA.GenereatePrivateKey(e, Φn);
        }

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

        public BigInteger N
        {
            get => n;
            set => SetProperty(ref n, value);
        }

        public BigInteger ΦN
        {
            get => Φn;
            set => SetProperty(ref Φn, value);
        }

        public BigInteger P
        {
            get => p;
            set => SetProperty(ref p, value);
        }

        public BigInteger Q
        {
            get => q;
            set => SetProperty(ref q, value);
        }

        public BigInteger E
        {
            get => e;
            set => SetProperty(ref e, value);
        }

        public BigInteger D
        {
            get => d;
            set => SetProperty(ref d, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public BigIntegerConverter BigIntegerConverter { get; set; }

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

        public ICommand CommandSwapInputOutput
        {
            get => new CommandHandler(() => (Input, Output) = (Output, Input), () => true);
        }

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
