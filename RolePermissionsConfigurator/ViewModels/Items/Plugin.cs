using System.Collections.ObjectModel;
using System.Linq;
using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class Plugin : BindableBase
	{
		#region Fields

		private string _name;
		private string _displayName;
		private string _summary;
		private string _value;
		private Role _role;
		private bool _isSet;

		#endregion

		#region Properties

		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value, nameof(Name)); }
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

		public string Value
		{
			get { return _value; }
			set { SetProperty(ref _value, value, nameof(Value)); }
		}

		public Role Role
		{
			get { return _role; }
			set { SetProperty(ref _role, value, nameof(Role)); }
		}

		public bool IsSet
		{
			get { return _isSet; }
			set { SetProperty(ref _isSet, value, nameof(IsSet)); }
		}

		public Collection<PluginPermission> Permissions { get; }

		#endregion

		#region Constructors

		public Plugin(string name, string displayName = "", string summary = "")
		{
			Name = name;
			DisplayName = displayName;
			Summary = summary;
			Permissions = new ObservableCollection<PluginPermission>();
		}

		public Plugin(string name, Role role) : this(name)
		{
			Role = role;
		}

		#endregion

		#region Methods
		public static Plugin GetCopyFrom(Plugin plugin)
		{
			var newPlugin = new Plugin(plugin.Name, plugin.DisplayName, plugin.Summary) {IsSet = plugin.IsSet};
			foreach (var newPermission in plugin.Permissions.Select(PluginPermission.GetCopyFrom))
			{
				newPermission.Plugin.Role = plugin.Role;
				newPlugin.Permissions.Add(newPermission);
			}
			return newPlugin;
		}
		#endregion

	}
}