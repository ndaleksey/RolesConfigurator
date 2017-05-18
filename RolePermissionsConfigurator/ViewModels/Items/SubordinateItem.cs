using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class SubordinateItem : BindableBase
	{
		#region Fields

		private ESubordinate _subordinate;

		#endregion

		#region Properties

		public ESubordinate Subordinate
		{
			get { return _subordinate; }
			set { SetProperty(ref _subordinate, value, nameof(Subordinate)); }
		}

		public int Id => (int) _subordinate;

		public string TypeName
		{
			get
			{
				switch (_subordinate)
				{
					case ESubordinate.Superior:
						return Properties.Resources.Superior;
					case ESubordinate.Interacting:
						return Properties.Resources.Interacting;
					case ESubordinate.Inferior:
						return Properties.Resources.Inferior;
					default:
						return "Unknown";
				}
			}
		}

		#endregion

		#region Constructors

		public SubordinateItem(ESubordinate subordinate)
		{
			_subordinate = subordinate;
		}

		#endregion
	}
}