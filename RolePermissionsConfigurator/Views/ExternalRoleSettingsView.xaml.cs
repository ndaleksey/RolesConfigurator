using System.Windows.Controls;
using System.Windows.Input;

namespace Swsu.Lignis.RolePermissionsConfigurator.Views
{
	/// <summary>
	/// Логика взаимодействия для ExternalRoleSettingsView.xaml
	/// </summary>
	public partial class ExternalRoleSettingsView : UserControl
	{
		public ExternalRoleSettingsView()
		{
			InitializeComponent();
		}

		private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if (!char.IsDigit(e.Text, 0))
				e.Handled = true;
		}
	}
}
