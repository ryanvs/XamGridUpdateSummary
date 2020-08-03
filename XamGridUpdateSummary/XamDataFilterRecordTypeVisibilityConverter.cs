using Infragistics.Windows.DataPresenter;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace XamGridUpdateSummary
{
    [ValueConversion(typeof(RecordType), typeof(Visibility))]
    public class XamDataFilterRecordTypeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RecordType)
            {
                var rt = (RecordType)value;
                if (rt == RecordType.FilterRecord)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
