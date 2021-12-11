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
        private const string FilterZipFiles = "Archivované soubory (*.zip)|*.zip";
        private const string TitleOpenPrivateKeyFile = "Přečtení soukromého klíče ze souboru";
        private const string TitleOpenPublicKeyFile = "Přečtení veřejného klíče ze souboru";
        private const string TitleSavePublicKeyFile = "Vepsání veřejného klíče do souboru";
        private const string FilterPublicKeyFiles = "Soubory veřejného klíče (*.pub)|*.pub";
        private const string TitleSavePrivateKeyFile = "Vepsání soukromého klíče do souboru";
        private const string FilterPrivateKeyFiles = "Soubory soukromého klíče (*.priv)|*.priv";
        private BigInteger sharedKey = BigInteger.Zero;
        private BigInteger publicKey = BigInteger.Zero;
        private BigInteger privateKey = BigInteger.Zero;
        private FileInfo? sourceFileInfo;
        private FileInfo? sourceFileCopyInfo;
        private string sourceFileHash = string.Empty;
        private string sourceFileHashEncrypted = string.Empty;
        private string sourceFileCopyHash = string.Empty;
        private string sourceFileCopyDecryptedSignature = string.Empty;
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

        public string SourceFileHash
        {
            get => sourceFileHash;
            set => SetProperty(ref sourceFileHash, value);
        }

        public string SourceFileHashEncrypted
        {
            get => sourceFileHashEncrypted;
            set => SetProperty(ref sourceFileHashEncrypted, value);
        }

        public string SourceFileCopyHash
        {
            get => sourceFileCopyHash;
            set => SetProperty(ref sourceFileCopyHash, value);
        }

        public string SourceFileCopyDecryptedSignature
        {
            get => sourceFileCopyDecryptedSignature;
            set => SetProperty(ref sourceFileCopyDecryptedSignature, value);
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
              SaveFileDialog saveFileDialog = new()
              {
                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  Filter = FilterPublicKeyFiles,
                  Title = TitleSavePublicKeyFile
              };

              if (saveFileDialog.ShowDialog() is false)
                  return;
              var path = saveFileDialog.FileName;
              const string msg = "RSA";
              var bytes = System.Text.Encoding.UTF8.GetBytes(publicKey.ToString() + ' ' + sharedKey.ToString());
              var text = string.Join(' ', msg, Convert.ToBase64String(bytes));
              File.WriteAllText(path, text);
          }, () => true);

        public ICommand CommandOpenPublicKeyPair => new CommandHandler(() =>
          {
              OpenFileDialog openFileDialog = new()
              {
                  Multiselect = false,
                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  Filter = FilterPublicKeyFiles,
                  Title = TitleOpenPublicKeyFile
              };

              if (openFileDialog.ShowDialog() is false)
                  return;
              var path = openFileDialog.FileName;
              var text = File.ReadAllText(openFileDialog.FileName);
              var bytes = Convert.FromBase64String(text.Split(' ')[1]);
              var decoded = System.Text.Encoding.UTF8.GetString(bytes);
              var keys = decoded.Split(' ');
              PublicKey = new BigInteger(keys[0]);
              SharedKey = new BigInteger(keys[1]);
          }, () => true);

        public ICommand CommandSavePrivateKeyPair => new CommandHandler(() =>
          {
              SaveFileDialog saveFileDialog = new()
              {
                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  Filter = FilterPrivateKeyFiles,
                  Title = TitleSavePrivateKeyFile
              };

              if (saveFileDialog.ShowDialog() is false)
                  return;
              var path = saveFileDialog.FileName;
              const string msg = "RSA";
              var bytes = System.Text.Encoding.UTF8.GetBytes(privateKey.ToString() + ' ' + sharedKey.ToString());
              var text = string.Join(' ', msg, Convert.ToBase64String(bytes));
              File.WriteAllText(path, text);
          }, () => true);

        public ICommand CommandOpenPrivateKeyPair => new CommandHandler(() =>
          {
              OpenFileDialog openFileDialog = new()
              {
                  Multiselect = false,
                  InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                  Filter = FilterPrivateKeyFiles,
                  Title = TitleOpenPrivateKeyFile
              };

              if (openFileDialog.ShowDialog() is false)
                  return;
              var path = openFileDialog.FileName;
              var text = File.ReadAllText(openFileDialog.FileName);
              var bytes = Convert.FromBase64String(text.Split(' ')[1]);
              var decoded = System.Text.Encoding.UTF8.GetString(bytes);
              var keys = decoded.Split(' ');
              PrivateKey = new BigInteger(keys[0]);
              SharedKey = new BigInteger(keys[1]);
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
                  Filter = FilterZipFiles,
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
                  Directory.Delete(dirFileName, true); // Tidy up
                  return;
              }

              // Open extracted files
              try
              {
                  foreach (var fileName in fileNames)
                  {
                      if (fileName.Contains(".sign"))
                      {
                          var fileContent = File.ReadAllText(fileName);
                          var base64String = fileContent.Split(' ')[1];
                          var utf8 = Convert.FromBase64String(base64String);
                          var encrypted = System.Text.Encoding.UTF8.GetString(utf8);
                          SourceFileCopyDecryptedSignature = RSA.Decrypt(encrypted, publicKey, sharedKey);
                      }
                      else
                      {
                          SourceFileCopyInfo = new FileInfo(fileName);
                          SourceFileCopyHash = GetHashFromFile(fileName);
                      }
                  }
              }
              catch (ArgumentOutOfRangeException)
              {
                  TidyUpOnException(dirFileName).Invoke();
                  return;
              }
              catch (FormatException)
              {
                  TidyUpOnException(dirFileName).Invoke();
                  return;
              }
              catch (UnauthorizedAccessException)
              {
                  TidyUpOnException(dirFileName).Invoke();
                  return;
              }
              catch (NotSupportedException)
              {
                  TidyUpOnException(dirFileName).Invoke();
                  return;
              }

              // Verify source file copy
              bool isNotAltered = string.Compare(sourceFileCopyHash, sourceFileCopyDecryptedSignature) is 0;
              IsVerificationAlright = isNotAltered
                  ? VerificationSuccess
                  : VerificationFailure;

              Directory.Delete(dirFileName, true); // Tidy up
          }, () => true);

        private Action TidyUpOnException(string dirFileName)
        {
            return () =>
            {
                Directory.Delete(dirFileName, true); // Tidy up
                SourceFileCopyInfo = null;
                SourceFileCopyHash = string.Empty;
                SourceFileCopyDecryptedSignature = string.Empty;
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
              var encodedHash = string.Join(' ', msg, Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SourceFileHashEncrypted)));

              // Store signature and the copy of source file in .zip file
              File.WriteAllText(directoryPath + @"\signature.sign", encodedHash);
              File.Copy(SourceFileInfo.FullName, directoryPath + @"\" + SourceFileInfo.Name);
              ZipFile.CreateFromDirectory(directoryPath, path);
              Directory.Delete(directoryPath, true);
          }, () => true);

        public ICommand CommandGenerateHashFromSourceFile => new CommandHandler(() =>
          {
              SourceFileHash = GetHashFromFile(SourceFileInfo.FullName);
              SourceFileHashEncrypted = RSA.Encrypt(sourceFileHash, PrivateKey, SharedKey);
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