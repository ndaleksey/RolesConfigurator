using System.Collections.Generic;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class DepartmentEqualityComparer : IEqualityComparer<DepartmentItem>
	{
		public bool Equals(DepartmentItem x, DepartmentItem y)
		{
			return x.Cluster == y.Cluster;
		}

		public int GetHashCode(DepartmentItem obj)
		{
			return base.GetHashCode();
		}
	}
}