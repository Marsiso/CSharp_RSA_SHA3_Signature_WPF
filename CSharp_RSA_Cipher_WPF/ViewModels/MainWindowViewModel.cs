using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Digests;
using Microsoft.Win32;
using System.IO.Compression;
using System.Linq;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private BigInteger sharedKey = BigInteger.Zero;
        private BigInteger publicKey = BigInteger.Zero;
        private BigInteger privateKey = BigInteger.Zero;
        private FileInfo? sourceFileInfo;
        private FileInfo? sourceFileCopyInfo;
        private string sourceFileHashEncrypted = string.Empty;
        private string sourceFileHashDoubleEncrypted = string.Empty;
        private string sourceFileCopyHashEncrypted = string.Empty;
        private string sourceFileCopyHashDoubleEncrypted = string.Empty;
        private string isVerificationAlright;

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

        public string SourceFileHashEncrypted
        {
            get => sourceFileHashEncrypted;
            set => SetProperty(ref sourceFileHashEncrypted, value);
        }

        public string SourceFileHashDoubleEncrypted
        {
            get => sourceFileHashDoubleEncrypted;
            set => SetProperty(ref sourceFileHashDoubleEncrypted, value);
        }

        public string SourceFileCopyHashEncrypted
        {
            get => sourceFileCopyHashEncrypted;
            set => SetProperty(ref sourceFileCopyHashEncrypted, value);
        }

        public string SourceFileCopyHashDoubleEncrypted
        {
            get => sourceFileCopyHashDoubleEncrypted;
            set => SetProperty(ref sourceFileCopyHashDoubleEncrypted, value);
        }

        public string IsVerificationAlright 
        { 
            get => isVerificationAlright;
            set => SetProperty(ref isVerificationAlright, value);
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

        public ICommand CommandOpenSourceFile => new CommandHandler(() =>
        {
            const string title = "Načtení zdrojového souboru";
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Všechny soubory (*.*)|*.*",
                Title = title
            };

            if (openFileDialog.ShowDialog() is false)
                return;
            var path = openFileDialog.FileName;
            SourceFileInfo = new FileInfo(path);
        }, () => true);

        public ICommand CommandOpenZipFile => new CommandHandler(() =>
        {
            const string title = "Otevření archivu se zdrojovým souborem a podpisem";
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = false,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Archivované soubory (*.zip)|*.zip",
                Title = title
            };
            if (openFileDialog.ShowDialog() is false)
                return;
            var zipFileName = openFileDialog.FileName;
            var dirFileName = zipFileName.Replace(".zip", string.Empty);

            //if (Directory.Exists(dirFileName))
            //    throw new ArgumentException();
            ZipFile.ExtractToDirectory(zipFileName, dirFileName, true);
            //var directoryInfo = new DirectoryInfo(dirFileName);
            //if ((directoryInfo.Attributes & FileAttributes.Hidden) is not FileAttributes.Hidden)
            //    directoryInfo.Attributes |= FileAttributes.Hidden;
            var fileNames = Directory.EnumerateFiles(dirFileName);
            if (fileNames.Count() is not 2)
                throw new ArgumentOutOfRangeException();

            if (fileNames.Any(entry => entry.Contains(".sign")) is false)
                throw new ArgumentException();
            string? signature = null;
            string? hash = null;
            foreach (var fileName in fileNames)
            {
                if (fileName.Contains(".sign"))
                {
                    signature = File.ReadAllText(fileName);
                    signature = signature.Split(' ')[1];
                    var temp = Convert.FromBase64String(signature);
                    SourceFileCopyHashDoubleEncrypted = System.Text.Encoding.UTF8.GetString(temp);
                    SourceFileCopyHashEncrypted = RSA.Decrypt(SourceFileCopyHashDoubleEncrypted, PublicKey, SharedKey);
                }
                else
                {
                    SourceFileCopyInfo = new FileInfo(fileName);
                    hash = GetHashFromFile(fileName);
                }
            }

            bool isAltered = string.Compare(hash, SourceFileCopyHashEncrypted) is 0;
            IsVerificationAlright = isAltered
            ? "Verifikace dokončena. Obsah souboru zůstal beze změny."
            : "Verifikace dokončena. Obsah souboru byl pozměněn.";
        }, () => true);

        public ICommand CommandSaveZipFile => new CommandHandler(() =>
        {
            // Zip file path
            const string title = "Vytvoření archivu se zdrojovým souborem a podpisem";
            SaveFileDialog saveFileDialog = new()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Filter = "Archivované soubory (*.zip)|*.zip",
                Title = title
            };
            if (saveFileDialog.ShowDialog() is false)
                return;
            var path = saveFileDialog.FileName;
            var directoryPath = path.Replace(".zip", string.Empty);
            Directory.CreateDirectory(directoryPath);
            // Create signature string
            const string msg = "RSA_SHA3-512";
            var encodedHash = string.Join(' ', msg, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SourceFileHashDoubleEncrypted)));
            // Store signature and the copy of source file in .zip file
            File.WriteAllText(directoryPath + @"\signature.sign", encodedHash);
            File.Copy(SourceFileInfo.FullName, directoryPath + @"\" + SourceFileInfo.Name);
            ZipFile.CreateFromDirectory(directoryPath, path);
            Directory.Delete(directoryPath, true);
        }, () => true);

        public ICommand CommandGenerateHashFromSourceFile => new CommandHandler(() =>
        {
            SourceFileHashEncrypted = GetHashFromFile(SourceFileInfo.FullName);
            SourceFileHashDoubleEncrypted = RSA.Encrypt(sourceFileHashEncrypted, PrivateKey, SharedKey);
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