using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows;
using Swsu.Lignis.RolePermissionsConfigurator.Helpers;
using Swsu.Lignis.RolePermissionsConfigurator.Infrastructure;
using Swsu.Lignis.RolePermissionsConfigurator.Properties;
using Swsu.Lignis.RolePermissionsConfigurator.Resources;
using Swsu.Lignis.RolePermissionsConfigurator.ViewModels;

namespace Swsu.Lignis.RolePermissionsConfigurator
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	public partial class App : Application
	{
		private Mutex _mutex;

		protected override void OnStartup(StartupEventArgs e)
		{
			bool createdNew;
			_mutex = new Mutex(true, "RolePermissionProgramme", out createdNew);

			if (!createdNew)
				Shutdown();

			CultureInfo.DefaultThreadCurrentCulture = Settings.Default.Culture;
			CultureInfo.DefaultThreadCurrentUICulture = Settings.Default.Culture;

			Thread.CurrentThread.CurrentCulture = Settings.Default.Culture;
			Thread.CurrentThread.CurrentUICulture = Settings.Default.Culture;

			AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
			AppDomain.CurrentDomain.ProcessExit += OnCurrentDomainProcessExit;

			Helper.Logger.Info(ELogMessageType.ApplicationStart, LogMessages.StartApplication);
		}

		private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
		{
			Helper.Logger.Error(ELogMessageType.Process, ((Exception) args.ExceptionObject));
		}

		private void OnCurrentDomainProcessExit(object sender, EventArgs e)
		{
			// если была изменена культура
			if (MainViewModel.IsCultureChanged)
			{
				var process = Process.Start(Assembly.GetEntryAssembly().Location);
				process?.Dispose();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			Helper.Logger.Info(ELogMessageType.ApplicationStop, LogMessages.StopApplication);
		}
		
	}
}
