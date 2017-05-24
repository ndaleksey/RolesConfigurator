namespace Swsu.Lignis.RolePermissionsConfigurator.Configuration
{
	internal class PlugInElementCollection : ElementCollectionBase<PlugInElement, string>
	{
		#region Constructors

		public PlugInElementCollection()
		{
		}

		#endregion

		#region Methods

		protected override string GetElementKeyCore(PlugInElement element)
		{
			return element.Name;
		}

		#endregion
	}
}