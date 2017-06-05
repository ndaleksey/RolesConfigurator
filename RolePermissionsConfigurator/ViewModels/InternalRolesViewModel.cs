using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using DevExpress.Mvvm;
using NLog.Internal;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Configuration;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;
using Swsu.Lignis.Workstation.Contract.Permissions.Metadata;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class InternalRolesViewModel : RolesViewModel
	{
		#region Fields

		private readonly string _currentDepartment;
		private readonly List<Plugin> _pluginsFromConfig = new List<Plugin>();
		private List<Account> _accounts;

		#endregion

		#region Properties

		#endregion

		#region Constructors

		public InternalRolesViewModel(Guid currentClusterId, string currentDepartment) : base(currentClusterId)
		{
			_currentDepartment = currentDepartment;
			Initialization();
		}

		#endregion

		#region Commands methods

		public override bool CanAddRole()
		{
			return true;
		}

		public override void AddRole()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				var maxNumber = Roles.Count > 0 ? Roles.Max(r => r.Number) + 1 : 1;
				var newRole = new Role(Guid.NewGuid(), CurrentClusterId, maxNumber,
					$"{Properties.Resources.NewRole} {maxNumber}", string.Empty, _currentDepartment);

				foreach (var subsystem in Subsystems)
					newRole.SubsystemPermissions.Add(new SubsystemPermission(newRole, subsystem));

				foreach (var plugin in _pluginsFromConfig)
				{
					var p = Plugin.GetCopyFrom(plugin);
					p.Role = newRole;

					newRole.Plugins.Add(p);
				}

				GetService<IDialogService>("RoleSettingsService")
					.ShowDialog(null, Properties.Resources.RoleAddition, null,
						new InternalRoleSettingsViewModel(newRole, Roles, _accounts, EDialogOpenMode.Insert));
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);
				
				MessageBox.Show(e, LogMessages.ReadFromDB);

				Helper.Logger.Error(ELogMessageType.Process, e);
				Helper.Logger.Error(ELogMessageType.Process, dbe);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				MessageBox.Show(e.Message);
				Helper.Logger.Error(ELogMessageType.Process, e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		public override bool CanModifyRole()
		{
			return SelectedRole != null;
		}

		public override void ModifyRole()
		{
			try
			{
				WorkflowType = EWorkflowType.WorkWithDb;
				GetService<IDialogService>("RoleSettingsService")
					.ShowDialog(null, Properties.Resources.RoleModification, null,
						new InternalRoleSettingsViewModel(SelectedRole, Roles, _accounts, EDialogOpenMode.Update));
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);
				Helper.Logger.Error(ELogMessageType.Process, e);
				Helper.Logger.Error(ELogMessageType.Process, dbe);

				MessageBox.Show(e);
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

		public override bool CanDeleteRole()
		{
			return SelectedRole != null;
		}

		public override async void DeleteRole()
		{
			try
			{
				if (MessageBox.Show($"{Properties.Resources.DeleteRoleAcquirement} \"{SelectedRole.Name}\"?",
					Properties.Resources.RoleDeleting,
					MessageBoxButton.YesNo) != MessageBoxResult.Yes)
					return;

				WorkflowType = EWorkflowType.WorkWithDb;

				using (var t = new Transaction())
				{
					await DbService.DeleteRoleAsync(t.Connection, SelectedRole);
					t.Commit();
				}

				foreach (var account in SelectedRole.Accounts)
					foreach (var a in _accounts.Where(a => a.Login == account.Login))
						a.Role = null;

				SelectedRole.Accounts.Clear();
				Roles.Remove(SelectedRole);
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);
				Helper.Logger.Error(ELogMessageType.WriteIntoDb, dbe);
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

		protected sealed override async void Initialization()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				try
				{
					LoadPluginsFromPluginsConfig();
				}
				catch (Exception e)
				{
					Helper.Logger.Error(ELogMessageType.ReadFromFile, e);
					Debug.WriteLine(e);
				}
				await SynchronizeAssountsAndUsersInDbAsync();
				await LoadFullRolesInfoAsync();
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);
				Helper.Logger.Error(ELogMessageType.Process, e);
				Helper.Logger.Error(ELogMessageType.Process, dbe);

				MessageBox.Show(e, LogMessages.ReadFromDB);
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

		/// <summary>
		/// Метод синхронизирет пользователей СУБД и пользователей в таблице permission.account
		/// </summary>
		/// <returns>Возвращает Task</returns>
		private async Task SynchronizeAssountsAndUsersInDbAsync()
		{
			using (var t = new Transaction())
			{
				await DbService.SynchronizeAccountsAndUsersAsync(t.Connection);
				t.Commit();
			}
		}

		private async Task LoadFullRolesInfoAsync()
		{
			List<Role> roles;
			List<Tuple<int, Guid>> subsystemsPermissions;
			List<Tuple<Guid, string>> plugins;
			List<Tuple<Guid, string, string, short>> pluginsPermissions;

			using (var t = new Transaction())
			{
				roles = await DbService.GetRolesByClusterIdAsync(t.Connection, CurrentClusterId);
				_accounts = await DbService.GetAccountsAsync(t.Connection);
				subsystemsPermissions = await DbService.GetSubsystemsPermissionsAsTuppleAsync(t.Connection);
				plugins = await DbService.GetRolePluginsAsTuppleAsync(t.Connection);
				pluginsPermissions = await DbService.GetPluginsPermissionsAsTuppleAsync(t.Connection);
			}

			foreach (var role in roles)
			{
				role.Plugins.Clear();

				foreach (var p in _pluginsFromConfig)
				{
					var rolePlugin = new Plugin(p.Name, role) {DisplayName = p.DisplayName, Summary = p.Summary};

					foreach (var permission in p.Permissions)
					{
						var type = new PluginPermissionType();

						var pluginPermission = pluginsPermissions.FirstOrDefault(
							pp =>
								pp.Item1 == role.Id
								&& string.Equals(pp.Item2, rolePlugin.Name, StringComparison.InvariantCultureIgnoreCase)
								&& string.Equals(pp.Item3, permission.InvariantName));

						foreach (var permissionValue in permission.Type.Values)
							type.Values.Add(new PluginPermissionValue(permissionValue.Value, permissionValue.DisplayName));

						rolePlugin.Permissions.Add(new PluginPermission(rolePlugin, permission.InvariantName, permission.DisplayName, type,
							permission.Summary)
						{
							Value = pluginPermission == null
								? null
								: type.Values.FirstOrDefault(v => v.Value == pluginPermission.Item4)
						});
					}

					role.Plugins.Add(rolePlugin);
				}

				role.SubsystemPermissions.Clear();
				foreach (var subsystem in Subsystems)
				{
					var isSet = subsystemsPermissions.Any(p => p.Item1 == subsystem.Number && p.Item2 == role.Id);
					role.SubsystemPermissions.Add(new SubsystemPermission(role, subsystem) {IsSet = isSet});
				}

				foreach (var plugin in role.Plugins)
					if (plugins.Where(p => p.Item1 == role.Id).Any(p => p.Item2 == plugin.Name))
						plugin.IsSet = true;

				role.Accounts.Clear();
				foreach (var acc in _accounts.Where(a => a.Role != null && a.Role.Id == role.Id))
				{
					acc.Role = role;
					role.Accounts.Add(acc);
				}
				Roles.Add(role);
			}
		}

		/// <summary>
		/// Загрузка плагинов из файла Plugins.config
		/// </summary>
		private void LoadPluginsFromPluginsConfig()
		{
			_pluginsFromConfig.Clear();

			PluginsCatalog pluginsCatalog;
			var serializer = new XmlSerializer(typeof (PluginsCatalog));

			using (var stream = new FileStream(Settings.Default.PluginsCatalog, FileMode.OpenOrCreate))
			{
				pluginsCatalog = (PluginsCatalog) serializer.Deserialize(stream);
				stream.Close();
			}
			
			foreach (var p in pluginsCatalog.Adds)
			{
				var plugin = new Plugin(p.Name, p.GetDisplayName(CultureInfo.CurrentUICulture), p.GetDescription(CultureInfo.CurrentUICulture));

				try
				{
					var table = PermissionTable.Load(p.Assembly, CultureInfo.CurrentUICulture);

					foreach (var permission in table.Permissions)
					{
						var type = new PluginPermissionType();

						foreach (var permissionValue in permission.Type.Values)
							type.Values.Add(new PluginPermissionValue(permissionValue.Value, permissionValue.DisplayName,
								permissionValue.Summary));

						plugin.Permissions.Add(new PluginPermission(plugin, permission.InvariantName, permission.DisplayName, type,
							permission.Summary));
					}
				}
				catch (Exception e)
				{
					Helper.Logger.Error(ELogMessageType.ReadFromFile, e);
				}
				_pluginsFromConfig.Add(plugin);
			}
		}

		#endregion
	}
}