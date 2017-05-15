using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels.Items
{
	public class Account : BindableBase
	{
		#region Fields
		private string _login;
		private string _name;
		private string _description;
		private Role _role;

		private bool _isSelected;
		private bool _isEnabled;
		
		#endregion

		#region Properties
		public string Login
		{
			get { return _login; }
			set { SetProperty(ref _login, value, nameof(Login)); }
		}
		public string Name
		{
			get { return _name; }
			set { SetProperty(ref _name, value, nameof(Name)); }
		}
		public string Description
		{
			get { return _description; }
			set { SetProperty(ref _description, value, nameof(Description)); }
		}

		public Role Role
		{
			get { return _role; }
			set { SetProperty(ref _role, value, nameof(Role)); }
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set { SetProperty(ref _isSelected, value, nameof(IsSelected)); }
		}
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { SetProperty(ref _isEnabled, value, nameof(IsEnabled)); }
		}

		#endregion

		#region Constructors
		public Account(string login, string name, string description, Role role = null)
		{
			Login = login;
			Name = name;
			Description = description;
			Role = role;
		}
		#endregion

		#region Methods

		public override string ToString()
		{
			return Login;
		}

		#endregion
	}
}