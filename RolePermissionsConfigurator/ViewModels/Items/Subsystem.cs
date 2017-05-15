using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class Subsystem : BindableBase
	{
		#region Fields
		private int _number;
		private string _name;
		
		#endregion

		#region Properties
		public int Number
		{
			get { return _number; }
			set { SetProperty(ref _number, value, nameof(Number)); }
		}
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value, nameof(Name)); }
		}
		#endregion

		#region Constructors
		public Subsystem(int number, string name)
		{
			Number = number;
			Name = name;
		}
		#endregion

		#region Methods

		#endregion
	}
}