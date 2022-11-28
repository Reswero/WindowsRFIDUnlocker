using RFIDUnlocker.GUI.Infrastructure.Commands;
using RFIDUnlocker.GUI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace RFIDUnlocker.GUI.ViewModels
{
	internal class MainWindowViewModel : INotifyPropertyChanged
	{
		public MainWindowViewModel()
		{
			COMPortNames = new(SerialPort.GetPortNames());
		}

		private SerialPort _serialPort;

		public ObservableCollection<string> COMPortNames { get; set; }
		public string SelectedCOMPortName { get; set; }

		#region

		private RelayCommand _connectCOMPort;
		public RelayCommand ConnectCOMPort 
			=> _connectCOMPort ??= new(_ =>
			{
				if (SelectedCOMPortName != null)
				{
					_serialPort = new()
					{
						PortName = SelectedCOMPortName,
						BaudRate = 9600,
						DataBits = 8,
						ReadTimeout = 500,
						WriteTimeout = 500
					};
				}
			});

		private RelayCommand _addCard;
		public RelayCommand AddCard
			=> _addCard ??= new(_ =>
			{
				if (_serialPort != null)
				{
					SendRequest(Command.Add);
				}
			});

		#endregion

		private void SendRequest(string command)
		{
			string request = $"<?{command}>";

			_serialPort.Open();
			_serialPort.Write(request);
			_serialPort.Close();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
