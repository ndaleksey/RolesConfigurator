using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Swsu.Lignins.PluginPermissionsContract
{
	public class PermissionTypeInfo
	{
		#region Properties
		public List<PermissionValueInfo> Values { get; } = new List<PermissionValueInfo>();

		#endregion

		#region Constructors

		#endregion

		#region Methods

		public static PermissionTypeInfo FromEnum<T>()
		{
			return FromType(typeof (T));
		}

		public static PermissionTypeInfo FromType(Type type)
		{
			ResourceManager resourceManager = null;
			var attrubute = type.GetCustomAttribute<PermissionTypeAttribute>();

			if (null != attrubute)
				resourceManager = new ResourceManager(attrubute.BaseName.FullName, type.Assembly);

			var info = new PermissionTypeInfo();

			foreach (var value in Enum.GetValues(type))
			{
				if (value != null)
					info.Values.Add(new PermissionValueInfo
					{
						Ordinal = Convert.ToInt32(value),
						Name =
							resourceManager == null
								? $"**** {Enum.GetName(type, value)} ****"
								: resourceManager.GetString(Enum.GetName(type, value))
					});
			}

			return info;
		}

		#endregion
	}
}