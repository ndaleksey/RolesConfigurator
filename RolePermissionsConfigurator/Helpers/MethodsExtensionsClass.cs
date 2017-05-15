using System;
using System.Globalization;
using Swsu.Lignis.Logger.PrtAgent;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;

namespace Swsu.Lignis.RolePermissionsConfigurator.Helpers
{
	public static class MethodsExtensionsClass
	{
		#region INFO extended methods

		public static void Info(this AppLogger logger, ELogMessageType messageType, string message)
		{
			Info(logger, messageType, message, CultureInfo.CurrentUICulture);
		}

		public static void Info(this AppLogger logger, ELogMessageType messageType, string message,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof (ELogMessageType), messageType);
			logger.Info(type, message, culture);
		}

		#endregion
		

		#region DEBUG extended methods

		public static void Debug(this AppLogger logger, ELogMessageType messageType, string message)
		{
			Debug(logger, messageType, message, CultureInfo.CurrentUICulture);
		}

		public static void Debug(this AppLogger logger, ELogMessageType messageType, string message,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof (ELogMessageType), messageType);
			logger.Debug(type, message, culture);
		}

		#endregion

		#region ERROR extended methods

		public static void Error(this AppLogger logger, ELogMessageType messageType, string message)
		{
			Error(logger, messageType, message, CultureInfo.CurrentUICulture);
		}

		public static void Error(this AppLogger logger, ELogMessageType messageType, string message,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof (ELogMessageType), messageType);
			logger.Error(type, message, culture);
		}

		public static void Error(this AppLogger logger, ELogMessageType messageType, Exception exception)
		{
			Error(logger, messageType, exception, CultureInfo.CurrentUICulture);
		}

		public static void Error(this AppLogger logger, ELogMessageType messageType, Exception exception,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof(ELogMessageType), messageType);
			logger.Error(type, exception, culture);
		}

		#endregion

		#region FATAL extended methods

		public static void Fatal(this AppLogger logger, ELogMessageType messageType, string message)
		{
			Fatal(logger, messageType, message, CultureInfo.CurrentUICulture);
		}

		public static void Fatal(this AppLogger logger, ELogMessageType messageType, string message,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof (ELogMessageType), messageType);
			logger.Fatal(type, message, culture);
		}

		public static void Fatal(this AppLogger logger, ELogMessageType messageType, Exception exception)
		{
			Fatal(logger, messageType, exception, CultureInfo.CurrentUICulture);
		}

		public static void Fatal(this AppLogger logger, ELogMessageType messageType, Exception exception,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof(ELogMessageType), messageType);
			logger.Fatal(type, exception, culture);
		}

		#endregion

		#region WARN extended methods

		public static void Warn(this AppLogger logger, ELogMessageType messageType, string message)
		{
			Warn(logger, messageType, message, CultureInfo.CurrentUICulture);
		}

		public static void Warn(this AppLogger logger, ELogMessageType messageType, string message,
			CultureInfo culture)
		{
			var type = Enum.GetName(typeof (ELogMessageType), messageType);
			logger.Warn(type, message, culture);
		}

		#endregion
	}
}