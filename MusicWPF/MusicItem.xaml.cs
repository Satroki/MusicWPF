using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MusicWPF
{
    /// <summary>
    /// MusicItem.xaml 的交互逻辑
    /// </summary>
    public partial class MusicItem : UserControl
    {
        public MusicItem()
        {
            InitializeComponent();
        }
    }

    public class TextOverVC : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            var length = int.Parse((string)parameter);
            if (str.Length <= length)
                return str;
            else
                return str.Substring(0, length - 1) + " ...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
