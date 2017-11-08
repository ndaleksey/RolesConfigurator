using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class MainViewModel : CustomViewModel
	{
		#region Fields

		private Guid? _currentRoleClusterId;
		private string _appTitle;
		private string _currentDepartment;
		private bool? _isConnectionInitialized;

		private InternalRolesViewModel _internalRolesViewModel;
		private ExternalRolesViewModel _externalRolesViewModel;
		private DepartmentClustersViewModel _departmentClustersViewModel;


		private RolesViewModel _selectedTab;
		private int _tabIndex;
		private string _cultureName;

		#endregion

		#region Properties

		public static bool IsCultureChanged { get; private set; }

		public string CurrentDepartment
		{
			get { return _currentDepartment; }
			set
			{
				if (string.Equals(value, _currentDepartment)) return;

				_currentDepartment = value;

				if (!string.IsNullOrEmpty(CurrentDepartment))
					AppTitle = Properties.Resources.ApplicationName + $"  ( {CurrentDepartment} )";
			}
		}

		public string CultureName
		{
			get { return _cultureName; }
			set { SetProperty(ref _cultureName, value, nameof(CultureName)); }
		}

		public int TabIndex
		{
			get { return _tabIndex; }
			set { SetProperty(ref _tabIndex, value, nameof(TabIndex), SetActiveRolesViewModel); }
		}

		public bool? IsConnectionInitialized
		{
			get { return _isConnectionInitialized; }
			set { SetProperty(ref _isConnectionInitialized, value, nameof(IsConnectionInitialized)); }
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
		public ICommand ChangeCultureCommand { get; }
		public ICommand InitializeCommand { get; }
		public ICommand ReloadDataCommand { get; }

		#endregion

		#region Construction

		public MainViewModel()
		{
			AppTitle = string.Empty;
			AddRoleCommand = new DelegateCommand(AddRole, CanAddRole);
			ModifyRoleCommand = new DelegateCommand(ModifyRole, CanModifyRole);
			DeleteRoleCommand = new DelegateCommand(DeleteRole, CanDeleteRole);

			ChangeCultureCommand = new DelegateCommand(ChangeCulture, CanChangeCulture);
			InitializeCommand = new DelegateCommand(Initialize);
			ReloadDataCommand = new DelegateCommand(ReloadDataAsync, CanReloadData);

			InternalRolesViewModel = new InternalRolesViewModel();
			ExternalRolesViewModel = new ExternalRolesViewModel();

			DepartmentClustersViewModel = new DepartmentClustersViewModel();

			SelectedTab = InternalRolesViewModel;

			CultureName = Thread.CurrentThread.CurrentUICulture.Name;

//			Initialization();
		}

		#endregion

		#region Commands' methods

		private bool CanAddRole()
		{
			return IsConnectionInitialized.HasValue && IsConnectionInitialized.Value && SelectedTab != null &&
			       SelectedTab.CanAddRole();
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
			SelectedTab?.DeleteRole();
		}

		private bool CanChangeCulture()
		{
			return true;
		}

		private void ChangeCulture()
		{
			if (
				MessageBox.Show(Properties.Resources.ChangeLanguageRequest, Properties.Resources.LanguageChanging,
					MessageBoxButton.YesNo) == MessageBoxResult.No)
				return;

			Settings.Default.Culture = Settings.Default.Culture.Name == "ru-RU"
				? new CultureInfo("fr-FR")
				: new CultureInfo("ru-RU");

			Settings.Default.Save();

			IsCultureChanged = true;
			Application.Current.Shutdown(1);
		}

		private bool CanReloadData()
		{
			return true;
		}

		private void ReloadDataAsync()
		{
			Initialize();
		}

		#endregion

		#region Methods

		private async void Initialize()
		{
			IsConnectionInitialized = null;
			WorkflowType = EWorkflowType.LoadFromDb;

			try
			{
				using (var t = new Transaction())
				{
					_currentRoleClusterId = await DbService.GetCurrentRoleClusterAsync(t.Connection);

					if (_currentRoleClusterId == null)
						throw new RoleNotExistsException(Properties.Resources.RoleNotExistsException);

					CurrentDepartment = await DbService.GetDepartmentNameByClusterIdAsync(t.Connection, _currentRoleClusterId.Value);

					var subsystems = await DbService.GetSubsystemsAsync(t.Connection);

					if (subsystems.Any())
					{
						InternalRolesViewModel.Subsystems.Clear();
						ExternalRolesViewModel.Subsystems.Clear();
					}

					InternalRolesViewModel.Subsystems.AddRange(subsystems);
					ExternalRolesViewModel.Subsystems.AddRange(subsystems);
				}

				InternalRolesViewModel.CurrentClusterId = _currentRoleClusterId.Value;
				ExternalRolesViewModel.CurrentClusterId = _currentRoleClusterId.Value;
				InternalRolesViewModel.CurrentDepartment = CurrentDepartment;

				InternalRolesViewModel.Initialization();
				ExternalRolesViewModel.Initialization();
				DepartmentClustersViewModel.Initialization();
				DepartmentClustersViewModel.Initialization();

				IsConnectionInitialized = true;
			}
			catch (SocketException se)
			{
				Helper.LogFatal(se);
				MessageBox.Show(se.Message, LogMessages.LoadRolesError, MessageBoxButton.OK, MessageBoxImage.Stop);

				AppTitle = Properties.Resources.ApplicationName;

				IsConnectionInitialized = false;
			}
			catch (RoleNotExistsException re)
			{
				Helper.LogFatal(re);
				MessageBox.Show(re.Message, LogMessages.LoadRolesError, MessageBoxButton.OK, MessageBoxImage.Stop);
				Application.Current.Shutdown(0);
			}
			catch (PostgresException dbe)
			{
				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);

				Helper.LogError(e, dbe);
				MessageBox.Show(e, LogMessages.ReadFromDB, MessageBoxButton.OK, MessageBoxImage.Error);

				IsConnectionInitialized = false;
			}
			catch (Exception e)
			{
//				MessageBox.Show(e.Message);
				Helper.LogError(e);
			}
			finally
			{
				WorkflowType = EWorkflowType.NormalWork;
			}
		}

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
		
		#endregion
	}
}