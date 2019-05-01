using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;

namespace BlockchainApp.Source.Common.Utils.UtilClasses
{
    public class WPFScreen
    {
        public static IEnumerable<WPFScreen> AllScreens()
        {
            return Screen.AllScreens.Select(screen => new WPFScreen(screen));
        }

        public static WPFScreen GetScreenFrom(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            var screen = Screen.FromHandle(windowInteropHelper.Handle);
            var wpfScreen = new WPFScreen(screen);
            return wpfScreen;
        }

        public static WPFScreen GetScreenFrom(Point point)
        {
            var x = (int)Math.Round(point.X);
            var y = (int)Math.Round(point.Y);

            var drawingPoint = new System.Drawing.Point(x, y);
            var screen = Screen.FromPoint(drawingPoint);
            var wpfScreen = new WPFScreen(screen);

            return wpfScreen;
        }

        public static WPFScreen Primary => new WPFScreen(Screen.PrimaryScreen);

        private readonly Screen screen;

        internal WPFScreen(Screen screen)
        {
            this.screen = screen;
        }

        public Rect DeviceBounds => GetRect(screen.Bounds);

        public Rect WorkingArea => GetRect(screen.WorkingArea);

        private static Rect GetRect(Rectangle value)
        {
            return new Rect
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }

        public bool IsPrimary => screen.Primary;

        public string DeviceName => screen.DeviceName;
    }
}