using RFIDUnlocker.GUI.Infrastructure.Commands;
using RFIDUnlocker.GUI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
		public string Password { get; set; }

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

		private RelayCommand _disconnectCOMPort;
		public RelayCommand DisconnectCOMPort
			=> _disconnectCOMPort ??= new(_ =>
			{
				//
				// TODO
				//
			});

		private RelayCommand _setPassword;
		public RelayCommand SetPassword
			=> _setPassword ??= new(_ =>
			{
				if (string.IsNullOrEmpty(Password) is false)
				{
					SecuritySettings settings = new(Password);
					SendRequest(Command.SetPassword, settings);
				}
			});

		private RelayCommand _addCard;
		public RelayCommand AddCard
			=> _addCard ??= new(_ =>
			{
				SendRequest(Command.Add);
			});

		#endregion

		private void SendRequest(string command, object? data = null)
		{
			string request = $"<?{command}";

			if (data is not null)
			{
				string json = JsonSerializer.Serialize(data, data.GetType());
				request += $"|{json}";
			}

			request += ">";

			if (_serialPort is not null)
			{
				_serialPort.Open();
				_serialPort.Write(request);
				_serialPort.Close();
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
