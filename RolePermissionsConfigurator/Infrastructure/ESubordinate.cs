namespace Swsu.Lignis.RolePermissionsConfigurator.Infrastructure
{
	/// <summary>
	/// Уровень подчинённости
	/// </summary>
	public enum ESubordinate
	{
		
		/// <summary>
		/// Не задано
		/// </summary>
		Unknown = 0,

		/// <summary>
		/// Вышестоящий
		/// </summary>
		Superior = 1,

		/// <summary>
		/// Нижестоящий
		/// </summary>
		Inferior = 2,

		/// <summary>
		/// Взаимодействующий
		/// </summary>
		Interacting = 3
	}
}
