namespace Swsu.Lignins.PluginPermissionsContract
{
	public class PermissionValueInfo
	{
		#region Properties
		public int Ordinal { get; set; }
		public string Name { get; set; }
		#endregion

		#region Methods

		public override string ToString()
		{
			return Name;
		}

		#endregion
	}
}