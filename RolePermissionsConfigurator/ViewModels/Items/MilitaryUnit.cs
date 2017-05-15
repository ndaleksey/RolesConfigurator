using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class MilitaryUnit : BindableBase
	{
		#region Properties
		public string Id { get; set; }
		public string ParentId { get; set; }
		public string Name { get; set; }
		public int? Cluster { get; set; }
		#endregion

		#region Constructors

		public MilitaryUnit(string id, string parentId, string name, int? cluster)
		{
			Id = id;
			ParentId = parentId;
			Name = name;
			Cluster = cluster;
		}

		#endregion
	}
}