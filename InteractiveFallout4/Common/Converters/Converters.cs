using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Data;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Globalization;
using System.Collections.ObjectModel;

namespace InteractiveFallout4.Common.Converters
{
    [ValueConversion(typeof(bool), typeof(ComboBoxItem))]
    class EnableConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return "Включено";
            }
            else
            {
                return "Отключено";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if ((value).ToString() == "Включено")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    class UriConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return Path.GetFileName(((string)value));
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    class DonateTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((string)value) == "Donate")
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

    class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    class IsGreaterThanZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value > 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    class IsEqualZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value == 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    class AlterationNumberConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value + 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

    class WriteTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Write": return "Простая запись";
                case "GetAddAndWrite": return "Добавление и запись";
                case "GetMultipleAndWrite": return "Умножение и запись";
                case "GetDevideAndWrite": return "Деление и запись";
                default: return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((string)value)
            {
                case "Простая запись": return "Write";
                case "Добавление и запись": return "GetAddAndWrite";
                case "Умножение и запись": return "GetMultipleAndWrite";
                case "Деление и запись": return "GetDevideAndWrite";
                default: return "";
            }
        }
    }

    class CommandMultParamConverter : IMultiValueConverter
    {


        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class CurrentAppDirectoryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Directory.GetCurrentDirectory() + (string)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

    class ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (System.Windows.Media.Color)value;
            return new System.Windows.Media.SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    class BoolInvertingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }
    class ObservableCollBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<bool> obs = (ObservableCollection<bool>)value;
            return !obs.All(obj=>obj==true);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("IsNullConverter can only be used OneWay.");
        }
    }

}
