using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	[DebuggerDisplay("Name = {Name}")]
	public class DepartmentItem : BindableBase, INotifyDataErrorInfo
	{
		#region Fields

		private bool _hasCluster;
		private int? _cluster;
		private bool _clusterIsNotUnique = false;

		#endregion

		#region Properties

		public Guid Id { get; set; }
		public Guid ParentId { get; set; }
		public string Name { get; set; }

		public int? Cluster
		{
			get { return _cluster; }
			set
			{
				if (value == _cluster)
					return;

				var oldValue = _cluster;
				_cluster = value;
				OnClusterNumberChanged(new PropertyValueChangedEventArgs<int?>(oldValue, value));
				RaisePropertyChanged(nameof(Cluster));
			}
		}

		public ESubordinate Subordinate { get; set; }

		public bool HasCluster
		{
			get { return _hasCluster; }
			set { SetProperty(ref _hasCluster, value, nameof(HasCluster)); }
		}

		public bool HasErrors => ClusterIsNotUnique;

		internal bool ClusterIsNotUnique
		{
			get { return _clusterIsNotUnique; }
			set
			{
				if (value == _clusterIsNotUnique)
					return;

				_clusterIsNotUnique = value;
				OnErrorsChanged(new DataErrorsChangedEventArgs(nameof(Cluster)));
			}
		}

		#endregion

		#region Constructors

		public DepartmentItem(bool hasCluster)
		{
			HasCluster = hasCluster;
		}

		public DepartmentItem(Guid id, Guid parentId, string name, int? cluster)
		{
			Id = id;
			ParentId = parentId;
			Name = name;
			Cluster = cluster;
		}

		#endregion

		#region Methods

		public IEnumerable GetErrors(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(Cluster):
					if (ClusterIsNotUnique)
						yield return Properties.Resources.ClusterNumberMustBeUnique;

					break;
			}
		}

		protected virtual void OnErrorsChanged(DataErrorsChangedEventArgs e)
		{
			ErrorsChanged?.Invoke(this, e);
		}

		protected virtual void OnClusterNumberChanged(PropertyValueChangedEventArgs<int?> e)
		{
			ClusterNumberChanged?.Invoke(this, e);
		}

		#endregion

		#region Events

		public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

		internal event PropertyValueChangedEventHandler<int?> ClusterNumberChanged;

		#endregion
	}
}