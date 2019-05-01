using System.Windows;
using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class PointUtils
    {
        public static Point ScreenCenter()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            return new Point(screenWidth / 2, screenHeight / 2);
        }

        public static Point CenteredWindowTopLeft(Size size)
        {
            var center = ScreenCenter();
            return new Point(center.X - size.Width / 2, center.Y - size.Height / 2);
        }

        public static Point CenteredWindowTopLeft(System.Drawing.Size size)
        {
            return CenteredWindowTopLeft(size.ToSize());
        }
    }

}
