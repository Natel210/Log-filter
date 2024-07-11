using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace LogFilter_UI
{
    public class DirectoryInfoToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo directoryInfo)
            {
                return directoryInfo.FullName;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return new DirectoryInfo(path);
            }
            return null;
        }
    }
}
