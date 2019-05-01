using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Org.BouncyCastle.Math;

namespace BlockchainApp.Source.Common
{
    public static class Constants
    {
        public static readonly Random _r = new Random();

        public const string Space = " ";
        public static string Dot = ".";
        public static string Comma = ",";
        public const string PszBase58 = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        public static string[] Numbers { get; } = "1234567890".ToArray().Select(c => c.ToString()).ToArray();
        public static string[] Operators { get; } = "+-/*".ToArray().Select(c => c.ToString()).ToArray();
        public static string[] NumbersAndOperators => Numbers.Concat(Operators).ToArray();

        public const double TOLERANCE = 0.00001;

        public static readonly object _globalSync = new object();

        public static readonly byte[] EmptyByteArray = new byte[0];
        public static readonly char[] PszBase58Chars = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();

        public static readonly Dictionary<FrameworkElement, Storyboard> StoryBoards = new Dictionary<FrameworkElement, Storyboard>();
        public static readonly Dictionary<string, bool> PanelAnimations = new Dictionary<string, bool>();

        public static readonly BigInteger Bn58 = BigInteger.ValueOf(58);

        public static readonly Action EmptyDelegate = delegate { };

        public static CultureInfo Culture = new CultureInfo("pl-PL")
        {
            NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." },
            DateTimeFormat = { ShortDatePattern = "dd-MM-yyyy" } // nie tworzyć nowego obiektu DateTimeFormat tutaj tylko przypisać jego interesujące nas właściwości, bo nowy obiekt nieokreślone właściwości zainicjalizuje wartościami dla InvariantCulture, czyli angielskie nazwy dni, miesięcy itd.
        };
    }
}
