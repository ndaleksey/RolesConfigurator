using DevExpress.Mvvm;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class BindableBaseExtanded : BindableBase
	{
		public delegate void PropertyChangedCallback<in T>(T oldValue, T newValue);

		#region Methods
		protected void SetProperty<T>(ref T storage, T value, string propertyName, PropertyChangedCallback<T> changedCallback)
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