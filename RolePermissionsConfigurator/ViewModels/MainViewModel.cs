using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm;
using Npgsql;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public class MainViewModel : CustomViewModel
	{
		#region Fields

		private Guid? _currentRoleClusterId;
		private string _appTitle;

		private InternalRolesViewModel _internalRolesViewModel;
		private ExternalRolesViewModel _externalRolesViewModel;
		private DepartmentClustersViewModel _departmentClustersViewModel;


		private RolesViewModel _selectedTab;
		private int _tabIndex;
		private string _cultureName;

		#endregion

		#region Properties
		
		public static bool IsCultureChanged { get; private set; }

		public string CurrentDepartment { get; private set; }

		public string CultureName
		{
			get { return _cultureName;}
			set { SetProperty(ref _cultureName, value, nameof(CultureName)); }
		}

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
		public ICommand ChangeCultureCommand { get; }
		public AsyncCommand ReloadDataCommand { get; }

		#endregion

		#region Construction

		public MainViewModel()
		{
			AppTitle = string.Empty;
			AddRoleCommand = new DelegateCommand(AddRole, CanAddRole);
			ModifyRoleCommand = new DelegateCommand(ModifyRole, CanModifyRole);
			DeleteRoleCommand = new DelegateCommand(DeleteRole, CanDeleteRole);

			ChangeCultureCommand = new DelegateCommand(ChangeCulture, CanChangeCulture);

			ReloadDataCommand = new AsyncCommand(ReloadDataAsync, CanReloadData);

			CultureName = Thread.CurrentThread.CurrentUICulture.Name;

			Initialization();
		}
		
		#endregion

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

		private Task ReloadDataAsync()
		{
			return Task.Run(() => ReloadData());
		}
		
		#endregion

		#region Methods

		protected new async void Initialization()
		{
			try
			{
				using (var t = new Transaction())
					_currentRoleClusterId = await DbService.GetCurrentRoleClusterAsync(t.Connection);

				if (_currentRoleClusterId == null)
					throw new RoleNotExistsException(Properties.Resources.RoleNotExistsException);

				var roleClusterId = _currentRoleClusterId.Value;

				await InitializeApplicationTitleAsync();

				List<Subsystem> subsystems;

				using (var t = new Transaction())
					subsystems = await DbService.GetSubsystemsAsync(t.Connection);

				InternalRolesViewModel = new InternalRolesViewModel(roleClusterId, CurrentDepartment);
				InternalRolesViewModel.Subsystems.Clear();
				InternalRolesViewModel.Subsystems.AddRange(subsystems);

				ExternalRolesViewModel = new ExternalRolesViewModel(roleClusterId);
				ExternalRolesViewModel.Subsystems.Clear();
				ExternalRolesViewModel.Subsystems.AddRange(subsystems);

				DepartmentClustersViewModel = new DepartmentClustersViewModel();

				SelectedTab = InternalRolesViewModel;
			}
			catch (RoleNotExistsException re)
			{
				Debug.WriteLine(re);
				Helper.Logger.Error(Properties.Resources.LogSource, re.Message);
				MessageBox.Show(re.Message, LogMessages.LoadRolesError, MessageBoxButton.OK, MessageBoxImage.Stop);
				Application.Current.Shutdown(0);
			}
			catch (PostgresException dbe)
			{
				Debug.WriteLine(dbe);

				var e = Helper.GetPostgresErrorDescriptionBySqlState(dbe.SqlState);

				Helper.Logger.Error(Properties.Resources.LogSource, e, dbe);
				Helper.ModuleScmf.AddError(dbe.Message);
				MessageBox.Show(e, LogMessages.ReadFromDB, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
//				MessageBox.Show(e.Message);
				Helper.Logger.Error(Properties.Resources.LogSource, e);
				Helper.ModuleScmf.AddError(e.Message);
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

		private async Task InitializeApplicationTitleAsync()
		{
			if (!_currentRoleClusterId.HasValue)
				throw new NullReferenceException(Properties.Resources.ClusterInitializationException);


			using (var t = new Transaction())
				CurrentDepartment = await DbService.GetDepartmentNameByClusterIdAsync(t.Connection, _currentRoleClusterId.Value);

			AppTitle = Properties.Resources.ApplicationName;

			if (!string.IsNullOrEmpty(CurrentDepartment))
				AppTitle += $"  ( {CurrentDepartment} )";
		}

		private void ReloadData()
		{
			
		}

		#endregion
	}
}