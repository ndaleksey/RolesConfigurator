using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class RoleSettingsViewModel : CustomViewModel
	{
		#region Fields
		
		private readonly Role _modifiedRole;
		private readonly ICollection<Role> _roles;
		private readonly EDialogOpenMode _openMode;
		private bool? _pluginSelectionStatus;
		private bool? _subsystemSelectionStatus;
		#endregion

		#region Properties

		public Role ModifiedRole { get; }

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

		public ObservableCollection<Account> Accounts { get; }
		public ObservableCollection<SubsystemPermission> SubsystemPermissions { get; }

		#endregion

		#region Commands
		public ICommand ApplyChangesCommand { get; }
		public ICommand CancelCommand { get; }
		public ICommand SelectAllPluginsCommand { get; }
		public ICommand SelectAllSubsystemsCommand { get; }
		#endregion

		#region Constructors

		public RoleSettingsViewModel(Role modifiedRole, ICollection<Role> roles, IEnumerable<Account> accounts, EDialogOpenMode openMode)
		{
			Accounts = new ObservableCollection<Account>(accounts);
			SubsystemPermissions = new ObservableCollection<SubsystemPermission>();
			ApplyChangesCommand = new DelegateCommand(ApplyChanges, CanApplyChanges);
			CancelCommand = new DelegateCommand(Cancel, CanCancel);
			SelectAllPluginsCommand = new DelegateCommand(SelectAllPlugins);
			SelectAllSubsystemsCommand = new DelegateCommand(SelectAllPSubsystems);
			
			_roles = roles;
			_openMode = openMode;
			_modifiedRole = modifiedRole;
			ModifiedRole = Role.GetCopyFrom(modifiedRole);
			
			Initialization();
		}

		#endregion

		#region Commands Methods
		private void SelectAllPSubsystems()
		{
			if (!SubsystemSelectionStatus.HasValue) return;

			foreach (var permission in ModifiedRole.SubsystemPermissions.Where(s => s != null))
				permission.IsSet = SubsystemSelectionStatus.Value;
		}

		private void SelectAllPlugins()
		{
			if (!PluginSelectionStatus.HasValue) return;

			foreach (var plugin in ModifiedRole.Plugins.Where(p => p != null))
				plugin.IsSet = PluginSelectionStatus.Value;
		}

		private bool CanApplyChanges()
		{
			return true;
		}

		private async void ApplyChanges()
		{
			try
			{
				if (
					_roles.Where(role => role != null && role.Id != ModifiedRole.Id)
						.Any(role => role.Number == ModifiedRole.Number || string.Equals(role.Name, ModifiedRole.Name)))
				{
					MessageBox.Show(Properties.Resources.RoleNameAndNumberUniqueRequirement,
						_openMode == EDialogOpenMode.Insert ? Properties.Resources.RoleAddition : Properties.Resources.ModifyRole);
					return;
				}

				WorkflowType = EWorkflowType.SaveToDb;

				var permissionsForInsert = new List<SubsystemPermission>();
				var permissionsForDelete = new List<SubsystemPermission>();
				InitializeSubsystemPermissionsForInsertAndDelete(permissionsForInsert, permissionsForDelete);

				var pluginsForInsert = new List<Plugin>();
				var pluginsForDelete = new List<Plugin>();
				InitializeRolePluginsForInsertAndDelete(pluginsForInsert, pluginsForDelete);

				var pluginPermissionsForInsert = new List<PluginPermission>();
				var pluginPermissionsForUpdate = new List<PluginPermission>();
				var pluginPermissionsForDelete = new List<PluginPermission>();
				InitializePluginPermissionsForInsertUpdateDelete(pluginPermissionsForInsert, pluginPermissionsForUpdate,
					pluginPermissionsForDelete);

				using (var t = new Transaction())
				{
					if (_openMode == EDialogOpenMode.Insert)
						await DbService.InsertRoleAsync(t.Connection, ModifiedRole);
					else
						await DbService.UpdateRoleAsync(t.Connection, ModifiedRole);

					await DbService.UpdateEnabledAccountsForRoleAsync(t.Connection, Accounts.Where(a => a.IsEnabled), ModifiedRole.Id);

					if (permissionsForDelete.Any())
						await DbService.DeleteSubsystemPermissionsAsync(t.Connection, permissionsForDelete);

					if (permissionsForInsert.Any())
						await DbService.InsertSubsystemPermissionsAsync(t.Connection, permissionsForInsert);

					if (pluginsForDelete.Any())
						await DbService.DeleteRolePluginsAsync(t.Connection, pluginsForDelete);

					if (pluginsForInsert.Any())
						await DbService.InsertRolePluginsAsync(t.Connection, pluginsForInsert);

					if (pluginPermissionsForInsert.Any())
						await DbService.InsertPluginPermissionsAsync(t.Connection, pluginPermissionsForInsert);

					if (pluginPermissionsForUpdate.Any())
						await DbService.UpdatePluginPermissionsAsync(t.Connection, pluginPermissionsForUpdate);

					if (pluginPermissionsForDelete.Any())
						await DbService.DeletePluginPermissionsAsync(t.Connection, pluginPermissionsForDelete);

					t.Commit();
				}

				permissionsForInsert.Clear();
				permissionsForDelete.Clear();
				pluginsForInsert.Clear();
				pluginsForDelete.Clear();
				pluginPermissionsForInsert.Clear();
				pluginPermissionsForUpdate.Clear();
				pluginPermissionsForDelete.Clear();

				SynchronizeRoleAccountsSelection();
				SynchronizeSubsystemPermissionSelection();
				SynchronizePluginRolesSelection();

				if (_openMode == EDialogOpenMode.Insert)
					_roles.Add(ModifiedRole);
				else
					_modifiedRole.CopyFrom(ModifiedRole);

				Cancel();
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetDescriptionBySqlState(dbe.SqlState);
				Helper.Logger.Error(ELogMessageType.Process, e);
				Helper.Logger.Error(ELogMessageType.Process, dbe);

				MessageBox.Show(dbe.Message, LogMessages.WriteIntoDb);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(ELogMessageType.Process, e);
				MessageBox.Show(e.Message);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		private void InitializePluginPermissionsForInsertUpdateDelete(
			ICollection<PluginPermission> pluginPermissionsForInsert, ICollection<PluginPermission> pluginPermissionsForUpdate,
			ICollection<PluginPermission> pluginPermissionsForDelete)
		{
			for (var i = 0; i < ModifiedRole.Plugins.Count; i++)
			{
				var oldPlugin = _modifiedRole.Plugins[i];
				var newPlugin = ModifiedRole.Plugins[i];

				for (var j = 0; j < newPlugin.Permissions.Count; j++)
				{
					var oldPermission = oldPlugin.Permissions[j];
					var newPermission = newPlugin.Permissions[j];

					if (oldPermission.Value == null && newPermission.Value != null)
					{
						pluginPermissionsForInsert.Add(newPermission);
						continue;
					}

					if (oldPermission.Value != null && newPermission.Value != null)
					{
						pluginPermissionsForUpdate.Add(newPermission);
						continue;
					}

					if (oldPermission.Value != null && newPermission.Value == null)
						pluginPermissionsForDelete.Add(newPermission);
				}
			}
		}

		private void InitializeSubsystemPermissionsForInsertAndDelete(ICollection<SubsystemPermission> permissionsForInsert,
			ICollection<SubsystemPermission> permissionsForDelete)
		{
			for (var i = 0; i < ModifiedRole.SubsystemPermissions.Count; i++)
			{
				var oldPermission = _modifiedRole.SubsystemPermissions[i];
				var newPermission = ModifiedRole.SubsystemPermissions[i];

				if (oldPermission.IsSet == newPermission.IsSet) continue;

				if (oldPermission.IsSet && !newPermission.IsSet) permissionsForDelete.Add(oldPermission);

				if (!oldPermission.IsSet && newPermission.IsSet) permissionsForInsert.Add(newPermission);
			}
		}

		private void InitializeRolePluginsForInsertAndDelete(ICollection<Plugin> pluginsForInsert,
			ICollection<Plugin> pluginsForDelete)
		{
			for (var i = 0; i < ModifiedRole.Plugins.Count; i++)
			{
				var oldPlugin = _modifiedRole.Plugins[i];
				var newplugin = ModifiedRole.Plugins[i];

				if (oldPlugin.IsSet == newplugin.IsSet) continue;

				if (oldPlugin.IsSet && !newplugin.IsSet) pluginsForDelete.Add(oldPlugin);

				if (!oldPlugin.IsSet && newplugin.IsSet)
				{
					newplugin.Role = ModifiedRole;
					pluginsForInsert.Add(newplugin);
				}
			}
		}

		private void SynchronizeSubsystemPermissionSelection()
		{
			for (var i = 0; i < ModifiedRole.SubsystemPermissions.Count; i++)
				_modifiedRole.SubsystemPermissions[i].IsSet = ModifiedRole.SubsystemPermissions[i].IsSet;
		}

		private void SynchronizePluginRolesSelection()
		{
			for (var i = 0; i < ModifiedRole.Plugins.Count; i++)
				_modifiedRole.Plugins[i].IsSet = ModifiedRole.Plugins[i].IsSet;
		}

		private void SynchronizeRoleAccountsSelection()
		{
			var selectedAccounts = Accounts.Where(a => a.IsEnabled && a.IsSelected);
			var deselectedAccounts = Accounts.Where(a => a.IsEnabled && !a.IsSelected);

			// добавить пользователя в роль, если он был выделен
			foreach (var account in selectedAccounts.Where(account => account.Role == null))
			{
				account.Role = ModifiedRole;
				ModifiedRole.Accounts.Add(account);
			}

			// удалить пользователя из роли, если было снято выделение
			foreach (
				var account in
					deselectedAccounts.Where(account => account.Role != null).Where(account => account.Role.Id == ModifiedRole.Id))
			{
				account.Role = null;

				var acc =
					ModifiedRole.Accounts.FirstOrDefault(
						a => string.Equals(a.Login, account.Login, StringComparison.CurrentCultureIgnoreCase));
				ModifiedRole.Accounts.Remove(acc);
			}
		}

		private bool CanCancel()
		{
			return true;
		}

		private void Cancel()
		{
			GetService<ICurrentDialogService>().Close();
		}
		#endregion

		#region Methods

		protected override void Initialization()
		{
			if (ModifiedRole != null)
			{
				if (ModifiedRole.Plugins.Where(p => p != null).All(p => p.IsSet))
					PluginSelectionStatus = true;
				else if (ModifiedRole.Plugins.Where(p => p != null).All(p => !p.IsSet))
					PluginSelectionStatus = false;
				else
					PluginSelectionStatus = null;

				if (ModifiedRole.SubsystemPermissions.Where(p => p != null).All(p => p.IsSet))
					SubsystemSelectionStatus = true;
				else if (ModifiedRole.SubsystemPermissions.Where(p => p != null).All(p => !p.IsSet))
					SubsystemSelectionStatus = false;
				else
					SubsystemSelectionStatus = null;
			}

			foreach (var account in Accounts)
			{
				// если пользователь занят
				if (account.Role != null)
				{
					account.IsSelected = true;
					// если пользователь принадлежит текущей роли
					account.IsEnabled = account.Role.Id == ModifiedRole.Id;
				}
				else
				{
					account.IsSelected = false;
					account.IsEnabled = true;
				}
			}
		}

		#endregion
	}
}