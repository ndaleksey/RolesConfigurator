using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class ExternalRolesViewModel : RolesViewModel
	{
		#region Constructors
		public ExternalRolesViewModel(Guid currentClusterId) : base(currentClusterId)
		{
			Initialization();
		}
		#endregion

		#region Commands' methods

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
					$"{Properties.Resources.NewRole} {maxNumber}", string.Empty);

				foreach (var subsystem in Subsystems)
					newRole.SubsystemPermissions.Add(new SubsystemPermission(newRole, subsystem));
				
				GetService<IDialogService>("RoleSettingsService")
					.ShowDialog(null, Properties.Resources.RoleAddition, null,
						new ExternalRoleSettingsViewModel(newRole, Roles, EDialogOpenMode.Insert));

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
						new ExternalRoleSettingsViewModel(SelectedRole, Roles, EDialogOpenMode.Update));
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

				using (var t = new Transaction())
				{
					var subsystemsPermissions = await DbService.GetSubsystemsPermissionsAsTuppleAsync(t.Connection);
					var roles = await DbService.GetRolesExceptClusterIdAsync(t.Connection, CurrentClusterId);

					Roles.Clear();

					foreach (var role in roles)
					{
						role.SubsystemPermissions.Clear();
						foreach (var subsystem in Subsystems)
						{
							var isSet = subsystemsPermissions.Any(p => p.Item1 == subsystem.Number && p.Item2 == role.Id);
							role.SubsystemPermissions.Add(new SubsystemPermission(role, subsystem) {IsSet = isSet});
						}
						Roles.Add(role);
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}
		#endregion
	}
}