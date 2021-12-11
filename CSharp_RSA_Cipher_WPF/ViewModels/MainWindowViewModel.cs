using Microsoft.Win32;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string VerificationSuccess = "Verifikace dokončena. Obsah souboru zůstal beze změny.";
        private const string VerificationFailure = "Verifikace dokončena. Obsah souboru byl pozměněn.";
        private const string TitleOpenZipFile = "Otevření archivu se zdrojovým souborem a podpisem";
        private const string FormatZipFile = "Archivované soubory (*.zip)|*.zip";
        private BigInteger sharedKey = BigInteger.Zero;
        private BigInteger publicKey = BigInteger.Zero;
        private BigInteger privateKey = BigInteger.Zero;
        private FileInfo? sourceFileInfo;
        private FileInfo? sourceFileCopyInfo;
        private string sourceFileHashEncrypted = string.Empty;
        private string sourceFileHashDoubleEncrypted = string.Empty;
        private string sourceFileCopyHashEncrypted = string.Empty;
        private string sourceFileCopyHashDoubleEncrypted = string.Empty;
        private string isVerificationAlright = string.Empty;
        private string message = string.Empty;

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

        public FileInfo? SourceFileInfo
        {
            get => sourceFileInfo;
            set => SetProperty(ref sourceFileInfo, value);
        }

        public FileInfo? SourceFileCopyInfo
        {
            get => sourceFileCopyInfo;
            set => SetProperty(ref sourceFileCopyInfo, value);
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

        public string Message { get => message; set => SetProperty(ref message, value); }

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
              // Zip and directory file paths
              OpenFileDialog openFileDialog = new()
              {
                  Multiselect = false,
                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  Filter = FormatZipFile,
                  Title = TitleOpenZipFile
              };
              if (openFileDialog.ShowDialog() is false)
              {
                  return;
              }

              var zipFileName = openFileDialog.FileName;
              var dirFileName = zipFileName.Replace(".zip", string.Empty); // Directory inherits zip file's file name

              // Zip file extraction
              ZipFile.ExtractToDirectory(zipFileName, dirFileName, true); // Overwrites existing file
              var directoryInfo = new DirectoryInfo(dirFileName);
              if ((directoryInfo.Attributes & FileAttributes.Hidden) is not FileAttributes.Hidden) // Make directory hidden
                  directoryInfo.Attributes |= FileAttributes.Hidden;

              // Extracted zip file format validation
              var fileNames = Directory.EnumerateFiles(dirFileName);
              if (fileNames.Count() is not 2 || fileNames.Count(entry => entry.Contains(".sign")) is not 1)
              {
                  Directory.Delete(dirFileName); // Tidy up
                  return;
              }

              // Open extracted files
              string? srcFileCopyHash = null;
              try
              {
                  foreach (var fileName in fileNames)
                  {
                      if (fileName.Contains(".sign"))
                      {
                          var fileContent = File.ReadAllText(fileName);
                          var base64String = fileContent.Split(' ')[1];
                          var utf8 = Convert.FromBase64String(base64String);
                          SourceFileCopyHashDoubleEncrypted = System.Text.Encoding.UTF8.GetString(utf8);
                          SourceFileCopyHashEncrypted = RSA.Decrypt(sourceFileCopyHashDoubleEncrypted, publicKey, sharedKey);
                      }
                      else
                      {
                          SourceFileCopyInfo = new FileInfo(fileName);
                          srcFileCopyHash = GetHashFromFile(fileName);
                      }
                  }
              }
              catch (ArgumentOutOfRangeException)
              {
                  TidyUpOnException(dirFileName).Invoke();
              }
              catch (FormatException)
              {
                  TidyUpOnException(dirFileName).Invoke();
              }
              catch (UnauthorizedAccessException)
              {
                  TidyUpOnException(dirFileName).Invoke();
              }
              catch (NotSupportedException)
              {
                  TidyUpOnException(dirFileName).Invoke();
              }

              // Verify source file copy
              bool isAltered = string.Compare(srcFileCopyHash, sourceFileCopyHashEncrypted) is 0;
              IsVerificationAlright = isAltered
                  ? VerificationSuccess
                  : VerificationFailure;

              Directory.Delete(dirFileName); // Tidy up
          }, () => true);

        private Action TidyUpOnException(string dirFileName)
        {
            return () =>
            {
                Directory.Delete(dirFileName); // Tidy up
                SourceFileCopyInfo = null;
                SourceFileCopyHashEncrypted = string.Empty;
                SourceFileCopyHashDoubleEncrypted = string.Empty;
            };
        }

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