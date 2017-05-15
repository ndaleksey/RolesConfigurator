using System.Xml.Serialization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	[XmlType(TypeName = "add")]
	public class PluginMetaData
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlAttribute("assembly")]
		public string Assembly { get; set; }

		public PluginMetaData()
		{
		}

		public PluginMetaData(string name, string assembly)
		{
			Name = name;
			Assembly = assembly;
		}
	}
}