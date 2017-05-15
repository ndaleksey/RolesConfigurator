using System.ComponentModel;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	// TODO: локализовать дескрипшены
	public enum EWorkflowType
	{
		[Description("Нормальная работа")]
		NormalWork = 0,

		[Description("Работа с БД")]
		WorkWithDb = 1,
		LoadFromDb = 2,
		SaveToDb = 3
	}
}