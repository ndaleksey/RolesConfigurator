using System;
using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class MilitaryUnit : BindableBase
	{
		#region Fields

		private string _name;

		#endregion

		#region Properties

		public Guid Id { get; set; }

		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value, nameof(Name)); }
		}

		#endregion

		#region Constructions

		public MilitaryUnit(Guid id, string name)
		{
			Id = id;
			_name = name;
		}

		#endregion
	}
}