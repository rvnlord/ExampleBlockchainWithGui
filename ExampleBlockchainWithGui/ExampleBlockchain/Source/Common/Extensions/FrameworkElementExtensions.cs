using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using BlockchainApp.Source.Common.Extensions.Collections;
using BlockchainApp.Source.Common.Utils.UtilClasses.Menu;
using static BlockchainApp.Source.Common.Constants;
using static BlockchainApp.Source.Common.Utils.LockUtils;
using ContextMenu = BlockchainApp.Source.Common.Utils.UtilClasses.Menu.ContextMenu;

namespace BlockchainApp.Source.Common.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static Rect ClientRectangle(this FrameworkElement el, FrameworkElement container)
        {
            var rect = VisualTreeHelper.GetDescendantBounds(el);
            var loc = el.TransformToAncestor(container).Transform(new Point(0, 0));
            rect.Location = loc;
            return rect;
        }

        public static bool HasClientRectangle(this FrameworkElement el, FrameworkElement container)
        {
            return !VisualTreeHelper.GetDescendantBounds(el).IsEmpty;
        }

        public static bool IsWithinBounds(this FrameworkElement element, FrameworkElement container)
        {
            if (!element.IsVisible)
                return false;

            var bounds = element.TransformToAncestor(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        public static bool HasContextMenu(this FrameworkElement el)
        {
            return ContextMenusManager.ContextMenus.VorN(el)?.IsCreated == true;
        }

        public static ContextMenu ContextMenu(this FrameworkElement el)
        {
            return ContextMenusManager.ContextMenu(el);
        }

        public static Task AnimateAsync(this FrameworkElement fwElement, PropertyPath propertyPath, AnimationTimeline animation)
        {
            return Lock (_globalSync, nameof(_globalSync), nameof(AnimateAsync), () =>
            {
                var tcs = new TaskCompletionSource<bool>();
                var storyBoard = new Storyboard();
                void storyBoard_Completed(object s, EventArgs e) => tcs.TrySetResult(true);

                var isSbInDict = StoryBoards.VorN(fwElement) != null;
                if (isSbInDict)
                {
                    StoryBoards[fwElement].Stop(fwElement);
                    StoryBoards.Remove(fwElement);
                    StoryBoards.Add(fwElement, storyBoard);
                }

                Storyboard.SetTarget(animation, fwElement);
                Storyboard.SetTargetProperty(animation, propertyPath);
                storyBoard.Children.Add(animation);
                storyBoard.Completed += storyBoard_Completed;

                storyBoard.Begin(fwElement, true);
                return tcs.Task;
            });
        }

        public static Task AnimateAsync(this FrameworkElement fwElement, DependencyProperty dp, AnimationTimeline animation)
        {
            return AnimateAsync(fwElement, new PropertyPath(dp), animation);
        }

        public static Task AnimateAsync(this FrameworkElement fwElement, string propertyPath, AnimationTimeline animation)
        {
            return AnimateAsync(fwElement, new PropertyPath(propertyPath), animation);
        }

        public static void Animate(this FrameworkElement fwElement, PropertyPath propertyPath, AnimationTimeline animation, EventHandler callback = null)
        {
            Lock (_globalSync, nameof(_globalSync), nameof(Animate), () =>
            {
                var storyBoard = new Storyboard();

                var isSbInDict = StoryBoards.VorN(fwElement) != null;
                if (isSbInDict)
                {
                    StoryBoards[fwElement].Stop(fwElement);
                    StoryBoards.Remove(fwElement);
                    StoryBoards.Add(fwElement, storyBoard);
                }

                Storyboard.SetTarget(animation, fwElement);
                Storyboard.SetTargetProperty(animation, propertyPath);
                storyBoard.Children.Add(animation);
                if (callback != null)
                    storyBoard.Completed += callback;

                storyBoard.Begin(fwElement, true);
            });
        }

        public static void Animate(this FrameworkElement fwElement, DependencyProperty dp, AnimationTimeline animation, EventHandler callback = null)
        {
            Animate(fwElement, new PropertyPath(dp), animation, callback);
        }

        public static void Animate(this FrameworkElement fwElement, string propertyPath, AnimationTimeline animation, EventHandler callback = null)
        {
            Animate(fwElement, new PropertyPath(propertyPath), animation, callback);
        }

        public static void SlideShow(this Panel c)
        {
            if (!PanelAnimations.VorN_Ts(c.Name + "IsOpened"))
                c.SlideToggle();
        }

        public static void SlideHide(this Panel c)
        {
            if (PanelAnimations.VorN_Ts(c.Name + "IsOpened"))
                c.SlideToggle();
        }

        public static async void SlideToggle(this Panel c)
        {
            var strIsOpened = c.Name + "IsOpened";
            var isOpened = PanelAnimations.VorN_Ts(strIsOpened);
            var slideGrid = c.HasSlideGrid() ? c.GetSlideGrid() : c.CreateAndAddSlideGrid(!isOpened ? 0 : c.Width);
            PanelAnimations.V_Ts(strIsOpened, !isOpened);
            var slideAni = new DoubleAnimation(isOpened ? 0 : c.Width, new Duration(TimeSpan.FromMilliseconds(500)));

            if (isOpened) // jeśli otwarty na początku
                c.Visibility = Visibility.Hidden;

            await slideGrid.AnimateAsync(FrameworkElement.WidthProperty, slideAni);
            c.RemoveSlideGrid();

            if (PanelAnimations.VorN_Ts(strIsOpened)) // jeśli otwarty po animacji (w dowolnej kolejności)
                c.Visibility = Visibility.Visible;
        }

        private static bool HasSlideGrid(this Panel c)
        {
            return Lock(_globalSync, nameof(_globalSync), nameof(HasSlideGrid), () =>
            {
                var parentGrid = c.LogicalAncestor<Grid>();
                return parentGrid.Children.OfType<Grid>().Any(grid => grid.Name == "gridSlide" + c.Name);
            });
        }

        private static Grid GetSlideGrid(this Panel c)
        {
            return Lock(_globalSync, nameof(_globalSync), nameof(GetSlideGrid), () =>
            {
                var parentGrid = c.LogicalAncestor<Grid>();
                return parentGrid.Children.OfType<Grid>().Single(grid => grid.Name == "gridSlide" + c.Name);
            });
        }

        private static Grid CreateSlideGrid(this Panel c, double slideGridWidth)
        {
            return Lock(_globalSync, nameof(_globalSync), nameof(CreateSlideGrid), () =>
            {
                var phName = "gridSlide" + c.Name;
                var gridPh = new Grid
                {
                    Width = slideGridWidth,
                    Height = c.Height,
                    Background = c.Background,
                    Margin = c.Margin,
                    HorizontalAlignment = c.HorizontalAlignment,
                    VerticalAlignment = c.VerticalAlignment,
                    Name = phName
                };
                Panel.SetZIndex(gridPh, Panel.GetZIndex(c));
                return gridPh;
            });
        }

        private static Grid AddSlideGrid(this Panel c, Grid slideGrid)
        {
            return Lock(_globalSync, nameof(_globalSync), nameof(AddSlideGrid), () =>
            {
                var parentGrid = c.LogicalAncestor<Grid>();
                parentGrid.Children.Add(slideGrid);
                return slideGrid;
            });
        }

        private static Grid CreateAndAddSlideGrid(this Panel c, double slideGridWidth)
        {
            return c.AddSlideGrid(c.CreateSlideGrid(slideGridWidth));
        }

        private static void RemoveSlideGrid(this Panel c)
        {
            Lock(_globalSync, nameof(_globalSync), nameof(RemoveSlideGrid), () =>
            {
                var parentGrid = c.LogicalAncestor<Grid>();
                var slideGrid = parentGrid.Children.OfType<Grid>().SingleOrDefault(grid => grid.Name == "gridSlide" + c.Name);
                if (slideGrid != null)
                    parentGrid.Children.Remove(slideGrid);
            });
        }

        public static int ZIndex(this FrameworkElement fe)
        {
            return Panel.GetZIndex(fe);
        }

        public static void ZIndex(this FrameworkElement fe, int zINdex)
        {
            Panel.SetZIndex(fe, zINdex);
        }

        public static void Position(this FrameworkElement control, Point pos)
        {
            Canvas.SetLeft(control, pos.X);
            Canvas.SetTop(control, pos.Y);
        }

        public static void PositionX(this FrameworkElement control, double posX)
        {
            Canvas.SetLeft(control, posX);
        }

        public static void PositionY(this FrameworkElement control, double posY)
        {
            Canvas.SetTop(control, posY);
        }

        public static Point Position(this FrameworkElement control)
        {
            control.Refresh();
            return new Point(Canvas.GetLeft(control), Canvas.GetTop(control));
        }

        public static double PositionX(this FrameworkElement control)
        {
            control.Refresh();
            return Canvas.GetLeft(control);
        }

        public static double PositionY(this FrameworkElement control)
        {
            control.Refresh();
            return Canvas.GetTop(control);
        }

        public static void Margin(this FrameworkElement control, Point pos)
        {
            var initOpacity = control.Opacity;
            var initVisibility = control.Visibility;
            control.Opacity = 0;
            control.Visibility = Visibility.Visible;
            control.Margin = new Thickness(pos.X, pos.Y, control.Margin.Right, control.Margin.Bottom);
            control.Visibility = initVisibility;
            control.Opacity = initOpacity;
        }

        public static void MarginX(this FrameworkElement control, double posX)
        {
            control.Margin = new Thickness(posX, control.Margin.Top, control.Margin.Right, control.Margin.Bottom);
        }

        public static void MarginY(this FrameworkElement control, double posY)
        {
            control.Margin = new Thickness(control.Margin.Left, posY, control.Margin.Right, control.Margin.Bottom);
        }

        public static Point MarginPosition(this FrameworkElement control)
        {
            control.Refresh();
            return new Point(control.Margin.Left, control.Margin.Top);
        }

        public static double MarginX(this FrameworkElement control)
        {
            control.Refresh();
            return control.Margin.Left;
        }

        public static double MarginY(this FrameworkElement control)
        {
            control.Refresh();
            return control.Margin.Top;
        }

        public static void Refresh(this FrameworkElement control)
        {
            control.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
            control.UpdateLayout();
        }
    }
}
