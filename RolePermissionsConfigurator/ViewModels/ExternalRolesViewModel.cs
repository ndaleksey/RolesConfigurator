using System;
using System.Diagnostics;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class ExternalRolesViewModel : RolesViewModel
	{
		public ExternalRolesViewModel(Guid currentClusterId) : base(currentClusterId)
		{
			Initialization();
		}

		protected sealed override async void Initialization()
		{
			try
			{
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
		}
	}
}