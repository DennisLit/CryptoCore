using CryptoCore.Helpers;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using CryptoCore.Data;
using CiphersLibrary.Core;
using CiphersLibrary.Algorithms;
using CiphersLibrary.Data;

namespace CryptoCore.Core
{
    public class StreamCiphersViewModel : BaseViewModel
    { 
        #region Ctor
         
        public StreamCiphersViewModel()
        { 
            Init();
        }

        #endregion

        #region UI strings

        /// <summary>
        /// Represents  files to enrypt/ decipher
        /// </summary>
        public ObservableCollection<LoadedFileItem> FilesLoaded { get; set; }

        public static string[] KeyGenerationMethodsList => new string[] { KeyGenerators.LFSR.ToString(), KeyGenerators.Geffe.ToString(), KeyGenerators.RC4.ToString() };
         
        public string ChosenKeyGenerator { get; set; } = KeyGenerators.LFSR.ToString();

        public bool IsCompleted { get; set; } = true;

        public bool IsNoActionRunning { get; set; } = true;

        public string StateText { get; set; } = "Waiting for an action...";

        public int CurrentProgressValue { get; set; }

        public string InitialState { get; set; }
        #endregion

        #region Progress info

        private Progress<ProgressChangedEventArgs> progress { get; set; }

        #endregion

        #region Misc
        /// <summary>
        /// Used to determine the file user've chosen
        /// </summary>
        public string ChosenFileId { get; set; }

        /// <summary>
        /// UI string showing generated key if user 
        /// </summary>
        public string GeneratedKey { get; set; }

        /// <summary>
        /// Amount of chars in file name to display
        /// </summary>
        private readonly int maxCharsInName = 10;


        #endregion

        #region Commands

        public ICommand ChooseFileCommand { get { return new RelayCommandWithParam(ChooseFile); } }

        public ICommand WorkCommand { get { return new RelayCommand(WorkAsync); } }

        #endregion

        #region Flags

        public bool IsAutoGeneratingKeys { get; set; }

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

                Enum.TryParse<KeyGenerators>(ChosenKeyGenerator, out var chosenKeyGenerationMethod);

                var keyGeneratorInstance = new KeyGeneratorsFactory().NewKeyGenerator(chosenKeyGenerationMethod);

                var cipher = new StreamCipher();

                cipher.Initialize(keyGeneratorInstance);

                if(IsAutoGeneratingKeys)
                {
                    var generatedKey = RandomNumbersGenerator.GenerateBinaryString(LFSR.DefaultHighestPower);
                    GeneratedKey = $"Your generated key is : {generatedKey}";
                    await cipher.Encrypt(FilePath, generatedKey, progress);
                }
                else { await cipher.Encrypt(FilePath, InitialState, progress); }
                             

                StateText = "Action completed.";

                IsCompleted = true;

            }
            catch(ArgumentException ex)
            {
                StateText = ex.Message;
                IsCompleted = false;
            }
            catch(OutOfMemoryException ex)
            {
                StateText = ex.Message;
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

        private void Init()
        {
            progress = new Progress<ProgressChangedEventArgs>();

            progress.ProgressChanged += ProgressChanged;

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

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.CurrentProgressValue = e.value;
        }


        #endregion

    }
}
