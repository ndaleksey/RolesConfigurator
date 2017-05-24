using System;
using System.Configuration;
using System.Globalization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal class PlugInElement : ConfigurationElement
	{
		#region Constants

		private const string AssemblyPropertyName = "assembly";

		private const string DescriptionPropertyName = "description";

		private const string DisplayNamePropertyName = "displayName";

		private const string LocalizationsPropertyName = "localizations";

		private const string NamePropertyName = "name";

		#endregion

		#region Constructors

		public PlugInElement()
		{
		}

		#endregion

		#region Properties

		[ConfigurationProperty(AssemblyPropertyName, IsRequired = true)]
		public string Assembly
		{
			get { return (string) this[AssemblyPropertyName]; }
			set { this[AssemblyPropertyName] = value; }
		}

		[ConfigurationProperty(DescriptionPropertyName, IsRequired = false)]
		public string Description
		{
			get { return (string) this[DescriptionPropertyName]; }
			set { this[DescriptionPropertyName] = value; }
		}

		[ConfigurationProperty(DisplayNamePropertyName, IsRequired = false)]
		public string DisplayName
		{
			get { return (string) this[DisplayNamePropertyName]; }
			set { this[DisplayNamePropertyName] = value; }
		}

		[ConfigurationProperty(LocalizationsPropertyName)]
		public PlugInLocalizationElementCollection Localizations
		{
			get { return (PlugInLocalizationElementCollection) this[LocalizationsPropertyName]; }
		}

		[ConfigurationProperty(NamePropertyName, IsKey = true, IsRequired = true)]
		public string Name
		{
			get { return (string) this[NamePropertyName]; }
			set { this[NamePropertyName] = value; }
		}

		#endregion

		#region Methods

		public string GetDescription(CultureInfo culture)
		{
			return Get(culture, pl => pl.Description, p => p.Description);
		}

		public string GetDisplayName(CultureInfo culture)
		{
			return Get(culture, pl => pl.DisplayName, p => p.DisplayName);
		}

		private string Get(CultureInfo culture, Func<PlugInLocalizationElement, string> get,
			Func<PlugInElement, string> getInvariant)
		{
			for (var c = culture; c != CultureInfo.InvariantCulture; c = c.Parent)
			{
				var localization = Localizations[c];

				if (localization == null)
				{
					continue;
				}

				var result = get(localization);

				if (string.IsNullOrEmpty(result))
				{
					continue;
				}

				return result;
			}

			return getInvariant(this);
		}

		#endregion
	}
}