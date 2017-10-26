namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	public class RoleNotExistsException : System.Exception
	{
		public RoleNotExistsException(string message) : base(message)
		{
		}
	}
}