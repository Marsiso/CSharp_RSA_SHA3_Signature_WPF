using System;
using System.Globalization;
using Org.BouncyCastle.Math;
using System.Windows.Data;

namespace CSharp_RSA_Cipher_WPF.ViewModels
{
    public sealed class BigIntegerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BigInteger bigInteger = (BigInteger)value;
            if (value is null)
            {
                throw new ArgumentException();
            }
            return bigInteger.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? text = value as string;
            if (text is null)
            {
                throw new ArgumentException();
            }
            if (text.Equals(string.Empty))
            {
                return BigInteger.Zero;
            }

            return new BigInteger(text);
        }
    }
}
