using System.ComponentModel;
using System.Windows.Controls;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Controls
{
    public abstract class UserControlWithNotifyProperty : UserControl, INotifyPropertyChanged, INotifyPropertyChangedHelper
    {
        public void SetPropertyAndNotify<T>(ref T field, T propVal, string propName)
        {
            if (Equals(field, propVal)) return;
            field = propVal;
            OnPropertyChanging(propName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanging(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
