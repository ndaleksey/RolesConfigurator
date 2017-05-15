using System;

namespace Swsu.Lignins.PluginPermissionsContract
{
	[AttributeUsage(AttributeTargets.Enum)]
	public class PermissionTypeAttribute : Attribute
	{
		 public Type BaseName { get; set; }
	}
}