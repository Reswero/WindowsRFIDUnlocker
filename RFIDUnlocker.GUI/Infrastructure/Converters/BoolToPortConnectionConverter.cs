using System;
using System.Globalization;
using System.Windows.Data;

namespace RFIDUnlocker.GUI.Infrastructure.Converters
{
	internal class BoolToPortConnectionConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool isConnected)
			{
				if (isConnected)
				{
					return "Отключиться";
				}
				else
				{
					return "Подключиться";
				}
			}

			return string.Empty;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
