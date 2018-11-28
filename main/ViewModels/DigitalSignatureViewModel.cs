using CryptoCore.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CryptoCore.Data;
using CiphersLibrary.Algorithms;
using System.Threading.Tasks;
using CiphersLibrary.Data;
using System.Numerics;
using System.Collections.Generic;
using CiphersLibrary.Core;
using System.Security.Cryptography;

namespace CryptoCore.Core
{

    public class DigitalSignatureViewModel : BaseViewModel
    {
        #region Ctor

        public DigitalSignatureViewModel()
        {
            InitCollections();
        }

        #endregion

        #region UI strings

        /// <summary>
        /// Represents text files to enrypt/ decipher
        /// </summary>
        public ObservableCollection<LoadedFileItem> FilesLoaded { get; set; }

        public static string[] ActionsList => new string[] { "Sign a message", "Signature check" };
         
        public static string[] SignatureTypeList => new string[] { "RSA default", "RSA SHA-1"};

        public string ChosenSignature { get; set; } = "RSA default";
         
        public string ChosenAction { get; set; } = "Sign a message";

        public bool IsCompleted { get; set; } = true;

        public bool IsNoActionRunning { get; set; } = true;

        public string StateText { get; set; } = "Waiting for an action...";

        public string ReturnedText { get; set; } = "Output usually saves as a file with added Encrypted/Decrypted words. Also , result goes here...";

        public string OutputText { get; set; }
        #endregion
         
        #region Important parameters

        public string PublicKeyParam1 { get; set; }

        public string PublicKeyParam2 { get; set; }

        public string SecretKey { get; set; }
        
        private Dictionary<string, HashFunction> HashFunctionsDict { get; set; }

        #endregion

        #region Flags

        public bool IsKeysAutoGenerating { get; set; }

        public bool IsCorrectnessChecksEnabled { get; set; }

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


        #endregion

        #region Commands

        public ICommand ChooseFileCommand { get { return new RelayCommandWithParam(ChooseFile); } }

        public ICommand WorkCommand { get { return new RelayCommand(Work); } }

        #endregion

        #region Main methods

        private void Work()
        { 
            IsNoActionRunning = false;

            try
            {
                #region Checks for chosen file

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

                if(Path.GetExtension(FilePath) != ".txt")
                {
                    IsCompleted = false;
                    StateText = "Text files should be used only...";
                    return;
                }

                #endregion

                var hashFuncChosen = HashFunctionsDict[ChosenSignature];

                BigInteger p = 0, q = 0, secretKey = 0;

                if(!IsKeysAutoGenerating)
                {
                    #region Checks for user input

                    if (!BigInteger.TryParse(PublicKeyParam1, out p))
                    {
                        IsCompleted = false;
                        StateText = "Wrong p value";
                        return;
                    }


                    if (!BigInteger.TryParse(PublicKeyParam2, out q))
                    {
                        IsCompleted = false;
                        StateText = "Wrong q value";
                        return;
                    }

                    if (!BigInteger.TryParse(SecretKey, out secretKey))
                    {
                        IsCompleted = false;
                        StateText = "Wrong secret key value";
                        return;
                    }

                    #endregion
                }
                else
                { //if secret key couldn't meet the requerements here, should regenerate the numbers.
                    p = RandomNumbersGenerator.GeneratePrime(15 * 8);
                    OutputText += $"P : {p.ToString()} "; 
                    q = RandomNumbersGenerator.GeneratePrime(15 * 8);
                    OutputText += Environment.NewLine + $"Q : {q.ToString()} ";
                    secretKey = RandomNumbersGenerator.GeneratePrime(15 * 8);
                    OutputText += Environment.NewLine + $"Secret Key : {secretKey.ToString()} ";
                }


                var signatureInstance = new RsaSignature(p, q, secretKey, hashFuncChosen, IsCorrectnessChecksEnabled);

                if (ChosenAction == "Sign a message")
                {
                    OutputText += Environment.NewLine + $"Result Is: {signatureInstance.Sign(FilePath).ToString()}";
                }                 
                    
                if(ChosenAction == "Signature check")
                {
                                            
                    if (signatureInstance.CheckSignature(FilePath))
                    {
                        OutputText = "Digital signature is right!";
                    }
                    else
                    {
                        OutputText = "Digital signature is corrupted!";
                    }

                }             

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
            catch (IOException IoExc)
            {
                StateText = IoExc.Message;
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
            FilesLoaded = new ObservableCollection<LoadedFileItem>
            {
                new LoadedFileItem()
                {
                    Id = 0,
                    FileFixedName = "Not loaded...",
                    IsLoaded = false
                },

                new LoadedFileItem()
                {
                    Id = 1,
                    FileFixedName = "Not loaded...",
                    IsLoaded = false
                },

                new LoadedFileItem()
                {
                    Id = 2,
                    FileFixedName = "Not loaded...",
                    IsLoaded = false
                },

                new LoadedFileItem()
                {
                    Id = 3,
                    FileFixedName = "Not loaded...",
                    IsLoaded = false
                }
            };

            HashFunctionsDict = new Dictionary<string, HashFunction>
            {
                { "RSA default", HashFunction.YarmolikHash },
                { "RSA SHA-1", HashFunction.Sha1 }
            };

        }


        #endregion

    }
}
