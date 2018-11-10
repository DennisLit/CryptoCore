using CryptoCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CryptoCore.User_Controls
{
    /// <summary>
    /// Interaction logic for CryptoSystemsUC.xaml
    /// </summary>
    public partial class DigitalSignatureUC : UserControl
    {
        public DigitalSignatureUC()
        { 
            InitializeComponent();

            this.DataContext = new DigitalSignatureViewModel();
        }
    }
}
