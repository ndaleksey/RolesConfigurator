using System.Threading;
using System.Windows;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;

namespace Swsu.Lignis.RolePermissionsConfigurator
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			
			Thread.CurrentThread.CurrentCulture = Settings.Default.Culture;
			Thread.CurrentThread.CurrentUICulture = Settings.Default.Culture;

			Helper.Logger.Info(ELogMessageType.ApplicationStart, LogMessages.StartApplication);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Helper.Logger.Info(ELogMessageType.ApplicationStop, LogMessages.StopApplication);
		}
	}
}
