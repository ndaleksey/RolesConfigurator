using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class ViewModelBaseExtended : ViewModelBase
	{
		#region Methods
		protected void SetProperty<T>(ref T storage, T value, string propertyName, BindableBaseExtanded.PropertyChangedCallback<T> changedCallback)
		{
			var oldValue = storage;

			if (SetProperty(ref storage, value, propertyName))
			{
				changedCallback(oldValue, value);
			}
		}
		#endregion
	}
}