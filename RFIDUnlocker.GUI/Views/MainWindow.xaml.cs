using System.Windows;
using System.Windows.Forms;

namespace RFIDUnlocker.GUI.Views
{
	public partial class MainWindow : Window
	{

		public MainWindow()
		{
			InitializeComponent();
		}

		private NotifyIcon? _notifyIcon;

		private void Window_StateChanged(object sender, System.EventArgs e)
		{
			if (WindowState == WindowState.Minimized)
			{
				_notifyIcon = new()
				{
					Icon = new("Resources/Lock64x64.ico"),
					Visible = true,
					Text = "RFID Unlocker"
				};
				_notifyIcon.Click += NotifyIcon_Clicked;

				Hide();
			}
		}

		private void NotifyIcon_Clicked(object? sender, System.EventArgs e)
		{
			Show();
			WindowState = WindowState.Normal;
			Activate();

			_notifyIcon?.Dispose();
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_notifyIcon?.Dispose();
		}
	}
}
