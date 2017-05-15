using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class PluginPermissionValue : BindableBase
	{
		#region Fields
		private short _value;
		private string _displayName;
		private string _summary;
		#endregion

		#region Properties
		public short Value
		{
			get { return _value; }
			set { SetProperty(ref _value, value, nameof(Value)); }
		}
		public string DisplayName
		{
			get { return _displayName; }
			set { SetProperty(ref _displayName, value, nameof(DisplayName)); }
		}
		public string Summary
		{
			get { return _summary; }
			set { SetProperty(ref _summary, value, nameof(Summary)); }
		}
		#endregion

		#region Constructors

		public PluginPermissionValue(short value, string displayName) : this(value, displayName, null)
		{
			
		}

		public PluginPermissionValue(short value, string displayName, string summary)
		{
			Value = value;
			DisplayName = displayName;
			Summary = summary;
		}
		#endregion

		#region Methods

		public override string ToString()
		{
			return DisplayName;
		}

		#endregion
	}
}