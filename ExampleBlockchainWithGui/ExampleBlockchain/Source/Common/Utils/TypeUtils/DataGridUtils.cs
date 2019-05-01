using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using BlockchainApp.Source.Common.Extensions;

namespace BlockchainApp.Source.Common.Utils.TypeUtils
{
    public static class DataGridUtils
    {
        private static bool _isSyncingSelection;
        private static readonly List<Tuple<WeakReference, List<DataGrid>>> _collectionToGridViews = new List<Tuple<WeakReference, List<DataGrid>>>();
        private static Dictionary<string, List<Delegate>> _selectionChangedEventMap = new Dictionary<string, List<Delegate>>();

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(INotifyCollectionChanged),
            typeof(DataGridUtils),
            new PropertyMetadata(null, OnSelectedItemsChanged));

        public static INotifyCollectionChanged GetSelectedItems(DependencyObject obj)
        {
            return (INotifyCollectionChanged)obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject depObj, INotifyCollectionChanged value)
        {
            var gv = (DataGrid) depObj;
            gv.RemoveEventHandlers(nameof(gv.SelectionChanged));
            gv.SetValue(SelectedItemsProperty, value);
        }

        private static void OnSelectedItemsChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var gridView = (DataGrid)target;

            if (args.OldValue is INotifyCollectionChanged oldCollection)
            {
                gridView.SelectionChanged -= GridView_SelectionChanged;
                oldCollection.CollectionChanged -= SelectedItems_CollectionChanged;
                RemoveAssociation(oldCollection, gridView);
            }

            if (args.NewValue is INotifyCollectionChanged newCollection)
            {
                gridView.SelectionChanged += GridView_SelectionChanged;
                newCollection.CollectionChanged += SelectedItems_CollectionChanged;

                AddAssociation(newCollection, gridView);
                OnSelectedItemsChanged(newCollection, null, (IList)newCollection);
            }
        }

        private static void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            var collection = (INotifyCollectionChanged)sender;
            OnSelectedItemsChanged(collection, args.OldItems, args.NewItems);
        }

        private static void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncingSelection)
                return;

            var gridView = (DataGrid)sender;
            var collection = (IList)GetSelectedItems(gridView);
            foreach (var item in e.RemovedItems)
                collection.Remove(item);
            foreach (var item in e.AddedItems)
                collection.Add(item);

            var wnd = gridView.LogicalAncestor<Window>();
            var handler = wnd.GetType().GetRuntimeMethods().FirstOrDefault(m => m.Name == $"{gridView.Name}_SelectionChanged");
            handler?.Invoke(wnd, new object[] { gridView, e });
        }

        private static void OnSelectedItemsChanged(INotifyCollectionChanged collection, IList oldItems, IList newItems)
        {
            _isSyncingSelection = true;

            var gridViews = GetOrCreateGridViews(collection);
            foreach (var gridView in gridViews)
                SyncSelection(gridView, oldItems, newItems);

            _isSyncingSelection = false;
        }

        private static void SyncSelection(DataGrid gridView, IEnumerable oldItems, IEnumerable newItems)
        {
            if (oldItems != null)
                SetItemsSelection(gridView, oldItems, false);
            if (newItems != null)
                SetItemsSelection(gridView, newItems, true);
        }

        private static void SetItemsSelection(DataGrid gridView, IEnumerable items, bool shouldSelect)
        {
            foreach (var item in items)
            {
                var contains = gridView.SelectedItems.Contains(item);
                if (shouldSelect && !contains)
                    gridView.SelectedItems.Add(item);
                else if (contains && !shouldSelect)
                    gridView.SelectedItems.Remove(item);
            }
        }

        private static void AddAssociation(INotifyCollectionChanged collection, DataGrid gridView)
        {
            var gridViews = GetOrCreateGridViews(collection);
            gridViews.Add(gridView);
        }

        private static void RemoveAssociation(INotifyCollectionChanged collection, DataGrid gridView)
        {
            var gridViews = GetOrCreateGridViews(collection);
            gridViews.Remove(gridView);

            if (gridViews.Count == 0)
                CleanUp();
        }

        private static List<DataGrid> GetOrCreateGridViews(INotifyCollectionChanged collection)
        {
            foreach (var (wr, gv) in _collectionToGridViews)
                if (wr.Target.Equals(collection))
                    return gv;

            _collectionToGridViews.Add(new Tuple<WeakReference, List<DataGrid>>(new WeakReference(collection), new List<DataGrid>()));
            return _collectionToGridViews[_collectionToGridViews.Count - 1].Item2;
        }

        private static void CleanUp()
        {
            for (var i = _collectionToGridViews.Count - 1; i >= 0; i--)
            {
                var isAlive = _collectionToGridViews[i].Item1.IsAlive;
                var behaviors = _collectionToGridViews[i].Item2;
                if (behaviors.Count == 0 || !isAlive)
                    _collectionToGridViews.RemoveAt(i);
            }
        }
    }

}
