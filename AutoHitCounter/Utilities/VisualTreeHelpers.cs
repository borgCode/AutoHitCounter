// 

using System.Windows;
using System.Windows.Media;

namespace AutoHitCounter.Utilities;

public static class VisualTreeHelpers
{
    public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
                return target;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}