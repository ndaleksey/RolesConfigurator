using System.ComponentModel;

namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public enum ELogMessageType
	{
		[Description("StartApp")] ApplicationStart,

		[Description("StopApp")] ApplicationStop,

		[Description("WorkWithDb")] WorkWithDb,

		[Description("WriteIntoDb")] WriteIntoDb,

		[Description("ReadFromDb")] ReadFromDb,

		[Description("WriteIntoFile")] WriteIntoFile,

		[Description("ReadFromFile")] ReadFromFile,

		[Description("Process")] Process
	}
}