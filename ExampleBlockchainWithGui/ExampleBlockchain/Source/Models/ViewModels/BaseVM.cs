using System.ComponentModel;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Models.ViewModels
{
    public abstract class BaseVM : INotifyPropertyChanged, INotifyPropertyChangedHelper
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
