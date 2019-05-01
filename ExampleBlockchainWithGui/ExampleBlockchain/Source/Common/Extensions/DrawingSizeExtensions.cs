using System.Windows;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class DrawingSizeExtensions
    {
        public static Size ToSize(this System.Drawing.Size s)
        {
            return new Size(s.Width, s.Height);
        }
    }
}
