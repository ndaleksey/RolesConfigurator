using System.Configuration;
using System.Globalization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal class PlugInLocalizationElement : ConfigurationElement
	{
		#region Constants

		private const string CulturePropertyName = "culture";

		private const string DescriptionPropertyName = "description";

		private const string DisplayNamePropertyName = "displayName";

		#endregion

		#region Constructors

		public PlugInLocalizationElement()
		{
		}

		#endregion

		#region Properties

		[ConfigurationProperty(CulturePropertyName, IsKey = true, IsRequired = true)]
		public CultureInfo Culture
		{
			get { return (CultureInfo) this[CulturePropertyName]; }
			set { this[CulturePropertyName] = value; }
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

		#endregion
	}
}