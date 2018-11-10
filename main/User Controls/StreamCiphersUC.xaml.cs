using CryptoCore.Core;
using System.Windows.Controls;

namespace CryptoCore.User_Controls
{
    /// <summary>
    /// Interaction logic for CryptoSystemsUC.xaml
    /// </summary>
    public partial class StreamCiphersUC : UserControl
    { 
        public StreamCiphersUC()
        {
            InitializeComponent();

            this.DataContext = new StreamCiphersViewModel();
        }
    }
}
