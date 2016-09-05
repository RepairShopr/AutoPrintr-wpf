using System.Windows;
using System.Windows.Media;

namespace AutoPrintr.Helpers
{
    public static class VisualTreeHelperExtensions
    {
        public static T FindParent<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent == null) return null;

            var parentT = parent as T;
            return parentT ?? parent.FindParent<T>();
        }
    }
}