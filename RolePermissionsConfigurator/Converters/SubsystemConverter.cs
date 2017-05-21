using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using DevExpress.Mvvm.Native;

namespace Swsu.Lignis.RolePermissionsConfigurator.Converters
{
	public class SubsystemConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var subsystems = value as ObservableCollection<object>;

			if (subsystems == null) return value;

			var sbs = new List<object>();
			subsystems.ForEach(s => sbs.Add(s));

			return sbs;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var list = value as List<object>;

			if (list == null) return value;

			var sbs = new ObservableCollection<object>();

			list.ForEach(e => sbs.Add(e));

			return sbs;
		}
	}
}