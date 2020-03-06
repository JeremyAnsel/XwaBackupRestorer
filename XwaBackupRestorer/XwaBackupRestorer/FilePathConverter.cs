using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace XwaBackupRestorer
{
    public sealed class FilePathConverter : IMultiValueConverter
    {
        public static readonly FilePathConverter Default = new FilePathConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values.Any(t => t == null || t == DependencyProperty.UnsetValue))
            {
                return null;
            }

            string directory = (string)values[0];
            string file = (string)values[1];

            return System.IO.Path.Combine(directory, "Backup", file);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
