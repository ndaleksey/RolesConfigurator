using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class Role : BindableBase
	{
		#region Fields

		private int _number;
		private string _name;
		private string _description;
		private string _department;
		
		#endregion

		#region Properties

		public Guid Id { get; private set; }

		public Guid ClusterId { get; set; }
		
		public int Number
		{
			get { return _number; }
			set { SetProperty(ref _number, value, nameof(Number)); }
		}
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value, nameof(Name)); }
		}
		public string Description
		{
			get { return _description; }
			set { SetProperty(ref _description, value, nameof(Description)); }
		}

		public string Department
		{
			get { return _department; }
			set { SetProperty(ref _department, value, nameof(Department)); }
		}

		public ObservableCollection<Account> Accounts { get; }
		public ObservableCollection<Plugin> Plugins { get; }
		public ObservableCollection<SubsystemPermission> SubsystemPermissions { get; }

		#endregion

		#region Constructors
		
		public Role(int number, string name, string description = null, string department = null)
		{
			Number = number;
			Name = name;
			Description = description;
			Department = department;
			Accounts = new ObservableCollection<Account>();
			Plugins = new ObservableCollection<Plugin>();
			SubsystemPermissions = new ObservableCollection<SubsystemPermission>();
		}

		public Role(Guid id, Guid clusterId, int number, string name, string description = null, string department = null)
			: this(number, name, description, department)
		{
			Id = id;
			ClusterId = clusterId;
		}

		#endregion

		#region Methods

		public static Role GetCopyFrom(Role role)
		{
			var newRole = new Role(role.Id, role.ClusterId, role.Number, role.Name, role.Description);

			foreach (var account in role.Accounts)
				newRole.Accounts.Add(account);

			foreach (var plugin in role.Plugins)
				newRole.Plugins.Add(Plugin.GetCopyFrom(plugin));

			foreach (var p in role.SubsystemPermissions)
				newRole.SubsystemPermissions.Add(SubsystemPermission.GetCopyFrom(p));
			
			return newRole;
		}

		public void CopyFrom(Role role)
		{
			Id = role.Id;
			Number = role.Number;
			Name = role.Name;
			Description = role.Description;

			Accounts.Clear();

			foreach (var account in role.Accounts)
				Accounts.Add(account);

			Plugins.Clear();

			foreach (var plugin in role.Plugins)
				Plugins.Add(plugin);

			SubsystemPermissions.Clear();

			foreach (var subsystemPermission in role.SubsystemPermissions)
				SubsystemPermissions.Add(subsystemPermission);
		}
		#endregion
	}
}