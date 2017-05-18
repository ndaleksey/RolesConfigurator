using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Npgsql;
using NpgsqlTypes;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class DepartmentClustersViewModel : CustomViewModel
	{
		#region Fields

		private DepartmentItem _selectedDepartment;

		#endregion

		#region Properties

		public DepartmentItem SelectedDepartment
		{
			get { return _selectedDepartment; }
			set { SetProperty(ref _selectedDepartment, value, nameof(SelectedDepartment)); }
		}

		public ObservableCollection<DepartmentItem> Departments { get; } = new ObservableCollection<DepartmentItem>();

		public ObservableCollection<SubordinateItem> SubordinationTypes { get; } =
			new ObservableCollection<SubordinateItem>(
				new List<SubordinateItem>(new[]
				{
					new SubordinateItem(ESubordinate.Unknown),
					new SubordinateItem(ESubordinate.Superior),
					new SubordinateItem(ESubordinate.Inferior),
					new SubordinateItem(ESubordinate.Interacting)
				}));

		#endregion

		#region Commands

		public ICommand ChangeRecordCommand { get; set; }
		public ICommand ValidateClusterCommand { get; set; }

		#endregion

		#region Constructors

		public DepartmentClustersViewModel()
		{
			ChangeRecordCommand = new DelegateCommand(ChangeRecordAsync);
			ValidateClusterCommand = new DelegateCommand<ValidationParameters>(ValidateCode);
			Initialization();
		}

		#endregion

		#region Commands' methods

		//сохранение после изменении/добавлении кластеров и подчиненности
		private async void ChangeRecordAsync()
		{
			try
			{
				WorkflowType = EWorkflowType.SaveToDb;
				await ChangeRecord();
				MessageBox.Show("Изменение записи", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception e)
			{
				MessageBox.Show("Не возможно осуществить запись", "Внимание", MessageBoxButton.OK,
					MessageBoxImage.Information);
				Debug.WriteLine(e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		//Добавление записи в БД при редактировании группировки
		private Task ChangeRecord()
		{
			return Task.Run(() =>
			{
				var cmp = new DepartmentEqualityComparer();

				var itemsWithCluster = Departments.Where(i => i.Cluster.HasValue).ToList();
				var itemsWithDistinctCluster = itemsWithCluster.Distinct(cmp).ToList();

				// если есть повторяющиеся значения кластера
				if (itemsWithCluster.Count != itemsWithDistinctCluster.Count)
					throw new ApplicationException("Есть повторяющиеся значения номера кластера");

				using (var connection = new NpgsqlConnection(Settings.Default.ConnectionString))
				{
					connection.Open();

					using (var cmd = new NpgsqlCommand())
					{
						cmd.Connection = connection;
						using (var t = connection.BeginTransaction(IsolationLevel.Snapshot))
						{
							foreach (var i in Departments)
							{
								if (i.Cluster == 0) continue;
								cmd.Parameters.Clear();
								cmd.Parameters.AddWithValue("id", NpgsqlDbType.Uuid, i.Id);
								cmd.Parameters.AddWithValue("priority", NpgsqlDbType.Integer,
									i.Subordinate == ESubordinate.Superior
										? 1
										: i.Subordinate == ESubordinate.Inferior 
											? 2 : i.Subordinate == ESubordinate.Interacting ? 3 : 0);
								cmd.Parameters.AddWithValue("cluster", NpgsqlDbType.Integer,
									i.Cluster.HasValue ? (object) i.Cluster.Value : DBNull.Value);
								cmd.CommandText = i.Cluster.HasValue
									? "INSERT INTO permission.cluster VALUES (@id, @cluster, @priority) " +
									  "ON CONFLICT (id) DO UPDATE SET number = @cluster, priority = @priority;"
									: "DELETE FROM permission.cluster WHERE id = @id";
								cmd.ExecuteNonQuery();
							}
							t.Commit();
						}
					}
				}
			});
		}

		public void ValidateCode(ValidationParameters p)
		{
			var res = Departments.FirstOrDefault(x => Equals(x.Cluster, p.Value));
			p.Msg = res == null ? string.Empty : $"Value {p.Value} already exits.";
		}

		#endregion


		#region Methods

		protected sealed override async void Initialization()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				await LoadGroupingAsync();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				Helper.Logger.Error(ELogMessageType.ReadFromDb, e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

		private async Task LoadGroupingAsync()
		{
			var lowItems = await GetGroupingFromDb(ESubordinate.Inferior);
			var highItems = await GetGroupingFromDb(ESubordinate.Superior);
			var linkedItems = await GetGroupingFromDb(ESubordinate.Interacting);
			Departments.Clear();
			//                items.ForEach(i =>{Items.Add(i);});

			var inferiorParent = new DepartmentItem
			{
				Id = Guid.NewGuid(),
				Name = Properties.Resources.Inferior //"Нижестоящие"
			};
			var superiorParent = new DepartmentItem
			{
				Id = Guid.NewGuid(),
				Name = Properties.Resources.Superior //"Вышестоящие"
			};
			var interatingPerent = new DepartmentItem
			{
				Id = Guid.NewGuid(),
				Name = Properties.Resources.Interacting // "Взаимодействующие"
			};

			var lowParent = lowItems.FirstOrDefault(i => i.ParentId == Guid.Empty);
			if (lowParent != null)
				lowParent.ParentId = inferiorParent.Id;

			var highParent = highItems.FirstOrDefault(i => i.ParentId == Guid.Empty);
			if (highParent != null)
				highParent.ParentId = superiorParent.Id;

			var linkedParent = linkedItems.FirstOrDefault(i => i.ParentId == Guid.Empty);
			if (linkedParent != null)
				linkedParent.ParentId = interatingPerent.Id;

			Departments.Add(superiorParent);
			Departments.Add(inferiorParent);
			Departments.Add(interatingPerent);

			lowItems.ForEach(i => Departments.Add(i));
			highItems.ForEach(i => Departments.Add(i));
			linkedItems.ForEach(i => Departments.Add(i));
		}

		//Загрузка  группировки, номеров кластера и подчиненности из БД
		private Task<List<DepartmentItem>> GetGroupingFromDb(ESubordinate subordinate)
		{
			var items = new List<DepartmentItem>();

			return Task.Run(() =>
			{
				if (subordinate == ESubordinate.Unknown) return items;

				var view = subordinate == ESubordinate.Inferior
					? "inferior_grouping"
					: subordinate == ESubordinate.Superior ? "superior_grouping" : "interacting_grouping";


				using (var con = new NpgsqlConnection(Settings.Default.ConnectionString))
				{
					var sql =
						"SELECT g.id, g.parent_id, g.name, c.number AS cluster_number, c.priority " +
						$"FROM permission.{view} g " +
						"LEFT JOIN permission.cluster c " +
						"ON g.id = c.id";
					con.Open();
					using (var cmd = new NpgsqlCommand(sql, con))
					{
						using (var reader = cmd.ExecuteReader())
						{
							if (!reader.HasRows) return items;

							while (reader.Read())
							{
								var id = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
								var parentId = reader.IsDBNull(1)
									? Guid.Empty
									: reader.GetGuid(1);
								var name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
								var cluster = reader.IsDBNull(3) ? null : (int?) reader.GetInt32(3);
								var sub = reader.IsDBNull(4) ? 0 : reader.GetByte(4);

								items.Add(new DepartmentItem
								{
									Id = id,
									ParentId = parentId,
									Name = name,
									Cluster = cluster,
									Subordinate =
										sub == 1
											? ESubordinate.Superior
											: sub == 2
												? ESubordinate.Inferior
												: sub == 3 ? ESubordinate.Interacting : ESubordinate.Unknown
								});
							}
						}
					}
				}
				return items;
			});
		}

		#endregion
	}
}