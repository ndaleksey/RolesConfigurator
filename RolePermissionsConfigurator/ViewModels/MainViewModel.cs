using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class MainViewModel : CustomViewModel
	{
		#region Fields
		private Guid? _currentClusterId;
		private string _appTitle;

		private InternalRolesViewModel _internalRolesViewModel;
		private ExternalRolesViewModel _externalRolesViewModel;
		private DepartmentClustersViewModel _departmentClustersViewModel;
		#endregion

		#region Properties
		public string AppTitle
		{
			get { return _appTitle; }
			set { SetProperty(ref _appTitle, value, nameof(AppTitle)); }
		}

		public InternalRolesViewModel InternalRolesViewModel
		{
			get { return _internalRolesViewModel; }
			set { SetProperty(ref _internalRolesViewModel, value, nameof(InternalRolesViewModel)); }
		}

		public ExternalRolesViewModel ExternalRolesViewModel
		{
			get { return _externalRolesViewModel; }
			set { SetProperty(ref _externalRolesViewModel, value, nameof(ExternalRolesViewModel)); }
		}

		public DepartmentClustersViewModel DepartmentClustersViewModel
		{
			get { return _departmentClustersViewModel; }
			set { SetProperty(ref _departmentClustersViewModel, value, nameof(DepartmentClustersViewModel)); }
		}
		#endregion

		#region Construction

		public MainViewModel()
		{
			AppTitle = string.Empty;
			Initialization();
		}

		#endregion

		#region Methods
		protected sealed override async void Initialization()
		{
			try
			{
				using (var t = new Transaction())
					_currentClusterId = await DbService.GetCurrentRoleClusterAsync(t.Connection);

				if (_currentClusterId == null)
				{
					MessageBox.Show(Properties.Resources.RoleNotExistsException);
					return;
				}

				await CreateApplicationTitleAsync();

				InternalRolesViewModel = new InternalRolesViewModel(_currentClusterId.Value);
				ExternalRolesViewModel = new ExternalRolesViewModel();
				DepartmentClustersViewModel = new DepartmentClustersViewModel();
			}
			catch (Exception e)
			{
				//TODO: использовать систему логирования
				Debug.WriteLine(e);
			}
		}

		private async Task CreateApplicationTitleAsync()
		{
			if (!_currentClusterId.HasValue)
				throw new NullReferenceException(Properties.Resources.ClusterInitializationException);

			string depratment;

			using (var t = new Transaction())
				depratment = await DbService.GetDepartmentNameByClusterIdAsync(t.Connection, _currentClusterId.Value);

			AppTitle = Properties.Resources.ApplicationName;

			if (!string.IsNullOrEmpty(depratment))
				AppTitle += $"  ( {depratment} )";
		}
		#endregion
	}
}