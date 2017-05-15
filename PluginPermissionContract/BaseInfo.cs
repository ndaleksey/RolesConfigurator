namespace Swsu.Lignins.PluginPermissionsContract
{
	public class BaseInfo
	{
		#region Properties
		public string Name { get; }
		public string Description { get; }
		#endregion

		#region Constructors
		public BaseInfo(string name, string description)
		{
			Name = name;
			Description = description;
		}
		#endregion
	}
}