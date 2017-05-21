using System;
using System.Collections.Generic;
using System.Diagnostics;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class DepartmentEqualityComparer : IEqualityComparer<DepartmentItem>
	{
		public bool Equals(DepartmentItem x, DepartmentItem y)
		{
			if (x == null || y == null) throw new NullReferenceException("Один из операндов сравнения подзраделений равен null");

			return x.Cluster == y.Cluster;
		}

		public int GetHashCode(DepartmentItem obj)
		{
			return base.GetHashCode();
		}
	}
}