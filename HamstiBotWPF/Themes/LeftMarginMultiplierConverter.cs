using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TBotHamsti.Themes
{
    public class LeftMarginMultiplierConverter : IValueConverter
    {
        private static double DefaultLenght { get; set; } = 15;
        public double Lenght { get => DefaultLenght; set => DefaultLenght = value; }
        public static Thickness DefaultMargin => new Thickness(DefaultLenght, 0, 0, 0);
        public static double WidthGrid => 520;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is TreeViewItem item ? new Thickness(DefaultLenght * GetDepth(item), 0, 0, 0) : new Thickness(0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();

        public static int GetDepth(TreeViewItem item)
        {
            TreeViewItem parent;
            while ((parent = GetParent(item)) != null)
            {
                return GetDepth(parent) + 1;
            }

            return 0;
        }

        private static TreeViewItem GetParent(TreeViewItem item)
        {
            var parent = VisualTreeHelper.GetParent(item);

            while (!(parent is TreeViewItem || parent is TreeView))
            {
                if (parent == null) return null;
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as TreeViewItem;
        }
    }
}
