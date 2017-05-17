using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public abstract class RoleSettingsViewModel : CustomViewModel
	{
		#region Fields

		private bool? _pluginSelectionStatus;
		private bool? _subsystemSelectionStatus;

		#endregion

		#region Properties

		public EDialogOpenMode OpenMode { get; }

		/// <summary>
		/// Изменяеммая текущая роль
		/// </summary>
		protected Role CurrentRole { get; set; }

		/// <summary>
		/// Изменяемая временная роль
		/// </summary>
		public Role TempRole { get; }

		public bool? PluginSelectionStatus
		{
			get { return _pluginSelectionStatus; }
			set { SetProperty(ref _pluginSelectionStatus, value, nameof(PluginSelectionStatus)); }
		}

		public bool? SubsystemSelectionStatus
		{
			get { return _subsystemSelectionStatus; }
			set { SetProperty(ref _subsystemSelectionStatus, value, nameof(SubsystemSelectionStatus)); }
		}

		protected ICollection<Role> Roles { get; }

		public ObservableCollection<SubsystemPermission> SubsystemPermissions { get; }

		#endregion

		#region Commands

		public ICommand ApplyChangesCommand { get; }
		public ICommand CancelCommand { get; }

		public ICommand SelectAllSubsystemsCommand { get; }

		#endregion

		#region Constructors

		protected RoleSettingsViewModel(Role modifiedRole, ICollection<Role> roles, EDialogOpenMode openMode)
		{
			SubsystemPermissions = new ObservableCollection<SubsystemPermission>();
			ApplyChangesCommand = new DelegateCommand(ApplyChanges, CanApplyChanges);
			CancelCommand = new DelegateCommand(Cancel, CanCancel);
			SelectAllSubsystemsCommand = new DelegateCommand(SelectAllPSubsystems);

			Roles = roles;
			OpenMode = openMode;
			CurrentRole = modifiedRole;
			TempRole = Role.GetCopyFrom(modifiedRole);
		}

		#endregion

		#region Commands Methods

		protected virtual void SelectAllPSubsystems()
		{
			if (!SubsystemSelectionStatus.HasValue) return;

			foreach (var permission in TempRole.SubsystemPermissions.Where(s => s != null))
				permission.IsSet = SubsystemSelectionStatus.Value;
		}

		protected virtual bool CanApplyChanges()
		{
			return false;
		}

		protected virtual void ApplyChanges()
		{
		}

		protected virtual bool CanCancel()
		{
			return true;
		}

		protected virtual void Cancel()
		{
			GetService<ICurrentDialogService>().Close();
		}

		#endregion
	}
}