using System;
using Swsu.Lignis.Logger.PrtAgent;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	public static class Helper
	{
		#region Properties
		public static AppLogger Logger { get; }

		#endregion

		#region Constructor

		static Helper()
		{
			Logger = new AppLogger(null, Properties.Resources.AppName, -1);
		}

		#endregion

		#region Methods
		
		public static string GetPostgresErrorDescriptionBySqlState(string state)
		{
			if (string.IsNullOrEmpty(state)) return "Sql Error is null or empty";

			var code = state.ToUpperInvariant().Trim();
			switch (code)
			{
				case "08000":
					return PostgreSQLErrorCodes.Err08000;

				case "08003":
					return PostgreSQLErrorCodes.Err08003;

				case "08006":
					return PostgreSQLErrorCodes.Err08006;

				case "28000":
					return PostgreSQLErrorCodes.Err28000;

				case "28P01":
					return PostgreSQLErrorCodes.Err28P01;

				case "3D000":
					return PostgreSQLErrorCodes.Err3D000;

				case "3F000":
					return PostgreSQLErrorCodes.Err3F000;

				case "42601":
					return PostgreSQLErrorCodes.Err42601;

				case "57P03":
					return PostgreSQLErrorCodes.Err57P03;
			}

			return $"Unspecified error (Code: {code})";
		}
		#endregion
	}
}