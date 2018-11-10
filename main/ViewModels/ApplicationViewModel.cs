using System.Windows.Input;
using CryptoCore.Helpers;
using CryptoCore.Data;
using System.Collections.Generic;

namespace CryptoCore.Core
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Ctor

        public ApplicationViewModel()
        {
            InitializeAppPages();
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Pages in application
        /// </summary>
        public List<ApplicationPageItem> ApplicationPages { get; set; }

        /// <summary>
        /// Index of chosen page
        /// </summary>
        public int CurrentApplicationPage { get; set; }

        #endregion

        #region Commands

        public ICommand ChangeAppPageCommand => new RelayCommandWithParam(ChangeAppPage);

        #endregion

        #region Command methods

        /// <summary>
        /// Changes app page to specific page
        /// </summary>
        /// <param name="pageObject"></param>
        private void ChangeAppPage(object pageObject)
        {
            CurrentApplicationPage = ((ApplicationPageItem)pageObject).Id;
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Initializes the pages of application
        /// </summary>
        private void InitializeAppPages()
        {
            ApplicationPages = new List<ApplicationPageItem>();

            ApplicationPages.Add(new ApplicationPageItem() { ApplicationPageName = "Home", IconName = "HomeOutline", Id = (int)ApplicationPage.GreetPage });

            ApplicationPages.Add(new ApplicationPageItem() { ApplicationPageName = "Simple ciphers", IconName = "Autofix", Id = (int)ApplicationPage.SimpleCiphers });

            ApplicationPages.Add(new ApplicationPageItem() { ApplicationPageName = "Stream ciphers", IconName = "ViewStream", Id = (int)ApplicationPage.StreamCiphers });

            ApplicationPages.Add(new ApplicationPageItem() { ApplicationPageName = "Cryptosystems", IconName = "Buffer", Id = (int)ApplicationPage.CryptoSystems });

            ApplicationPages.Add(new ApplicationPageItem() { ApplicationPageName = "Digital signature", IconName = "Pen", Id = (int)ApplicationPage.DigitalSignature });
        }

        #endregion
    }
}
