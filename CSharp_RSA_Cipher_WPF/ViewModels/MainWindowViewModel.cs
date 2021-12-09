using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Digests;
using Microsoft.Win32;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private BigInteger sharedKey = BigInteger.Zero;
        private BigInteger publicKey = BigInteger.Zero;
        private BigInteger privateKey = BigInteger.Zero;
        private FileInfo? sourceFileInfo;
        private FileInfo? sourceFileCopyInfo;

        public BigInteger PublicKey
        {
            get => publicKey;
            set => SetProperty(ref publicKey, value);
        }

        public BigInteger PrivateKey
        {
            get => privateKey;
            set => SetProperty(ref privateKey, value);
        }

        public BigInteger SharedKey
        {
            get => sharedKey;
            set => SetProperty(ref sharedKey, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand CommandGenerateKeypPairs => new CommandHandler(() => (PublicKey, PrivateKey, SharedKey) = RSA.GenerateKeyPairs(), () => true);

        public ICommand CommandSavePublicKeyPair => new CommandHandler(() => 
        {
            const string title = "Vepsání veřejného klíče do souboru";
            SaveFileDialog saveFileDialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Soubory veřejného klíče (*.pub)|*.pub",
                Title = title
            };

            if (saveFileDialog.ShowDialog() is false)
                return;
            var path = saveFileDialog.FileName;
            const string msg = "RSA";
            var publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(PublicKey.ToString());
            var sharedKeyBytes = System.Text.Encoding.UTF8.GetBytes(SharedKey.ToString());
            var lines = new[]
            {
                string.Join(' ', msg, Convert.ToBase64String(publicKeyBytes)),
                string.Join(' ', msg, Convert.ToBase64String(sharedKeyBytes))
            };
            File.WriteAllLines(path, lines);
        }, () => true);

        public ICommand CommandOpenPublicKeyPair => new CommandHandler(() =>
        {
            const string title = "Přečtení veřejného klíče ze souboru";
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Soubory veřejného klíče (*.pub)|*.pub",
                Title = title
            };

            if (openFileDialog.ShowDialog() is false)
                return;
            var path = openFileDialog.FileName;
            var lines = File.ReadAllLines(openFileDialog.FileName);
            var publicKeyBytes = Convert.FromBase64String(lines[0].Split(' ')[1]);
            var sharedKeyBytes = Convert.FromBase64String(lines[1].Split(' ')[1]);

            PublicKey = new BigInteger(System.Text.Encoding.UTF8.GetString(publicKeyBytes));
            SharedKey = new BigInteger(System.Text.Encoding.UTF8.GetString(sharedKeyBytes));
        }, () => true);

        public ICommand CommandSavePrivateKeyPair => new CommandHandler(() =>
        {
            const string title = "Vepsání soukromého klíče do souboru";
            SaveFileDialog saveFileDialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Soubory soukromého klíče (*.priv)|*.priv",
                Title = title
            };

            if (saveFileDialog.ShowDialog() is false)
                return;
            var path = saveFileDialog.FileName;
            const string msg = "RSA";
            var privateKeyBytes = System.Text.Encoding.UTF8.GetBytes(PrivateKey.ToString());
            var sharedKeyBytes = System.Text.Encoding.UTF8.GetBytes(SharedKey.ToString());
            var lines = new[]
            {
                string.Join(' ', msg, Convert.ToBase64String(privateKeyBytes)),
                string.Join(' ', msg, Convert.ToBase64String(sharedKeyBytes))
            };
            File.WriteAllLines(path, lines);
        }, () => true);

        public ICommand CommandOpenPrivateKeyPair => new CommandHandler(() =>
        {
            const string title = "Přečtení soukromého klíče ze souboru";
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Soubory soukromého klíče (*.priv)|*.priv",
                Title = title
            };

            if (openFileDialog.ShowDialog() is false)
                return;
            var path = openFileDialog.FileName;
            var lines = File.ReadAllLines(openFileDialog.FileName);
            var privateKeyBytes = Convert.FromBase64String(lines[0].Split(' ')[1]);
            var sharedKeyBytes = Convert.FromBase64String(lines[1].Split(' ')[1]);

            PrivateKey = new BigInteger(System.Text.Encoding.UTF8.GetString(privateKeyBytes));
            SharedKey = new BigInteger(System.Text.Encoding.UTF8.GetString(sharedKeyBytes));
        }, () => true);

        public void SetProperty<T>(ref T store, T value, [CallerMemberName] string name = null)
        {
            if (Equals(store, value) is false)
            {
                store = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public FileInfo SourceFileInfo
        {
            get => sourceFileInfo;
            set => SetProperty(ref sourceFileInfo, value);
        }

        public FileInfo SourceFileCopyInfo
        {
            get => sourceFileCopyInfo;
            set => SetProperty(ref sourceFileCopyInfo, value);
        }

        public string GetHashFromFile(string path)
        {
            var hashAlgorithm = new Sha3Digest(512);
            var fileBytes = File.ReadAllBytes(path);
            hashAlgorithm.BlockUpdate(fileBytes, 0, fileBytes.Length);
            var hash = new byte[64]; // 512b / 8B = 64b
            hashAlgorithm.DoFinal(hash, 0);
            return BitConverter.ToString(hash)
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}
