using System.ComponentModel;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	// TODO: локализовать дескрипшены
	public enum EWorkflowType
	{
		[Description("Нормальная работа")]
		NormalWork,

		[Description("Работа с БД")]
		WorkWithDb,
		LoadFromDb,
		SaveToDb
	}
}