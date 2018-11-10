using PropertyChanged;
using System.ComponentModel;

namespace CryptoCore.Core
{ 
    [AddINotifyPropertyChangedInterface]

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
    }
}

