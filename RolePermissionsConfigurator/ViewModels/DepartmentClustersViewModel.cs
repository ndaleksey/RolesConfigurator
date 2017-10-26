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
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{

	public class DepartmentClustersViewModel : CustomViewModel
	{
		#region Fields

		private DepartmentItem _selectedDepartment;

		private readonly Dictionary<int, HashSet<DepartmentItem>> _departmentItemsByClusterNumber
			= new Dictionary<int, HashSet<DepartmentItem>>();

		#endregion

		#region Properties

		public DepartmentItem SelectedDepartment
		{
			get { return _selectedDepartment; }
			set { SetProperty(ref _selectedDepartment, value, nameof(SelectedDepartment)); }
		}

		public ObservableCollection<DepartmentItem> Departments { get; }

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
			Departments = new DepartmentItemCollection(this);
			ChangeRecordCommand = new DelegateCommand(ChangeRecordAsync);
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
				MessageBox.Show(Properties.Resources.ClusterAppointmentComplete, Properties.Resources.ClusterModification,
					MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);

				Helper.Logger.Error(Properties.Resources.LogSource, e, dbe);
				Helper.ModuleScmf.AddError(e);

				MessageBox.Show(e, LogMessages.ReadFromDB, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (ApplicationException e)
			{
				Debug.WriteLine(e);
				MessageBox.Show(e.Message, Properties.Resources.ClusterAppointmentError, MessageBoxButton.OK,
					MessageBoxImage.Error);
				Helper.Logger.Error(Properties.Resources.LogSource, Properties.Resources.ClusterAppointmentError, e);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				MessageBox.Show(e.Message, Properties.Resources.ClusterAppointmentError, MessageBoxButton.OK,
					MessageBoxImage.Error);
				Helper.Logger.Error(Properties.Resources.LogSource, Properties.Resources.ClusterAppointmentError, e);
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

				var depsWithCluster = Departments.Where(i => i.Cluster.HasValue).ToList();
				var depsWithDistinctCluster = depsWithCluster.Distinct(cmp).ToList();

				// если есть повторяющиеся значения кластера
				if (depsWithCluster.Count != depsWithDistinctCluster.Count)
					throw new ApplicationException(Properties.Resources.ClusterDuplicationMessage);

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
											? 2
											: i.Subordinate == ESubordinate.Interacting ? 3 : 0);
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

		#endregion


		#region Methods

		public sealed override async void Initialization()
		{
			try
			{
				WorkflowType = EWorkflowType.LoadFromDb;

				await LoadGroupingAsync();
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);
				
				Helper.Logger.Error(Properties.Resources.LogSource, e, dbe);
				Helper.ModuleScmf.AddError(e);

				MessageBox.Show(e, LogMessages.ReadFromDB, MessageBoxButton.OK, MessageBoxImage.Error);

			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				MessageBox.Show(e.Message, Properties.Resources.LoadGroupingError, MessageBoxButton.OK, MessageBoxImage.Error);
				Helper.Logger.Error(Properties.Resources.LogSource, e);
				Helper.ModuleScmf.AddError(e.Message);
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

			var inferiorParent = new DepartmentItem(false)
			{
				Id = Guid.NewGuid(),
				Name = Properties.Resources.Inferior //"Нижестоящие"
			};
			var superiorParent = new DepartmentItem(false)
			{
				Id = Guid.NewGuid(),
				Name = Properties.Resources.Superior //"Вышестоящие"
			};
			var interatingPerent = new DepartmentItem(false)
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

								items.Add(new DepartmentItem(true)
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

		private void OnDepartmentItemClusterNumberChanged(object sender, PropertyValueChangedEventArgs<int?> e)
		{
			var item = (DepartmentItem) sender;

			if (e.OldValue.HasValue)
				UnregisterClusterNumber(e.OldValue.Value, item);

			if (e.NewValue.HasValue)
				RegisterClusterNumber(e.NewValue.Value, item);
		}

		private void OnDepartmentItemRemoving(DepartmentItem item)
		{
			item.ClusterNumberChanged -= OnDepartmentItemClusterNumberChanged;

			if (item.Cluster.HasValue)
				UnregisterClusterNumber(item.Cluster.Value, item);
		}

		private void OnDepartmentItemAdded(DepartmentItem item)
		{
			if (item.Cluster.HasValue)
				RegisterClusterNumber(item.Cluster.Value, item);

			item.ClusterNumberChanged += OnDepartmentItemClusterNumberChanged;
		}

		private void RegisterClusterNumber(int value, DepartmentItem item)
		{
			HashSet<DepartmentItem> groupingItems;

			if (!_departmentItemsByClusterNumber.TryGetValue(value, out groupingItems))
				_departmentItemsByClusterNumber.Add(value, groupingItems = new HashSet<DepartmentItem>());

			switch (groupingItems.Count)
			{
				case 0:
					break;

				case 1:
					foreach (var i in groupingItems)
						i.ClusterIsNotUnique = true;

					goto default;

				default:
					item.ClusterIsNotUnique = true;
					break;
			}

			if (!groupingItems.Add(item))
				return; // Should never happen.
		}

		private void UnregisterClusterNumber(int value, DepartmentItem item)
		{
			HashSet<DepartmentItem> groupingItems;

			if (!_departmentItemsByClusterNumber.TryGetValue(value, out groupingItems))
				return; // Should never happen.

			if (!groupingItems.Remove(item))
				return; // Should never happen.

			switch (groupingItems.Count)
			{
				case 0:
					_departmentItemsByClusterNumber.Remove(value);
					break;

				case 1:
					foreach (var i in groupingItems)
						i.ClusterIsNotUnique = false;

					goto default;

				default:
					item.ClusterIsNotUnique = false;
					break;
			}
		}

		#endregion

		#region Nested Types

		public class DepartmentItemCollection : ObservableCollection<DepartmentItem>
		{
			#region Fields

			private readonly DepartmentClustersViewModel _outer;

			#endregion

			#region Constructors

			public DepartmentItemCollection(DepartmentClustersViewModel outer)
			{
				_outer = outer;
			}

			#endregion

			#region Methods

			protected override void ClearItems()
			{
				foreach (var item in Items)
					_outer.OnDepartmentItemRemoving(item);

				base.ClearItems();
			}

			protected override void InsertItem(int index, DepartmentItem item)
			{
				base.InsertItem(index, item);
				_outer.OnDepartmentItemAdded(item);
			}

			protected override void RemoveItem(int index)
			{
				_outer.OnDepartmentItemRemoving(Items[index]);
				base.RemoveItem(index);
			}

			protected override void SetItem(int index, DepartmentItem item)
			{
				_outer.OnDepartmentItemRemoving(Items[index]);
				base.SetItem(index, item);
				_outer.OnDepartmentItemAdded(item);
			}

			#endregion
		}

		#endregion
	}
}