using System.Windows;
using BlockchainApp.Source.Common.Utils;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class WindowExtensions
    {
        public static Window CenterOnScreen(this Window wnd)
        {
            wnd.Position(PointUtils.CenteredWindowTopLeft(wnd.Size()));
            return wnd;
        }

        public static WPFScreen Screen(this Window wnd)
        {
            return WPFScreen.GetScreenFrom(wnd);
        }

        public static Point WindowPointToScreen(this Window wnd, Point p)
        {
            var screenMousePos = wnd.PointToScreen(p);
            var screen = wnd.Screen().DeviceBounds;
            var screenWidth = screen.Width;
            var screenHeight = screen.Height;
            var screenDPIWidth = SystemParameters.FullPrimaryScreenWidth;
            var screenDPIHeight = SystemParameters.FullPrimaryScreenHeight;

            return new Point(
                screenDPIWidth / (screenWidth / screenMousePos.X),
                screenDPIHeight / (screenHeight / screenMousePos.Y));
        }

        public static Window SizeToContentAndUnlock(this Window wnd)
        {
            wnd.SizeToContent = SizeToContent.WidthAndHeight;
            wnd.SizeToContent = SizeToContent.Manual;
            return wnd;
        }

        public static Window SizeToContentAndKeep(this Window wnd)
        {
            wnd.SizeToContent = SizeToContent.WidthAndHeight;
            return wnd;
        }

        public static LoaderSpinnerWrapper GetLoader(this Window wnd)
        {
            return AsyncUtils.GetLoader(wnd);
        }

        public static void SetLoaderStatus(this Window wnd, string status)
        {
            AsyncUtils.SetLoaderStatus(wnd, status);
        }
    }
}
