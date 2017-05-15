using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;
using Swsu.Lignis.Workstation.Contract.Permissions.Metadata;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class MainViewModel : CustomViewModel
	{
		#region Fields

		private string _appTitle;
		private Guid? _currentClusterId;
		private Role _selectedRole;
		private Account _selectedAccount;
		private Subsystem _selectedSubsystem;
		private readonly List<Plugin> _pluginsFromConfig;

		#endregion

		#region Properties

		public string AppTitle
		{
			get { return _appTitle; }
			set { SetProperty(ref _appTitle, value, nameof(AppTitle)); }
		}
		public Role SelectedRole
		{
			get { return _selectedRole; }
			set { SetProperty(ref _selectedRole, value, nameof(SelectedRole)); }
		}

		public Account SelectedAccount
		{
			get { return _selectedAccount; }
			set { SetProperty(ref _selectedAccount, value, nameof(SelectedAccount)); }
		}

		public Subsystem SelectedSubsystem
		{
			get { return _selectedSubsystem; }
			set { SetProperty(ref _selectedSubsystem, value, nameof(SelectedSubsystem)); }
		}

		public ObservableCollection<Role> Roles { get; }
		public ObservableCollection<Account> Accounts { get; }
		public ObservableCollection<Subsystem> Subsystems { get; }

		#endregion

		#region Commands

		public ICommand AddRoleCommand { get; }
		public ICommand ModifyRoleCommand { get; }
		public ICommand DeleteRoleCommand { get; }

		public ICommand InitializeCommand { get; }

		#endregion

		#region Constructors

		public MainViewModel()
		{
			WorkflowType = EWorkflowType.NormalWork;
			_pluginsFromConfig = new List<Plugin>();
			Roles = new ObservableCollection<Role>();
			Accounts = new ObservableCollection<Account>();
			Subsystems = new ObservableCollection<Subsystem>();

			AddRoleCommand = new DelegateCommand(AddRole, CanAddRole);
			ModifyRoleCommand = new DelegateCommand(ModifyRole, CanModifyRole);
			DeleteRoleCommand = new DelegateCommand(DeleteRole, CanDeleteRole);

			InitializeCommand = new DelegateCommand(Initialization);
		}

		#endregion

		#region Commands methods

		private bool CanAddRole()
		{
			return true;
		}

		private void AddRole()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				var maxNumber = Roles.Count > 0 ? Roles.Max(r => r.Number) + 1 : 1;
				var newRole = new Role(Guid.NewGuid(), _currentClusterId.Value, maxNumber,
					$"{Properties.Resources.NewRole} {maxNumber}", string.Empty);

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
						new RoleSettingsViewModel(newRole, Roles, Accounts, EDialogOpenMode.Insert));

				SelectedRole = newRole;
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

		private bool CanModifyRole()
		{
			return SelectedRole != null;
		}

		private void ModifyRole()
		{
			try
			{
				WorkflowType = EWorkflowType.WorkWithDb;
				GetService<IDialogService>("RoleSettingsService")
					.ShowDialog(null, Properties.Resources.RoleModification, null,
						new RoleSettingsViewModel(SelectedRole, Roles, Accounts, EDialogOpenMode.Update));
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetDescriptionBySqlState(dbe.SqlState);
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

		private bool CanDeleteRole()
		{
			return SelectedRole != null;
		}

		private async void DeleteRole()
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
					foreach (var a in Accounts.Where(a => a.Login == account.Login))
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

		private async void Initialization()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				using (var t = new Transaction())
					_currentClusterId = await DbService.GetCurrentRoleClusterAsync(t.Connection);

				if (_currentClusterId == null)
				{
					MessageBox.Show(Properties.Resources.RoleNotExistsException);
					return;
				}

				await CreateApplicationTitleAsync();

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

				var e = Helper.GetDescriptionBySqlState(dbe.SqlState);
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

		private async Task CreateApplicationTitleAsync()
		{
			string depratment;

			using (var t = new Transaction())
				depratment = await DbService.GetDepartmentNameByClusterIdAsync(t.Connection, _currentClusterId.Value);

			AppTitle = Properties.Resources.ApplicationName;

			if (!string.IsNullOrEmpty(depratment))
				AppTitle += $"  ( {depratment} )";
		}

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
			List<Account> accounts;
			List<Subsystem> subsystems;
			List<Tuple<int, Guid>> subsystemsPermissions;
			List<Tuple<Guid, string>> plugins;
			List<Tuple<Guid, string, string, short>> pluginsPermissions;

			using (var t = new Transaction())
			{
				roles = await DbService.GetRolesByClusterIdAsync(t.Connection, _currentClusterId.Value);
				accounts = await DbService.GetAccountsAsync(t.Connection);
				subsystems = await DbService.GetSubsystemsAsync(t.Connection);
				subsystemsPermissions = await DbService.GetSubsystemsPermissionsAsTuppleAsync(t.Connection);
				plugins = await DbService.GetRolePluginsAsTuppleAsync(t.Connection);
				pluginsPermissions = await DbService.GetPluginsPermissionsAsTuppleAsync(t.Connection);
			}

			foreach (var role in roles)
			{
				role.Plugins.Clear();

				foreach (var p in _pluginsFromConfig)
				{
					var rolePlugin = new Plugin(p.Name, role);

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
				foreach (var subsystem in subsystems)
				{
					var isSet = subsystemsPermissions.Any(p => p.Item1 == subsystem.Number && p.Item2 == role.Id);
					role.SubsystemPermissions.Add(new SubsystemPermission(role, subsystem) {IsSet = isSet});
				}

				foreach (var plugin in role.Plugins)
					if (plugins.Where(p => p.Item1 == role.Id).Any(p => p.Item2 == plugin.Name))
						plugin.IsSet = true;

				role.Accounts.Clear();
				foreach (var acc in accounts.Where(a => a.Role != null && a.Role.Id == role.Id))
				{
					acc.Role = role;
					role.Accounts.Add(acc);
				}
				Roles.Add(role);
			}

			Accounts.Clear();

			foreach (var account in accounts)
				Accounts.Add(account);

			Subsystems.Clear();

			foreach (var subsystem in subsystems)
				Subsystems.Add(subsystem);

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

			foreach (var p in pluginsCatalog)
			{
				var plugin = new Plugin(p.Name);
				
				try
				{
					var table = PermissionTable.Load(p.Assembly, CultureInfo.CurrentUICulture);

					foreach (var permission in table.Permissions)
					{
						var type = new PluginPermissionType();

						foreach (var permissionValue in permission.Type.Values)
							type.Values.Add(new PluginPermissionValue(permissionValue.Value, permissionValue.DisplayName, permissionValue.Summary));

						plugin.Permissions.Add(new PluginPermission(plugin, permission.InvariantName, permission.DisplayName, type,
							permission.Summary));
					}
				}
				catch (Exception)
				{
					// ignored
				}
				_pluginsFromConfig.Add(plugin);
			}
		}

		#endregion
	}
}