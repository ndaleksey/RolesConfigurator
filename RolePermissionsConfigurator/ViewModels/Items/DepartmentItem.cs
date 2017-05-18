using System;
using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class DepartmentItem : BindableBase
	{
		#region Properties

		public Guid Id { get; set; }
		public Guid ParentId { get; set; }
		public string Name { get; set; }
		public int? Cluster { get; set; }
		public ESubordinate Subordinate { get; set; }

		#endregion

		#region Constructors

		public DepartmentItem()
		{
		}

		public DepartmentItem(Guid id, Guid parentId, string name, int? cluster)
		{
			Id = id;
			ParentId = parentId;
			Name = name;
			Cluster = cluster;
		}

		#endregion
	}
}