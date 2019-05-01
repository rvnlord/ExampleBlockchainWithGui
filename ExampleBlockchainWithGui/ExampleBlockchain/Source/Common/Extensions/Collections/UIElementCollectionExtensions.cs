using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BlockchainApp.Source.Common.Extensions.Collections
{
    public static class UIElementCollectionExtensions
    {
        public static UIElementCollection ReplaceAll<T>(this UIElementCollection col, IEnumerable<T> en) where T : UIElement
        {
            var list = en.ToList();
            col.RemoveAll();
            col.AddRange(list);
            return col;
        }

        public static void RemoveAll(this UIElementCollection collection)
        {
            while (collection.Count != 0)
                collection.RemoveAt(0);
        }

        public static void PrepandAllBefore(this UIElementCollection existingUiElements, IEnumerable<UIElement> newUiElements, UIElement existingUiElement)
        {
            existingUiElements.Remove(existingUiElement);
            foreach (var newUiElement in newUiElements)
                existingUiElements.Add(newUiElement);
            existingUiElements.Add(existingUiElement);
        }

        public static void PrepandAllBefore(this UIElementCollection existingUiElements, IEnumerable<UIElement> newUiElements, IEnumerable<UIElement> beforeUiElements)
        {
            var arrBeforeUiElements = beforeUiElements.ToArray();
            existingUiElements.RemoveRange(arrBeforeUiElements);
            foreach (var newUiElement in newUiElements)
                existingUiElements.Add(newUiElement);
            existingUiElements.AddRange(arrBeforeUiElements);
        }
    }
}
