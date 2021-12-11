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
    /// <summary>
    /// Main Window View Model.
    /// </summary>
    public sealed class MainWindowViewModel : INotifyPropertyChanged
    {
        #region FIELDS
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
        private bool areKeyPairsSet;
        private bool isSourceFileOpened;
        private bool isSignatureGenerated;
        #endregion
        #region PROPERTIES
        /// <summary>
        /// Part of the public key pair used for signature encryption.
        /// </summary>
        public BigInteger PublicKey { get => publicKey; set => SetProperty(ref publicKey, value); }

        /// <summary>
        /// Part of the private key pair used for signature decryption.
        /// </summary>
        public BigInteger PrivateKey { get => privateKey; set => SetProperty(ref privateKey, value); }

        /// <summary>
        /// Shared part of key pairs used for encryption and decryption.
        /// </summary>
        public BigInteger SharedKey { get => sharedKey; set => SetProperty(ref sharedKey, value); }

        /// <summary>
        /// General information about source file.
        /// </summary>
        public FileInfo? SourceFileInfo { get => sourceFileInfo; private set => SetProperty(ref sourceFileInfo, value); }

        /// <summary>
        /// General information about source file copy.
        /// </summary>
        public FileInfo? SourceFileCopyInfo { get => sourceFileCopyInfo; private set => SetProperty(ref sourceFileCopyInfo, value); }

        /// <summary>
        /// Source file's unique hash string used for file verification.
        /// </summary>
        public string SourceFileHash { get => sourceFileHash; private set => SetProperty(ref sourceFileHash, value); }

        /// <summary>
        /// Encrypted source file's hash string using RSA encryption algorithm.
        /// </summary>
        public string SourceFileHashEncrypted { get => sourceFileHashEncrypted; private set => SetProperty(ref sourceFileHashEncrypted, value); }

        /// <summary>
        /// Source file copy's unique hash used for file verification.
        /// </summary>
        public string SourceFileCopyHash { get => sourceFileCopyHash; private set => SetProperty(ref sourceFileCopyHash, value); }

        /// <summary>
        /// Decrypted signature of original source file used for file verification.
        /// </summary>
        public string SourceFileCopyDecryptedSignature { get => sourceFileCopyDecryptedSignature; private set => SetProperty(ref sourceFileCopyDecryptedSignature, value); }

        /// <summary>
        /// Message indicating whether the verification was successful or not.
        /// </summary>
        public string IsVerificationAlright { get => isVerificationAlright; private set => SetProperty(ref isVerificationAlright, value); }

        /// <summary>
        /// GUI component controller used for enabling and disabling the control.
        /// </summary>
        public bool IsSignatureGenerated { get => isSignatureGenerated; private set => SetProperty(ref isSignatureGenerated, value); }

        /// <summary>
        /// GUI component controller used for enabling and disabling the control.
        /// </summary>
        public bool IsSourceFileOpened { get => isSourceFileOpened; private set => SetProperty(ref isSourceFileOpened, value); }

        /// <summary>
        /// GUI component controller used for enabling and disabling the control.
        /// </summary>
        public bool AreKeyPairsSet { get => areKeyPairsSet; private set => SetProperty(ref areKeyPairsSet, value); }

        /// <summary>
        /// Displayed message whenever exception occurs.
        /// </summary>
        public string Message { get => message; set => SetProperty(ref message, value); }

        /// <summary>
        /// Event handler used for updating view's data.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion
        #region COMMANDS
        /// <summary>
        /// Initiates key generation via key pairs generator method.
        /// </summary>
        public ICommand CommandGenerateKeypPairs => new CommandHandler(() => (PublicKey, PrivateKey, SharedKey) = RSA.GenerateKeyPairs(), () => true);

        /// <summary>
        /// Writes public key pair to *.pub file and saves it to specific folder destination.
        /// </summary>
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

        /// <summary>
        /// Reads public key pair from *.pub file and writes it into memory.
        /// </summary>
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

        /// <summary>
        /// Writes private key pair to *.priv file and saves it to specific folder destination.
        /// </summary>
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

        /// <summary>
        /// Reads private key pair from *.priv file and writes it into memory.
        /// </summary>
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

        /// <summary>
        /// Reads source file and writes general information about file into memory.
        /// </summary>
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

        /// <summary>
        /// Opens *.zip file and verifies contained source file using packed encrypted signature file.
        /// </summary>
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

        /// <summary>
        /// Sets default value to properties upon rising exception.
        /// </summary>
        /// <param name="dirFileName">Path to the temporary directory to be removed from the guest's system.</param>
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

        /// <summary>
        /// Exports encrypted signature and source file copy as *.zip file to specific folder destination.
        /// </summary>
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

        /// <summary>
        /// Initiates hash generation from file.
        /// </summary>
        public ICommand CommandGenerateHashFromSourceFile => new CommandHandler(() =>
          {
              SourceFileHash = GetHashFromFile(SourceFileInfo.FullName);
              SourceFileHashEncrypted = RSA.Encrypt(sourceFileHash, PrivateKey, SharedKey);
          }, () => true);
        #endregion
        #region METHODS
        /// <summary>
        /// Sets value to property and invokes PropertyChanged event handler.
        /// </summary>
        /// <typeparam name="T">Generic type.</typeparam>
        /// <param name="store">Reference to the property's field.</param>
        /// <param name="value">Value to be set.</param>
        /// <param name="name">Calling property's name.</param>
        public void SetProperty<T>(ref T store, T value, [CallerMemberName] string name = null)
        {
            if (Equals(store, value) is false)
            {
                store = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Generates hash from file using SHA3-512 Keccak algorithm.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>File's hash.</returns>
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
        #endregion
    }
}