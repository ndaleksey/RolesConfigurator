using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
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


		private RolesViewModel _selectedTab;
		private int _tabIndex;
		#endregion

		#region Properties

		public int TabIndex
		{
			get { return _tabIndex; }
			set { SetProperty(ref _tabIndex, value, nameof(TabIndex), SetActiveRolesViewModel); }
		}

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

		public RolesViewModel SelectedTab
		{
			get { return _selectedTab; }
			set { SetProperty(ref _selectedTab, value, nameof(SelectedTab)); }
		}
		#endregion

		#region Commands

		public ICommand AddRoleCommand { get; }
		public ICommand ModifyRoleCommand { get; }
		public ICommand DeleteRoleCommand { get; }

		#endregion

		#region Construction

		public MainViewModel()
		{
			AppTitle = string.Empty;
			AddRoleCommand = new DelegateCommand(AddRole, CanAddRole);
			ModifyRoleCommand = new DelegateCommand(ModifyRole, CanModifyRole);
			DeleteRoleCommand = new DelegateCommand(DeleteRole, CanDeleteRole);

			Initialization();
		}

		#endregion

		private void SetActiveRolesViewModel()
		{
			switch (TabIndex)
			{
				case 0:
					SelectedTab = InternalRolesViewModel;
					break;
				case 1:
					SelectedTab = ExternalRolesViewModel;
					break;
				default:
					SelectedTab = null;
					break;
			}
		}

		#region Commands' methods
		private bool CanAddRole()
		{
			return SelectedTab != null && SelectedTab.CanAddRole();
		}

		private void AddRole()
		{
			SelectedTab?.AddRole();
		}

		private bool CanModifyRole()
		{
			return SelectedTab != null && SelectedTab.CanModifyRole();
		}

		private void ModifyRole()
		{
			SelectedTab?.ModifyRole();
		}

		private bool CanDeleteRole()
		{
			return SelectedTab != null && SelectedTab.CanDeleteRole();
		}

		private void DeleteRole()
		{
			SelectedTab.DeleteRole();
		}
		#endregion

		#region Methods

		protected new async void Initialization()
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
				ExternalRolesViewModel = new ExternalRolesViewModel(_currentClusterId.Value);
				DepartmentClustersViewModel = new DepartmentClustersViewModel();

				TabIndex = 0;
			}
			catch (Exception e)
			{
				//TODO: обработать логгером
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