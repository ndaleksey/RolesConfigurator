using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class DepartmentClustersViewModel : CustomViewModel
	{
		#region Fields

		#endregion

		#region Properties

		public ObservableCollection<MilitaryUnit> MilitaryUnits { get; } = new ObservableCollection<MilitaryUnit>();

		#endregion

		#region Constructors

		public DepartmentClustersViewModel()
		{
			Initialization();
		}

		#endregion

		#region Methods

		protected sealed override async void Initialization()
		{
			try
			{
				await LoadGroupingAsync();
			}
			catch (Exception e)
			{
				//TODO: логирование
				Debug.WriteLine(e);
			}
		}

		private async Task LoadGroupingAsync()
		{
			using (var t = new Transaction())
			{
				var inferiorUnits =
					await DbService.GetMilitaryUnitsByGroupingNameAsync(t.Connection, "permission.inferior_grouping");
				var superiorUnits =
					await DbService.GetMilitaryUnitsByGroupingNameAsync(t.Connection, "permission.superior_grouping");
				var interactingUnits =
					await DbService.GetMilitaryUnitsByGroupingNameAsync(t.Connection, "permission.interacting_grouping");


				var interactingRoot = new MilitaryUnit("1", "0", Properties.Resources.Interacting, null);
				var inferiorRoot = new MilitaryUnit("2", "0", Properties.Resources.Inferior, null);
				var superiorRoot = new MilitaryUnit("3", "0", Properties.Resources.Superior, null);

				var intRoot = interactingUnits.FirstOrDefault(u => u.ParentId == Guid.Empty.ToString());
				if (intRoot != null)
					intRoot.ParentId = interactingRoot.Id;

				var infRoot = inferiorUnits.FirstOrDefault(u => u.ParentId == Guid.Empty.ToString());
				if (infRoot != null)
					infRoot.ParentId = interactingRoot.Id;

				var supRoot = superiorUnits.FirstOrDefault(u => u.ParentId == Guid.Empty.ToString());
				if (supRoot != null)
					supRoot.ParentId = superiorRoot.Id;

				MilitaryUnits.Clear();
				MilitaryUnits.Add(interactingRoot);
				MilitaryUnits.Add(superiorRoot);
				MilitaryUnits.Add(inferiorRoot);

				foreach (var unit in inferiorUnits)
					MilitaryUnits.Add(unit);

				foreach (var unit in superiorUnits)
					MilitaryUnits.Add(unit);

				foreach (var unit in interactingUnits)
					MilitaryUnits.Add(unit);

			}
		}

		#endregion
	}
}