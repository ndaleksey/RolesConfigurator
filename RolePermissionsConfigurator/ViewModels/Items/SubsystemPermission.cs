using System;
using System.Windows.Input;
using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class SubsystemPermission : BindableBase
	{
		#region Fields
		private bool _isSet;
		private Role _role;
		private Subsystem _subsystem;

		#endregion

		#region Properties

		public Subsystem Subsystem
		{
			get { return _subsystem; }
			set { SetProperty(ref _subsystem, value, nameof(Subsystem)); }
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

		#endregion

		#region Events

//		public event EventHandler<PropertyChangedEventArgs<bool>> IsSetChanged;

		#endregion

		#region Commands
		public ICommand ModifyPermissionCommand { get; }

		#endregion

		#region Constructors

		public SubsystemPermission()
		{
			ModifyPermissionCommand = new DelegateCommand(ModifyPermission);
		}
		
		public SubsystemPermission(Role role, Subsystem subsystem)
		{
			Role = role;
			Subsystem = subsystem;
		}

		private void ModifyPermission()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Methods

		public static SubsystemPermission GetCopyFrom(SubsystemPermission permission)
		{
			var newPermission = new SubsystemPermission(permission.Role, permission.Subsystem) {IsSet = permission.IsSet};
			return newPermission;
		}

		#endregion
	}
}