using System.Xml.Serialization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	public class PluginLocalizationMetaData
	{
		[XmlAttribute("culture")]
		public string Culture { get; set; }

		[XmlAttribute("displayName")]
		public string DisplayName { get; set; }

		[XmlAttribute("description")]
		public string Description { get; set; }
	}
}
