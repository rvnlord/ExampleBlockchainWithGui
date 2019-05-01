﻿using System.Windows;
using System.Windows.Media;
using BlockchainApp.Source.Common.Extensions;
using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

namespace BlockchainApp.Source.Controls
{
    public partial class TilesMenuOption
    {
        public static readonly DependencyProperty TileNameDp = DependencyProperty.Register(nameof(TileName),
            typeof(string), typeof(TilesMenuOption), new UIPropertyMetadata(string.Empty));
        public string TileName
        {
            get => GetValue(TileNameDp).ToString();
            set // call manually on init cuz DepProp will bypass it
            {
                SetValue(TileNameDp, value); // this happens before TilesMainMenu_Loaded Event
            }
        }

        public Tile Tile { get; set; }

        public static readonly DependencyProperty DescriptionDp = DependencyProperty.Register(nameof(Description),
            typeof(string), typeof(TilesMenuOption), new UIPropertyMetadata(string.Empty));
        public string Description
        {
            get => GetValue(DescriptionDp).ToString();
            set => SetValue(DescriptionDp, value);
        }

        public static readonly DependencyProperty IconDp = DependencyProperty.Register(nameof(Icon),
            typeof(PackIconModernKind), typeof(TilesMenuOption), new UIPropertyMetadata(null));
        public PackIconModernKind Icon
        {
            get => (PackIconModernKind)GetValue(IconDp);
            set => SetValue(IconDp, value);
        }

        public static readonly DependencyProperty SelectedTileNameDp = DependencyProperty.Register(nameof(Selected),
            typeof(bool), typeof(TilesMenuOption), new UIPropertyMetadata(null));
        public bool Selected
        {
            get => (bool)GetValue(SelectedTileNameDp);
            set
            {
                Tile.Highlight(value ? MouseOverColor : MouseOutColor);
                SetValue(SelectedTileNameDp, value);
            }
        }

        public Color MouseOverColor { get; set; }
        public Color MouseOutColor { get; set; }
        //public int DefaultTileWidth { get; set; }
        //public int ResizeValue { get; set; }

        public TilesMenuOption()
        {
            Loaded += TilesMenuOption_Loaded;
            InitializeComponent();
        }

        public void TilesMenuOption_Loaded(object sender, RoutedEventArgs e) { }

        public override string ToString()
        {
            return $"{(Selected ? "(Selected) " : "" )}[{TileName}] {Description} | {Icon.EnumToString()}";
        }
    }
}
