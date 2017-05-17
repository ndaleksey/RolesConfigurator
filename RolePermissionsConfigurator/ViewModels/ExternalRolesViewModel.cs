using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
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
						new InternalRoleSettingsViewModel(newRole, Roles, null, EDialogOpenMode.Insert));

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

		#endregion

		#region Methods
		protected sealed override async void Initialization()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				using (var t = new Transaction())
				{
					var roles = await DbService.GetRolesExceptClusterIdAsync(t.Connection, CurrentClusterId);
					Roles.Clear();
					foreach (var role in roles)
					{
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