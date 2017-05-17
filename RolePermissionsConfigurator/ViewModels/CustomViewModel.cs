using DevExpress.Mvvm;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.ViewModels
{
	public abstract class CustomViewModel : ViewModelBase
	{
		#region Fields

		private EWorkflowType _workflowType;

		#endregion

		#region Properties

		public EWorkflowType WorkflowType
		{
			get { return _workflowType; }
			set { SetProperty(ref _workflowType, value, nameof(WorkflowType)); }
		}

		#endregion

		#region Methods

		protected virtual void Initialization()
		{

		}

		#endregion
	}
}