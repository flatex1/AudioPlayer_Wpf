using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace WPF_Audio.Converters
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
                return new BitmapImage(new Uri("pack://application:,,,/WPF_Audio;component/Assets/file-music.png"));

            try
            {
                if (str.StartsWith("pack://"))
                    return new BitmapImage(new Uri(str));
                // base64
                byte[] bytes = System.Convert.FromBase64String(str);
                using (var ms = new MemoryStream(bytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    image.Freeze();
                    return image;
                }
            }
            catch
            {
                return new BitmapImage(new Uri("pack://application:,,,/WPF_Audio;component/Assets/file-music.png"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
} 