using Microsoft.UI.Xaml.Data;
using System;

namespace SuperAudio.Converters
{
    class StringCompareToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null)
                return false;
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isChecked && isChecked)
            {
                return parameter; // 返回对应的枚举值
            }
            return parameter;
        }
    }
}
