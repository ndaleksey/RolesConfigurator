using System.Globalization;

namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal class PlugInLocalizationElementCollection : ElementCollectionBase<PlugInLocalizationElement, CultureInfo>
	{
		#region Constructors

		public PlugInLocalizationElementCollection()
		{
		}

		#endregion

		#region Methods

		protected override CultureInfo GetElementKeyCore(PlugInLocalizationElement element)
		{
			return element.Culture;
		}

		#endregion
	}
}
