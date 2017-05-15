using System;
using System.Collections.Generic;

namespace Swsu.Lignins.PluginPermissionsContract
{
	public class PermissionInfo : BaseInfo
	{

		#region Properties

		public PermissionTypeInfo TypeInfo { get; }

		#endregion


		#region Methods

		#endregion

		public PermissionInfo(string name, string description, PermissionTypeInfo typeInfo) : base(name, description)
		{
			TypeInfo = typeInfo;
		}
	}
}