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
	public class InternalRoleSettingsViewModel : RoleSettingsViewModel
	{
		#region Properties
		public ObservableCollection<Account> Accounts { get; }

		#endregion

		#region Commands
		public ICommand SelectAllPluginsCommand { get; }
		#endregion

		#region Constructors

		public InternalRoleSettingsViewModel(Role modifiedRole, ICollection<Role> roles, IEnumerable<Account> accounts,
			EDialogOpenMode openMode) : base(modifiedRole, roles, openMode)
		{
			Accounts = new ObservableCollection<Account>(accounts);
			SelectAllPluginsCommand = new DelegateCommand(SelectAllPlugins);

			Initialization();
		}

		#endregion

		#region Commands' methods

		protected override bool CanApplyChanges()
		{
			return !string.IsNullOrEmpty(TempRole?.Name);
		}

		protected override async void ApplyChanges()
		{
			try
			{
				if (
					Roles.Where(role => role != null && role.Id != TempRole.Id)
						.Any(role => role.Number == TempRole.Number || string.Equals(role.Name, TempRole.Name)))
				{
					MessageBox.Show(Properties.Resources.RoleNameAndNumberUniqueRequirement,
						OpenMode == EDialogOpenMode.Insert ? Properties.Resources.RoleAddition : Properties.Resources.ModifyRole);
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
					if (OpenMode == EDialogOpenMode.Insert)
						await DbService.InsertRoleAsync(t.Connection, TempRole);
					else
						await DbService.UpdateRoleAsync(t.Connection, TempRole);

					await DbService.UpdateEnabledAccountsForRoleAsync(t.Connection, Accounts.Where(a => a.IsEnabled), TempRole.Id);

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

				// TODO : возможно можно убрать очистку
				/*permissionsForInsert.Clear();
				permissionsForDelete.Clear();
				pluginsForInsert.Clear();
				pluginsForDelete.Clear();
				pluginPermissionsForInsert.Clear();
				pluginPermissionsForUpdate.Clear();
				pluginPermissionsForDelete.Clear();*/

				SynchronizeRoleAccountsSelection();
				SynchronizeSubsystemPermissionSelection();
				SynchronizePluginRolesSelection();

				if (OpenMode == EDialogOpenMode.Insert)
					Roles.Add(TempRole);
				else
					CurrentRole.CopyFrom(TempRole);

				Cancel();
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);
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

		#endregion

		#region Methods

		protected sealed override void Initialization()
		{
			if (TempRole == null) throw new NullReferenceException();

			if (TempRole.Plugins.Where(p => p != null).All(p => p.IsSet))
				PluginSelectionStatus = true;
			else if (TempRole.Plugins.Where(p => p != null).All(p => !p.IsSet))
				PluginSelectionStatus = false;
			else
				PluginSelectionStatus = null;

			if (TempRole.SubsystemPermissions.Where(p => p != null).All(p => p.IsSet))
				SubsystemSelectionStatus = true;
			else if (TempRole.SubsystemPermissions.Where(p => p != null).All(p => !p.IsSet))
				SubsystemSelectionStatus = false;
			else
				SubsystemSelectionStatus = null;
			
			foreach (var account in Accounts)
			{
				// если пользователь занят
				if (account.Role != null)
				{
					account.IsSelected = true;
					// если пользователь принадлежит текущей роли
					account.IsEnabled = account.Role.Id == TempRole.Id;
				}
				else
				{
					account.IsSelected = false;
					account.IsEnabled = true;
				}
			}
		}

		private void SelectAllPlugins()
		{
			if (!PluginSelectionStatus.HasValue) return;

			foreach (var plugin in TempRole.Plugins.Where(p => p != null))
				plugin.IsSet = PluginSelectionStatus.Value;
		}

		private void InitializePluginPermissionsForInsertUpdateDelete(
			ICollection<PluginPermission> pluginPermissionsForInsert, ICollection<PluginPermission> pluginPermissionsForUpdate,
			ICollection<PluginPermission> pluginPermissionsForDelete)
		{
			for (var i = 0; i < TempRole.Plugins.Count; i++)
			{
				var oldPlugin = CurrentRole.Plugins[i];
				var newPlugin = TempRole.Plugins[i];

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
			for (var i = 0; i < TempRole.SubsystemPermissions.Count; i++)
			{
				var oldPermission = CurrentRole.SubsystemPermissions[i];
				var newPermission = TempRole.SubsystemPermissions[i];

				if (oldPermission.IsSet == newPermission.IsSet) continue;

				if (oldPermission.IsSet && !newPermission.IsSet) permissionsForDelete.Add(oldPermission);

				if (!oldPermission.IsSet && newPermission.IsSet) permissionsForInsert.Add(newPermission);
			}
		}

		private void InitializeRolePluginsForInsertAndDelete(ICollection<Plugin> pluginsForInsert,
			ICollection<Plugin> pluginsForDelete)
		{
			for (var i = 0; i < TempRole.Plugins.Count; i++)
			{
				var oldPlugin = CurrentRole.Plugins[i];
				var newplugin = TempRole.Plugins[i];

				if (oldPlugin.IsSet == newplugin.IsSet) continue;

				if (oldPlugin.IsSet && !newplugin.IsSet) pluginsForDelete.Add(oldPlugin);

				if (!oldPlugin.IsSet && newplugin.IsSet)
				{
					newplugin.Role = TempRole;
					pluginsForInsert.Add(newplugin);
				}
			}
		}

		private void SynchronizeSubsystemPermissionSelection()
		{
			for (var i = 0; i < TempRole.SubsystemPermissions.Count; i++)
				CurrentRole.SubsystemPermissions[i].IsSet = TempRole.SubsystemPermissions[i].IsSet;
		}

		private void SynchronizePluginRolesSelection()
		{
			for (var i = 0; i < TempRole.Plugins.Count; i++)
				CurrentRole.Plugins[i].IsSet = TempRole.Plugins[i].IsSet;
		}

		private void SynchronizeRoleAccountsSelection()
		{
			var selectedAccounts = Accounts.Where(a => a.IsEnabled && a.IsSelected);
			var deselectedAccounts = Accounts.Where(a => a.IsEnabled && !a.IsSelected);

			// добавить пользователя в роль, если он был выделен
			foreach (var account in selectedAccounts.Where(account => account.Role == null))
			{
				account.Role = TempRole;
				TempRole.Accounts.Add(account);
			}

			// удалить пользователя из роли, если было снято выделение
			foreach (
				var account in
					deselectedAccounts.Where(account => account.Role != null).Where(account => account.Role.Id == TempRole.Id))
			{
				account.Role = null;

				var acc =
					TempRole.Accounts.FirstOrDefault(
						a => string.Equals(a.Login, account.Login, StringComparison.CurrentCultureIgnoreCase));
				TempRole.Accounts.Remove(acc);
			}
		}
		#endregion
	}
}