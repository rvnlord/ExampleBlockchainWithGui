using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using BlockchainApp.Source.Common.Extensions;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.TypeUtils;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MoreLinq;
using static BlockchainApp.Source.Config;

namespace BlockchainApp.Source.Common.Utils
{
    public static class AsyncUtils
    {
        public static async Task<Exception> AsyncWithLoader(Panel loaderContainer, IReadOnlyCollection<object> objectsToDisable, Action action)
        {
            return await Task.Run(async () =>
            {
                var wnd = loaderContainer.LogicalAncestor<MetroWindow>();
                var actuallyDisabledControls = objectsToDisable;
                Exception exception = null;
                wnd.Dispatcher.Invoke(() =>
                {
                    if (objectsToDisable != null && objectsToDisable.Any())
                        objectsToDisable.DisableControls();
                    loaderContainer.ShowLoader();
                });

                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    exception = ex;
                    await wnd.Dispatcher.Invoke(async () => await wnd.ShowMessageAsync("Error occured", ex.Message));
                }
                finally
                {
                    wnd.Dispatcher.Invoke(() =>
                    {
                        loaderContainer.HideLoader();
                        var gridMain = wnd.LogicalDescendants<Grid>().Single(g => g.Name == "gridMain");
                        if (gridMain.HasLoader()) return;

                        if (actuallyDisabledControls != null && actuallyDisabledControls.Any())
                            actuallyDisabledControls.EnableControls();
                    });
                }

                return exception;
            });
        }

        public static Task AsyncWithLoader(Panel loaderContainer, IReadOnlyCollection<object> objectsToDisable, Func<List<object>> action)
        {
            var task = Task.Run(async () =>
            {
                var wnd = loaderContainer.LogicalAncestor<MetroWindow>();
                var actuallyDisabledControls = objectsToDisable;
                wnd.Dispatcher.Invoke(() =>
                {
                    if (objectsToDisable != null && objectsToDisable.Any())
                        objectsToDisable.DisableControls();
                    loaderContainer.ShowLoader();
                });

                try
                {
                    actuallyDisabledControls = action();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    await wnd.Dispatcher.Invoke(async () => await wnd.ShowMessageAsync("Error occured", ex.Message));
                }
                finally
                {
                    wnd.Dispatcher.Invoke(() =>
                    {
                        loaderContainer.HideLoader();
                        var gridMain = wnd.LogicalDescendants<Grid>().Single(g => g.Name == "gridMain");
                        if (gridMain.HasLoader()) return;

                        if (actuallyDisabledControls != null && actuallyDisabledControls.Any())
                            actuallyDisabledControls.EnableControls();
                    });
                }
            });
            return task;
        }

        public static void ShowLoader(Panel control)
        {
            var rect = new Rectangle
            {
                Margin = new Thickness(0),
                Fill = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Name = "prLoaderContainer"
            };

            var loader = new ProgressRing
            {
                Foreground = (Brush)control.LogicalAncestor<Window>().FindResource("AccentColorBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 80,
                Height = 80,
                IsActive = true,
                Name = "prLoader"
            };

            var status = new TextBlock
            {
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(0, 125, 0, 0),
                Text = "Loading...",
                Name = "prLoaderStatus"
            };

            var zIndex = MoreEnumerable.Append(control.LogicalDescendants<FrameworkElement>(), control).MaxBy(Panel.GetZIndex).First().ZIndex();
            Panel.SetZIndex(rect, zIndex + 1);
            Panel.SetZIndex(loader, zIndex + 1);
            Panel.SetZIndex(status, zIndex + 1);

            control.Children.AddRange(new FrameworkElement[] { rect, loader, status }); // 
        }

        public static void HideLoader(Panel control)
        {
            var loaders = control.LogicalDescendants<FrameworkElement>().Where(c => c.Name == "prLoader").ToArray();
            var loaderContainers = control.LogicalDescendants<FrameworkElement>().Where(c => c.Name == "prLoaderContainer").ToArray();
            var loaderStatuses = control.LogicalDescendants<FrameworkElement>().Where(c => c.Name == "prLoaderStatus").ToArray();
            var loaderParts = ArrayUtils.ConcatMany(loaders, loaderContainers, loaderStatuses);

            loaderParts.ForEach(lp => control.Children.Remove(lp));
        }

        public static bool HasLoader(Panel control)
        {
            return control.LogicalDescendants<Rectangle>().Any(r => r.Name == "prLoaderContainer");
        }

        public static LoaderSpinnerWrapper GetLoader(Window wnd)
        {
            return wnd.Dispatcher.Invoke(() =>
            {
                return new LoaderSpinnerWrapper
                {
                    LoaderControl = wnd.LogicalDescendants<ProgressRing>().First(c => c.Name == "prLoader"),
                    LoaderStatus = wnd.LogicalDescendants<TextBlock>().First(c => c.Name == "prLoaderStatus")
                };
            });
        }

        public static void SetLoaderStatus(Window wnd, string status)
        {
            var tbLoaderStatus = wnd.GetLoader().LoaderStatus;
            wnd.Dispatcher.Invoke(() =>
            {
                tbLoaderStatus.Text = status;
            });
        }

        public static void Sync(Task task) => task.GetAwaiter().GetResult();

        public static TResult Sync<TResult>(Task<TResult> task) => task.GetAwaiter().GetResult();
    }

    public class LoaderSpinnerWrapper
    {
        public ProgressRing LoaderControl { get; set; }
        public TextBlock LoaderStatus { get; set; }
    }
}
