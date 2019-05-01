namespace BlockchainApp.Source.Common.Utils.UtilClasses
{
    public interface INotifyPropertyChangedHelper
    {
        void SetPropertyAndNotify<T>(ref T field, T propVal, string propName);
    }
}
