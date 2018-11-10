using CiphersLibrary.Data;
using CryptoCore.Data;
using System;
using System.Globalization;

namespace CryptoCore.Core
{
    public class SelectedItemToBooleanConverter : BaseValueConverter<SelectedItemToBooleanConverter>
    {
        /// <summary>
        /// Converter helps with determining  if this selected 
        /// algorithm does support generating keys
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == (int)CryptoSystems.Rabin)
            {
                return true;
            }
            else if ((int)value == (int)CryptoSystems.YarmolikRabin)
            {
                return false;
            }

            return false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
