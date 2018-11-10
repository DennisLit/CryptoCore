using CryptoCore.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CryptoCore.Data;
using System.Numerics;
using System.Threading.Tasks;
using CiphersLibrary.Algorithms;
using CiphersLibrary.Core;

namespace CryptoCore.Core
{

    public class CryptoSystemsViewModel : BaseViewModel
    {
        #region Ctor

        public CryptoSystemsViewModel()
        {
            InitCollections();

            RabinInstance = new RabinCS();

            YarmolikRabinInstance = new YarmolikRabinCS();
        }

        #endregion

        #region UI strings

        /// <summary>
        /// Represents text files to enrypt/ decipher
        /// </summary>
        public ObservableCollection<LoadedFileItem> FilesLoaded { get; set; }

        public static string[] ActionsList => new string[] { "Encrypt", "Decipher" };

        public static string[] CiphersList => new string[] { "Rabin", "Yarmolik-rabin" };

        public string ChosenCipher { get; set; } = "Rabin";

        public string ChosenAction { get; set; } = "Encrypt";

        public bool IsCompleted { get; set; } = true;

        public bool IsNoActionRunning { get; set; } = true;

        public string StateText { get; set; } = "Waiting for an action...";

        public string ReturnedText { get; set; } = "Output usually saves as a file with added Encrypted/Decrypted words. Also , result goes here...";

        public string CredentialsText { get; set; }
        #endregion

        #region Important parameters

        public string PublicKeyParam1 { get; set; }

        public string PublicKeyParam2 { get; set; }

        public string SecretKeyParam1 { get; set; }

        public string SecretKeyParam2 { get; set; }

        #endregion

        #region Flags
        /// <summary>
        /// Indicates if user've chosen to generate private keys automatically
        /// </summary>
        public bool IsKeysAutoGenerating { get; set; } = false;

        /// <summary>
        /// Indicates if current cipher should support unicode or not
        /// </summary>
        public bool IsUnicodeSupported { get; set; } = false;

        #endregion

        #region Misc
        /// <summary>
        /// Used to determine the file user've chosen
        /// </summary>
        public string ChosenFileId { get; set; }

        /// <summary>
        /// Index of selected cryptosystem
        /// </summary>
        public int SelectedCryptoSystemIndex { get; set; }

        /// <summary>
        /// Amount of chars in file name to display
        /// </summary>
        private readonly int maxCharsInName = 10;

        /// <summary>
        /// Default public key length in bits
        /// </summary>
        private readonly int defaultKeyLength = 128;

        private readonly int russianMaxUnicodeIndx = 2100;

        #endregion

        #region Algorithms instances

        private YarmolikRabinCS YarmolikRabinInstance { get; set; } 

        private RabinCS RabinInstance { get; set; }

        #endregion

        #region Commands

        public ICommand ChooseFileCommand { get { return new RelayCommandWithParam(ChooseFile); } }

        public ICommand WorkCommand { get { return new RelayCommand(WorkAsync); } }

        #endregion

        #region Main methods

        private async void WorkAsync()
        {
            IsNoActionRunning = false;

            try
            {


                #region Checks

                //Check whether file id is int

                if (!int.TryParse(ChosenFileId, out int FileId))
                {
                    IsCompleted = false;
                    StateText = "Wrong File id!";
                    return;
                }

                //Check whether file id is correct

                if ((FileId >= FilesLoaded.Count()) || (FileId < 0))
                {
                    IsCompleted = false;
                    StateText = "Wrong File id!";
                    return;
                }

                if (string.IsNullOrEmpty(ChosenAction))
                    return;

                string FilePath = string.Empty;

                //Load file path

                foreach (var item in FilesLoaded)
                {
                    if (item.Id == FileId)
                    {
                        FilePath = item.FileRealName;
                        break;
                    }
                }

                //if file id points to empty record

                if (string.IsNullOrWhiteSpace(FilePath))
                {
                    IsCompleted = false;
                    StateText = "File id is wrong.";
                    return;
                }

                // if file was deleted after loading

                if (!File.Exists(FilePath))
                {
                    IsCompleted = false;
                    StateText = "File doesnt exist.";
                    return;
                }

                #endregion

                #region Rabin
                //Rabin
                if (ChosenCipher == CiphersList[0])
                {

                    BigInteger p = 0;
                    BigInteger q = 0;

                    //Encrypt
                    if (ChosenAction == ActionsList[0])
                    {

                        //Check everything strictly because we are waiting data from user
                        // and not generating it
                        if (!IsKeysAutoGenerating)
                        {
                            #region data parsing

                            if (!BigInteger.TryParse(SecretKeyParam1, out p))
                            {
                                StateText = "Wrong first secret key parameter .";
                                IsCompleted = false;
                                return;
                            }

                            if (!BigInteger.TryParse(SecretKeyParam2, out q))
                            {
                                StateText = "Wrong second secret key parameter .";
                                IsCompleted = false;
                                return;
                            }

                            if ((!p.CheckIfPrime()) || (!q.CheckIfPrime()))
                            {
                                StateText = "One of two numbers was not prime";
                                IsCompleted = false;
                                return;
                            }

                            if ((p % 4 != 3) || (q % 4 != 3))
                            {
                                StateText = "p and q mod 4 must be equal to 3.";
                                return;
                            }

                            if (p == q)
                            {
                                StateText = "Secret key values must differ.";
                                IsCompleted = false;
                                return;
                            }

                            #endregion
                        }


                        //if user've chosen to generate keys automatically, generate them now.
                        if (IsKeysAutoGenerating)
                        {
                            #region Key generation

                            p = PrimeNumsGenerator.GeneratePrime(defaultKeyLength);
                            q = PrimeNumsGenerator.GeneratePrime(defaultKeyLength);

                            if (p < 0) { p = -p; }
                            if (q < 0) { q = -q; }

                            CredentialsText = "p : " + p.ToString()
                            + Environment.NewLine + "q: " + q.ToString()
                            + Environment.NewLine + "Public key is: " +
                            (p * q).ToString();

                            #endregion
                        }


                        bool result = false;
                        result = RabinInstance.Initialize(p, q);

                        if (!result)
                        {
                            StateText = "Check key and try again.";
                            IsCompleted = false;
                            return;
                        }

                        await Task.Run(() => result = RabinInstance.Encrypt(p, q, FilePath));

                        if(!result)
                        {
                            StateText = "Check key and try again.";
                            IsCompleted = false;
                            return;
                        }
                    }
                    //Decrypt
                    else
                    {
                        #region data parsing 

                        if (!BigInteger.TryParse(SecretKeyParam1, out p))
                        {
                            StateText = "Wrong first secret key parameter .";
                            IsCompleted = false;
                            return;
                        }

                        if (!BigInteger.TryParse(SecretKeyParam2, out q))
                        {
                            StateText = "Wrong second secret key parameter .";
                            IsCompleted = false;
                            return;
                        }

                        if ((p % 4 != 3) || (q % 4 != 3))
                        {
                            StateText = "p and q mod 4 must be equal to 3.";
                            return;
                        }

                        if (p == q)
                        {
                            StateText = "Secret key values must differ.";
                            IsCompleted = false;
                            return;
                        }

                        bool result = false;
                        result = RabinInstance.Initialize(p, q);

                        if (!result)
                        {
                            StateText = "Check key and try again.";
                            IsCompleted = false;
                            return;
                        }

                        #endregion

                        result = false;
                        await Task.Run(() => result = RabinInstance.Decrypt(p, q, FilePath));

                        if (!result)
                        {
                            StateText = "Check data you are decrypting and try again.";
                            IsCompleted = false;
                            return;
                        }
                    }
                }
                #endregion

                #region Yarmolik - Rabin
                //Yarmolik - Rabin
                else if (ChosenCipher == CiphersList[1])
                {                 


                    //Encrypt
                    if (ChosenAction == ActionsList[0])
                    {

                        if (!short.TryParse(PublicKeyParam1, out var publicKey))
                        {
                            #region Checks

                            // if public key was not provided, try parse private keys
                            if (!short.TryParse(SecretKeyParam1, out var p))
                            {
                                StateText = "Wrong first secret param.";
                                IsCompleted = false;
                                return;
                            }

                            if (!short.TryParse(SecretKeyParam2, out var q))
                            {
                                StateText = "Wrong second secret param.";
                                IsCompleted = false;
                                return;
                            }

                            if ((!p.CheckIfPrime()) || (!q.CheckIfPrime()))
                            {
                                StateText = "One of two numbers was not prime";
                                IsCompleted = false;
                                return;
                            }

                            if ((p % 4 != 3) || (q % 4 != 3))
                            {
                                StateText = "p and q mod 4 must be equal to 3.";
                                IsCompleted = false;
                                return;
                            }

                            if (p * q < Math.Pow(2, 8))
                            {
                                StateText = "p * q must be more than 1 byte.";
                                IsCompleted = false;
                                return;
                            }

                            if (!short.TryParse(PublicKeyParam2, out var caesarCoeff))
                            {
                                StateText = "Wrong caesar coefficient.";
                                IsCompleted = false;
                                return;
                            }

                            if (caesarCoeff >= p * q)
                            {
                                StateText = "Caesar coefficient must be less than public key.";
                                IsCompleted = false;
                                return;
                            }

                            if(p == q)
                            {
                                StateText = "Secret key values must differ.";
                                IsCompleted = false;
                                return;
                            }

                            #endregion

                            YarmolikRabinInstance.Encrypt(p, q, caesarCoeff, FilePath);
                        }
                        else
                        { // if public key was provided

                            #region Checks

                            if (!short.TryParse(PublicKeyParam2, out var caesarCoeff))
                            {
                                StateText = "Wrong caesar coefficient.";
                                IsCompleted = false;
                                return;
                            }

                            if (caesarCoeff >= publicKey)
                            {
                                StateText = "Caesar coefficient must be less than public key.";
                                IsCompleted = false;
                                return;
                            }

                            if ((IsUnicodeSupported) && (publicKey < russianMaxUnicodeIndx))
                            {
                                StateText = $"To support unicode symbols, provide a public key larger than {russianMaxUnicodeIndx}";
                                IsCompleted = false;
                                return;
                            }

                            #endregion

                            YarmolikRabinInstance.Encrypt(publicKey, caesarCoeff, FilePath);
                        }


                    }
                    //Decrypt
                    else
                    {
                        #region Checks

                        if (!short.TryParse(SecretKeyParam1, out var p))
                        {
                            StateText = "Wrong first secret param.";
                            IsCompleted = false;
                            return;
                        }

                        if (!short.TryParse(SecretKeyParam2, out var q))
                        {
                            StateText = "Wrong second secret param.";
                            IsCompleted = false;
                            return;
                        }

                        if ((!p.CheckIfPrime()) || (!q.CheckIfPrime()))
                        {
                            StateText = "One of two numbers was not prime";
                            IsCompleted = false;
                            return;
                        }

                        if ((p % 4 != 3) || (q % 4 != 3))
                        {
                            StateText = "p and q mod 4 must be equal to 3.";
                            IsCompleted = false;
                            return;
                        }

                        if (p * q < Math.Pow(2, 8))
                        {
                            StateText = "p * q must be more than 1 byte.";
                            IsCompleted = false;
                            return;
                        }

                        if (!short.TryParse(PublicKeyParam2, out var caesarCoeff))
                        {
                            StateText = "Wrong caesar coefficient.";
                            IsCompleted = false;
                            return;
                        }

                        if (caesarCoeff >= p * q)
                        {
                            StateText = "Caesar coefficient must be less than public key.";
                            IsCompleted = false;
                            return;
                        }

                        if (p == q)
                        {
                            StateText = "Secret key values must differ.";
                            IsCompleted = false;
                            return;
                        }


                        #endregion

                        await Task.Run(() => YarmolikRabinInstance.Decrypt(p, q, caesarCoeff, FilePath));
                    }
                }
                #endregion

                StateText = "Action completed.";

                IsCompleted = true;


            }
            catch(ArgumentException ex)
            {
                StateText = ex.Message;
                IsCompleted = false;
            }
            catch (OutOfMemoryException)
            {
                StateText = "Memory limit was exceeded.";
                IsCompleted = false;
            }
            finally
            { IsNoActionRunning = true; }

        }

        private void ChooseFile(Object ChosenFile)
        {
            var chosenFile = ChosenFile as LoadedFileItem;

            OpenFileDialog file = new OpenFileDialog();

            file.ShowDialog();

            if (string.IsNullOrEmpty(file.FileName))
            {
                IsCompleted = false;
                StateText = "There was a problem loading that file!";
                return;
            }

            //check if this file was loaded before

            foreach (var item in FilesLoaded)
            {
                if (item.FileRealName == file.FileName)
                {
                    IsCompleted = false;
                    StateText = "File is already loaded !";
                    return;
                }
            }

            //Update File info

            foreach (var item in FilesLoaded)
            {
                if (item.Id == chosenFile.Id)
                {
                    item.IsLoaded = true;
                    item.FileFixedName = file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1).Truncate(maxCharsInName);
                    item.FileRealName = file.FileName;
                    IsCompleted = true;
                    StateText = "Successfully loaded.";
                    return;
                }
            }

        }
        #endregion

        #region Helper methods


        private void InitCollections()
        {
            FilesLoaded = new ObservableCollection<LoadedFileItem>();

            FilesLoaded.Add(new LoadedFileItem()
            {
                Id = 0,
                FileFixedName = "Not loaded...",
                IsLoaded = false
            });

            FilesLoaded.Add(new LoadedFileItem()
            {
                Id = 1,
                FileFixedName = "Not loaded...",
                IsLoaded = false
            });

            FilesLoaded.Add(new LoadedFileItem()
            {
                Id = 2,
                FileFixedName = "Not loaded...",
                IsLoaded = false
            });

            FilesLoaded.Add(new LoadedFileItem()
            {
                Id = 3,
                FileFixedName = "Not loaded...",
                IsLoaded = false
            });

        }

        #endregion

    }
}
