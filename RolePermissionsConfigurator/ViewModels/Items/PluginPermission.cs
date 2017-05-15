using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class PluginPermission : BindableBase
	{
		#region Fields

		private string _invariantName;
		private string _displayName;
		private string _summary;
		private PluginPermissionType _type;
		private PluginPermissionValue _value;

		#endregion

		#region Properties

		public string InvariantName
		{
			get { return _invariantName; }
			set { SetProperty(ref _invariantName, value, nameof(InvariantName)); }
		}

		public string DisplayName
		{
			get { return _displayName; }
			set { SetProperty(ref _displayName, value, nameof(DisplayName)); }
		}

		public string Summary
		{
			get { return _summary; }
			set { SetProperty(ref _summary, value, nameof(Summary)); }
		}

		public PluginPermissionType Type
		{
			get { return _type; }
			set { SetProperty(ref _type, value, nameof(Type)); }
		}

		public PluginPermissionValue Value
		{
			get { return _value; }
			set { SetProperty(ref _value, value, nameof(Value)); }
		}

		public Plugin Plugin { get; }

		#endregion

		#region Constructors

		public PluginPermission(Plugin plugin, string invariantName, string displayName, PluginPermissionType type)
			: this(plugin, invariantName, displayName, type, null)
		{
		}

		public PluginPermission(Plugin plugin, string invariantName, string displayName, PluginPermissionType type, string summary)
		{
			Plugin = plugin;
			InvariantName = invariantName;
			DisplayName = displayName;
			Summary = summary;
			Type = type;
		}

		#endregion

		public static PluginPermission GetCopyFrom(PluginPermission permission)
		{
			var newPermission = new PluginPermission(permission.Plugin, permission.InvariantName, permission.DisplayName,
				permission.Type, permission.Summary) {Value = permission.Value};
			return newPermission;
		}
	}
}