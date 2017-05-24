using System.Configuration;

namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal class PlugInsSection : ConfigurationSection
	{
		#region Constructors

		public PlugInsSection()
		{
		}

		#endregion

		#region Properties

		[ConfigurationProperty("", IsDefaultCollection = true)]
		public PlugInElementCollection Items => (PlugInElementCollection) this[""];

		#endregion
	}
}