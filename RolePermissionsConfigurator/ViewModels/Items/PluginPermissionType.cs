using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class PluginPermissionType : BindableBase
	{
		#region Properties

		public ObservableCollection<PluginPermissionValue> Values { get; }

		#endregion

		#region Constructors

		public PluginPermissionType()
		{
			Values = new ObservableCollection<PluginPermissionValue>();
		}

		#endregion
	}
}