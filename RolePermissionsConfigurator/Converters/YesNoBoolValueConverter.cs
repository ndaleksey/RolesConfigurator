using System;
using System.Globalization;
using System.Windows.Data;

namespace Swsu.Lignis.RolePermissionsConfigurator.Converters
{
	public class YesNoBoolValueConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return "нет";
			return value is bool && (bool) value ? "есть" : "нет";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}