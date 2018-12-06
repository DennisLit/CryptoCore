using CryptoCore.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CryptoCore.Data;
using CiphersLibrary.Data;
using CiphersLibrary.Core;
using System.Security.Cryptography;

namespace CryptoCore.Core
{ 
    public class SimpleCiphersViewModel : BaseViewModel
    {

        public SimpleCiphersViewModel()
        {
            InitCollections();
        }

        #region Fields

        private readonly int MaxCharsInName = 10;

        /// <summary>
        /// Represents text files to enrypt/ decipher
        /// </summary>
        public ObservableCollection<LoadedFileItem> FilesLoaded { get; set; }

        public static string[] ActionsList => new string[] { "Encrypt", "Decipher" };

        public static string[] AlphabetNamesList => new string[] { "English", "Russian" };

        public static string[] CiphersList => new string[] { "Railfence", "Columnar transposition", "Vigenere" };

        public string ChosenCipher { get; set; } = "Railfence";

        public string ChosenAlphabet { get; set; } = "English";

        public string ChosenAction { get; set; } = "Encrypt";

        public bool IsCompleted { get; set; } = true;

        public bool IsNoActionRunning { get; set; } = true;

        public string StateText { get; set; } = "Waiting for an action...";

        public string ReturnedText { get; set; } = "Output usually saves as a file with added Encrypted/Decrypted words. Also , result goes here...";

        /// <summary>
        /// Used to get additional information for the cipher(key word, fence height, etc.)
        /// </summary>
        public string KeyValue { get; set; }

        /// <summary>
        /// Used to determine the file user've chosen
        /// </summary>
        public string ChosenFileId { get; set; }

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

                var AlphabetInCiphers = string.Empty;

                //Get chosen Alphabet

                if (ChosenAlphabet == AlphabetNamesList[0])
                    AlphabetInCiphers = ResourceStrings.EngCapital;
                else
                    if (ChosenAlphabet == AlphabetNamesList[1])
                    AlphabetInCiphers = ResourceStrings.RuCapital;

                var ChosenCipherType = new SimpleCiphers();

                //Find Chosen Algorithm

                for (int i = 0; i < CiphersList.Count(); ++i)
                {
                    if (CiphersList[i] == ChosenCipher)
                    {
                        ChosenCipherType = (SimpleCiphers)i;
                    }
                }

                var Algorithm = new SimpleCipheringAlgorithmFactory().NewAlgorithm(ChosenCipherType);

                Algorithm.Initialize(KeyValue, AlphabetInCiphers);

                var chosenActionType = new Operations();

                for (int i = 0; i < ActionsList.Count(); ++i)
                {
                    if (ActionsList[i] == ChosenAction)
                    {
                        chosenActionType = (Operations)i;
                    }
                }

                var TextToWorkWith = File.ReadAllText(FilePath).ToUpper();

                TextToWorkWith = GetFixedString(ref TextToWorkWith, AlphabetInCiphers, chosenActionType);

                //Encrypt
                if (chosenActionType == Operations.Encrypt)
                {
                    ReturnedText = Algorithm.Encrypt(TextToWorkWith);
                    File.WriteAllText(FilePath.Insert(FilePath.IndexOf("."), "Encrypted"), ReturnedText);
                    StateText = "Successfully created text file " + Path.GetFileName(FilePath);
                }
                else//Decipher
                {
                    ReturnedText = Algorithm.Decipher(TextToWorkWith);
                    File.WriteAllText(FilePath.Insert(FilePath.IndexOf("."), "Decrypted"), ReturnedText);
                    StateText = "Successfully created text file " + Path.GetFileName(FilePath);
                }

                IsCompleted = true;

            }
            catch(ArgumentException ex)
            {
                StateText = ex.Message;
                IsCompleted = false;
            }

            finally
            { IsNoActionRunning = true; }

        }

        #endregion

        #region Helper methods


        private void ChooseFile(Object ChosenFile)
        {
            var chosenFile = ChosenFile as LoadedFileItem;

            OpenFileDialog file = new OpenFileDialog();

            file.ShowDialog();

            if (string.IsNullOrEmpty(file.FileName) || (!CheckIfRightFile(file.FileName)))
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
                    item.FileFixedName = file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1).Truncate(MaxCharsInName);
                    item.FileRealName = file.FileName;
                    IsCompleted = true;
                    StateText = "Successfully loaded.";
                    return;
                }
            }

        }
        

        /// <summary>
        /// Method for fixing a string to proceed to encrypt/decrypt operations(remove unneccessary symbols)
        /// </summary>
        /// <param name="stringToFix"></param>
        /// <param name="Alphabet"></param>
        /// <returns></returns>
        private string GetFixedString(ref string stringToFix, string Alphabet, Operations actionKind)
        {
            var Output = string.Empty;

            for (int i = 0; i < stringToFix.Length; ++i)
            {
                if (Alphabet.Contains(stringToFix[i]))
                    Output += stringToFix[i];

                if ((actionKind == Operations.Decipher) && (stringToFix[i] == ' '))
                    Output += stringToFix[i];
            }

            return Output;
        }
        /// <summary>
        /// Helper method to check the 
        /// contents of the file and file extension
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>if the file is Right returns true, either returns false </returns>
        private bool CheckIfRightFile(string fileName)
        {
            if (Path.GetExtension(fileName) != ".txt")
                return false;

            string[] fileLines = File.ReadAllLines(fileName);

            if (fileLines.Count() == 0)
                return false;

            return true;
        }

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
