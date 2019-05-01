using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using BlockchainApp.Source.Common.Utils.UtilClasses;
using BlockchainApp.Source.Constrollers;
using BlockchainApp.Source.Controls;
using BlockchainApp.Source.Models.ViewModels;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MoreLinq;
using NLog;
using static BlockchainApp.Source.Common.Utils.DeferredControlsUpdateUtils;
using static BlockchainApp.Source.Config;
using Button = System.Windows.Controls.Button;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Panel = System.Windows.Controls.Panel;
using TextBox = System.Windows.Controls.TextBox;
using ComboBox = System.Windows.Controls.ComboBox;
using ListBox = System.Windows.Controls.ListBox;

namespace BlockchainApp.Source.WIndows
{
    public partial class MainWindow : MetroWindow
    {
        private Color _mouseOverBlueColor;
        private Color _defaultBlueColor;
        private Color _defaultWindowColor;
        private NotifyIcon _notifyIcon;

        private readonly ObservableCollection<TransactionGvVM> _ocTransactions = new ObservableCollection<TransactionGvVM>(); // deferred until grid is visible
        private readonly ObservableCollection<TransactionGvVM> _ocSelectedTransactions = new ObservableCollection<TransactionGvVM>();
        private readonly ObservableCollection<BlockGvVM> _ocBlocks = new ObservableCollection<BlockGvVM>(); // deferred until grid is visible
        private readonly ObservableCollection<BlockGvVM> _ocSelectedBlocks = new ObservableCollection<BlockGvVM>();



        public MainWindow()
        {
            InitializeComponent();
            Loaded += mainWindow_Loaded;
        }

        #region Events

        #region - Window Events

        private async void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                LoggerUtils.Close();
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                _logger.Error(ex);
                await this.ShowMessageAsync("Wystąpił Błąd", ex.Message);
            }
        }

        #endregion

        #region - Button Events

        private void btnSizeToContent_Click(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
            SizeToContent = SizeToContent.Manual;
        }

        private void btnSizeToContent_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = new SolidColorBrush(_mouseOverBlueColor);
        }

        private void btnSizeToContent_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = Brushes.Transparent;
        }

        private void btnMinimizeToTray_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            ShowInTaskbar = false;
            _notifyIcon.Visible = true;
            _notifyIcon.ShowBalloonTip(1500);
        }

        private void btnMinimizeToTray_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = new SolidColorBrush(_mouseOverBlueColor);
        }

        private void btnMinimizeToTray_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = Brushes.Transparent;
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMinimize_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(76, 76, 76));
        }

        private void btnMinimize_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = Brushes.Transparent;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnClose_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = new SolidColorBrush(Color.FromRgb(76, 76, 76));
            ((Button)sender).Foreground = Brushes.Black;
        }

        private void btnClose_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = Brushes.Transparent;
            ((Button)sender).Foreground = Brushes.White;
        }

        #endregion

        #region - Notifyicon Events

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            _notifyIcon.Visible = false;
            WindowState = WindowState.Normal;

            if (IsVisible)
                Activate();
            else
                Show();
        }

        #endregion

        #region - Grid Events 

        private bool _restoreForDragMove;
        
        private void gridTitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (ResizeMode != ResizeMode.CanResize && ResizeMode != ResizeMode.CanResizeWithGrip)
                    return;

                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                _restoreForDragMove = WindowState == WindowState.Maximized;
                DragMove();
            }
        }

        private void gridTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (_restoreForDragMove && e.LeftButton == MouseButtonState.Pressed)
            {
                _restoreForDragMove = false;

                var wndMousePos = e.MouseDevice.GetPosition(this);
                var screenMousePos = this.WindowPointToScreen(wndMousePos);

                Left = screenMousePos.X - Width / (ActualWidth / wndMousePos.X);
                Top = screenMousePos.Y - Height / (ActualHeight / wndMousePos.Y);

                WindowState = WindowState.Normal;

                DragMove();
            }
        }

        private void gridTitleBar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _restoreForDragMove = false;
        }

        private void gridTitleBar_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Highlight(_defaultBlueColor);
        }

        private void gridTitleBar_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Grid)sender).Highlight(_defaultWindowColor);
        }

        #endregion

        #region - Textbox Events

        private static void TxtAll_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.ResetValue();
            (sender as TextBox)?.ClearValue();
        }

        private static void TxtAll_LostFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.ResetValue();
        }

        #endregion

        #region - Tile Events

        private async void tmMainMenu_MenuTileClick(object sender, MenuTileClickedEventArgs e)
        {
            if (e.TileClicked == e.PreviouslySelectedTile)
            {
                tmMainMenu.SelectedTile = e.PreviouslySelectedTile;
                return;
            }

            var name = e.TileClicked.Name.AfterFirst("tl");
            var parentGrid = ((TilesMenu)sender).LogicalAncestor<StackPanel>();
            var mainTabs = parentGrid.LogicalDescendants<Grid>().Where(g => g.Name.EndsWith("MainTab")).ToArray();
            var gridToShow = mainTabs.SingleOrDefault(g => g.Name.Between("grid", "MainTab") == name);

            if (gridToShow != null)
            {
                mainTabs.Except(gridToShow).ForEach(g => g.Visibility = Visibility.Collapsed);
                gridToShow.Visibility = Visibility.Visible;

                HandleMainTabChange(gridToShow);
            }
            else
                await this.ShowMessageAsync("Wystąpił Błąd", $"There is no content for chosen menu option ({name}).");
        }

        #endregion

        #endregion

        #region Setups

        private void SelectDefaultMainTab()
        {
            var tabs = this.LogicalDescendants<Grid>().Where(g => g.Name.EndsWith("MainTab")).ToArray();
            tabs.ForEach(t => t.Visibility = Visibility.Collapsed);
            gridDashboardMainTab.Visibility = Visibility.Visible;
            RepaintDataGrids(gridDashboardMainTab);
        }

        private void SetupNotifyIcon()
        {
            var iconHandle = Properties.Resources.NotifyIcon.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(iconHandle);

            _notifyIcon = new NotifyIcon
            {
                BalloonTipTitle = lblWindowTitle.Content.ToString(),
                BalloonTipText = @"is hidden here",
                Icon = icon
            };
            _notifyIcon.Click += notifyIcon_Click;
        }

        private void SetupWindow()
        {
            _mouseOverBlueColor = ((SolidColorBrush)FindResource("MouseOverBlueBrush")).Color;
            _defaultBlueColor = ((SolidColorBrush)FindResource("DefaultBlueBrush")).Color;
            _defaultWindowColor = ((SolidColorBrush)FindResource("DefaultWindowBrush")).Color;
        }

        private void SetupTextBoxes()
        {
            var emptyTextBoxes = this.LogicalDescendants<TextBox>().Where(t => t.Tag != null && t.Text.IsNullOrWhiteSpace());
            foreach (var txtB in emptyTextBoxes)
            {
                txtB.GotFocus += TxtAll_GotFocus;
                txtB.LostFocus += TxtAll_LostFocus;

                var currBg = ((SolidColorBrush)txtB.Foreground).Color;
                txtB.FontStyle = FontStyles.Italic;
                txtB.Text = txtB.Tag.ToString();
                txtB.Foreground = new SolidColorBrush(Color.FromArgb(128, currBg.R, currBg.G, currBg.B));
            }
        }

        private void SetupTiles()
        {
            tmMainMenu.MenuTileClick += tmMainMenu_MenuTileClick;
        }

        private void SetupDataGrids()
        {
            gvTransactions.ItemsSource = _ocTransactions;
            gvTransactions.SetSelecteditemsSource(_ocSelectedTransactions);

            gvBlockchainExplorer.ItemsSource = _ocBlocks;
            gvBlockchainExplorer.SetSelecteditemsSource(_ocSelectedBlocks);
        }

        private void SetupDdls()
        {
            foreach (var ddl in this.LogicalDescendants<ComboBox, ListBox>().Cast<Selector>())
            {
                ddl.DisplayMemberPath = "Text";
                ddl.SelectedValuePath = "Index";
            }

            var apiMethods = typeof(BlockchainApiController).GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(HttpGetAttribute), false).Length > 0 
                    || m.GetCustomAttributes(typeof(HttpPostAttribute), false).Length > 0)
                .Select(m => m.Name).OrderBy(n => n).ToArray();

            EnqueueSetValue(ddlRequests, apiMethods).OrSetDirectlyIfGridVisible(gridHttpApiTestMainTab);
        }
        
        #endregion

        #region ControlsManagement



        #endregion
    }
}
