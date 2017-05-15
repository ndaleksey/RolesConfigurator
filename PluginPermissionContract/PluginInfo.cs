using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Swsu.Lignins.PluginPermissionsContract
{
	/// <summary>
	/// Информаци о плагине
	/// </summary>
	public class PluginInfo : BaseInfo
	{
		#region Constructors
		public PluginInfo(string name, string description) : base(name, description)
		{
			Permissions = new Collection<PermissionInfo>();
		}
		#endregion

		#region Properties
		public Collection<PermissionInfo> Permissions { get; }

		#endregion
	}
}