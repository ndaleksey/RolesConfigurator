using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public abstract class RolesViewModel : CustomViewModel
	{
		#region Fields

		private Role _selectedRole;

		#endregion

		#region Properties

		protected Guid CurrentClusterId { get; }

		public Role SelectedRole
		{
			get { return _selectedRole; }
			set { SetProperty(ref _selectedRole, value, nameof(SelectedRole)); }
		}

		public List<Subsystem> Subsystems { get; } = new List<Subsystem>();
		public ObservableCollection<Role> Roles { get; } = new ObservableCollection<Role>();

		#endregion

		#region Commands

		public ICommand AddRoleCommand { get; }
		public ICommand ModifyRoleCommand { get; }
		public ICommand DeleteRoleCommand { get; }

		#endregion

		#region Constructors

		protected RolesViewModel(Guid currentClusterId)
		{
			CurrentClusterId = currentClusterId;
			AddRoleCommand = new DelegateCommand(AddRole, CanAddRole);
			ModifyRoleCommand = new DelegateCommand(ModifyRole, CanModifyRole);
			DeleteRoleCommand = new DelegateCommand(DeleteRole, CanDeleteRole);
		}

		#endregion

		#region Commands' methods

		public virtual bool CanAddRole()
		{
			return false;
		}

		public virtual void AddRole()
		{
		}

		public virtual bool CanModifyRole()
		{
			return false;
		}

		public virtual void ModifyRole()
		{
		}

		public virtual bool CanDeleteRole()
		{
			return false;
		}

		public virtual void DeleteRole()
		{
		}

		#endregion
	}
}