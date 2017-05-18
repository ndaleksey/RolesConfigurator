using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class ExternalRoleSettingsViewModel : RoleSettingsViewModel
	{
		#region Fields

		private Department _selectedDepartment;
		#endregion

		#region Properties

		public Department SelectedDepartment
		{
			get { return _selectedDepartment; }
			set { SetProperty(ref _selectedDepartment, value, nameof(SelectedDepartment)); }
		}
		public ObservableCollection<Department> Departments { get; } = new ObservableCollection<Department>();
		#endregion

		#region Constructors
		public ExternalRoleSettingsViewModel(Role modifiedRole, ICollection<Role> roles, EDialogOpenMode openMode)
			: base(modifiedRole, roles, openMode)
		{
			Initialization();
		}
		#endregion

		#region Commands' methods

		protected override bool CanApplyChanges()
		{
			return SelectedDepartment != null && !string.IsNullOrEmpty(TempRole?.Name);
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

				TempRole.ClusterId = SelectedDepartment.Id;
				TempRole.Department = SelectedDepartment.Name;

				using (var t = new Transaction())
				{
					if (OpenMode == EDialogOpenMode.Insert)
						await DbService.InsertRoleAsync(t.Connection, TempRole);
					else
						await DbService.UpdateRoleAsync(t.Connection, TempRole);


					if (permissionsForDelete.Any())
						await DbService.DeleteSubsystemPermissionsAsync(t.Connection, permissionsForDelete);

					if (permissionsForInsert.Any())
						await DbService.InsertSubsystemPermissionsAsync(t.Connection, permissionsForInsert);
					
					t.Commit();
				}

				SynchronizeSubsystemPermissionSelection();

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

		protected sealed override async void Initialization()
		{
			using (var t = new Transaction())
			{
				var departments = await DbService.GetDepartmentsWithClusterAsync(t.Connection);

				Departments.Clear();

				foreach (var department in departments)
					Departments.Add(department);

				var dep = Departments.FirstOrDefault(d => d.Id == TempRole.ClusterId);

				// Если новая роль, то удалить кластер текущего пользователя, иначе - установить текущее подразделение
				if (OpenMode == EDialogOpenMode.Update)
					SelectedDepartment = dep;
				else
					if (dep != null)
						Departments.Remove(dep);
			}
		}

		private void SynchronizeSubsystemPermissionSelection()
		{
			for (var i = 0; i < TempRole.SubsystemPermissions.Count; i++)
				CurrentRole.SubsystemPermissions[i].IsSet = TempRole.SubsystemPermissions[i].IsSet;
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

		#endregion
	}
}