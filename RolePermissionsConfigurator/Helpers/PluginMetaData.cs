using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	public class PluginMetaData
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("assembly")]
		public string Assembly { get; set; }

		[XmlAttribute("displayName")]
		public string DisplayName { get; set; }

		[XmlAttribute("description")]
		public string Description { get; set; }

		[XmlArray("localizations")]
		[XmlArrayItem("add")]
		public Collection<PluginLocalizationMetaData> Localizations { get; } = new Collection<PluginLocalizationMetaData>();

		public PluginMetaData()
		{
		}

		public PluginMetaData(string name, string assembly)
		{
			Name = name;
			Assembly = assembly;
		}

		public string GetDescription(CultureInfo culture)
		{
			return Get(culture, pl => pl.Description, p => p.Description);
		}

		public string GetDisplayName(CultureInfo culture)
		{
			return Get(culture, pl => pl.DisplayName, p => p.DisplayName);
		}

		private string Get(CultureInfo culture, Func<PluginLocalizationMetaData, string> get,
			Func<PluginMetaData, string> getInvariant)
		{
			for (var c = culture; c != CultureInfo.InvariantCulture; c = c.Parent)
			{
				var localization = FindLocalization(c);

				if (localization == null)
					continue;

				var result = get(localization);

				if (string.IsNullOrEmpty(result))
					continue;

				return result;
			}

			return getInvariant(this);
		}

		private PluginLocalizationMetaData FindLocalization(CultureInfo c)
		{
			return Localizations.FirstOrDefault(l => l.Culture == c.ToString());
		}
	}
}