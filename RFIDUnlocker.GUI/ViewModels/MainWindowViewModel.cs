using RFIDUnlocker.GUI.Infrastructure.Commands;
using RFIDUnlocker.GUI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
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

		private SerialPort? _serialPort;

		private JsonSerializerOptions _serializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();
		public ObservableCollection<string> COMPortNames { get; set; }
		public string? SelectedCOMPortName { get; set; }
		public string? Password { get; set; }
		
		private Card _selectedCard;
		public Card SelectedCard
		{
			get => _selectedCard;
			set => Set(ref _selectedCard, value);
		}

		#region Commands

		private RelayCommand? _connectCOMPort;
		public RelayCommand? ConnectCOMPort 
			=> _connectCOMPort ??= new(_ =>
			{
				if (SelectedCOMPortName != null)
				{
					_serialPort = new()
					{
						PortName = SelectedCOMPortName,
						BaudRate = 9600,
						DtrEnable = true
					};
					_serialPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveMessage);

					_serialPort.Open();
				}
			});

		private RelayCommand? _disconnectCOMPort;
		public RelayCommand? DisconnectCOMPort
			=> _disconnectCOMPort ??= new(_ =>
			{
				_serialPort?.Close();
			});

		private RelayCommand? _setPassword;
		public RelayCommand? SetPassword
			=> _setPassword ??= new(_ =>
			{
				if (string.IsNullOrEmpty(Password) is false)
				{
					SecuritySettings settings = new(Password);
					SendRequest(Command.SetPassword, settings);
				}
			});

		private RelayCommand? _addCard;
		public RelayCommand? AddCard
			=> _addCard ??= new(_ =>
			{
				SendRequest(Command.Add);			
			});

		private RelayCommand? _saveCard;
		public RelayCommand? SaveCard
			=> _saveCard ??= new(_ =>
			{
				// TODO
			});

		private RelayCommand? _deleteCard;
		public RelayCommand? DeleteCard
			=> _deleteCard ??= new(_ =>
			{
				Cards.Remove(SelectedCard);
			});

		private RelayCommand? _close;
		public RelayCommand? Close
			=> _close ??= new(_ =>
			{
				_serialPort?.Close();
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

			_serialPort?.Write(request);
		}

		private void SendResponse(string command, int? status = null, object? data = null)
		{
			string response = $"<!{command}";

			if (status is not null)
			{
				response += status.ToString();
			}

			if (data is not null)
			{
				string json = JsonSerializer.Serialize(data, data.GetType());
				response += $"|{json}";
			}

			response += ">";

			_serialPort?.Write(response);
		}

		private void ReceiveMessage(object sender, SerialDataReceivedEventArgs e)
		{
			string message = string.Empty;

			if (_serialPort?.BytesToRead > 0)
			{
				message = _serialPort.ReadLine();
			}

			if (message.StartsWith("<!"))
			{
				HandleResponse(message);
			}
			else if (message.StartsWith("<?"))
			{
				HandleRequest(message);
			}
		}

		private void HandleRequest(string request)
		{
			string command = request.Substring(startIndex: 2, length: 3);

			if (command == Command.Access)
			{
				int firstBracketIndex = request.IndexOf("{");
				int lastBracketIndex = request.LastIndexOf("}");
				int length = lastBracketIndex - firstBracketIndex + 1;

				string json = request.Substring(firstBracketIndex, length);

				Card? card = JsonSerializer.Deserialize<Card>(json, _serializerOptions);

				if (Cards.Any(c => c.UID == card?.UID))
				{
					SendResponse(Command.Access, status: 10);
				}
				else
				{
					SendResponse(Command.Access, status: 11);
				}
			}
		}

		private void HandleResponse(string response)
		{
			string command = response.Substring(startIndex: 2, length: 3);

			if (command == Command.Add)
			{
				if (response.Contains('|'))
				{
					int firstBracketIndex = response.IndexOf('{');
					int lastBracketIndex = response.LastIndexOf('}');
					int length = lastBracketIndex - firstBracketIndex + 1;

					string json = response.Substring(firstBracketIndex, length);

					Card card = JsonSerializer.Deserialize<Card>(json, _serializerOptions);

					if (!Cards.Any(c => c.UID == card.UID))
					{
						App.Current.Dispatcher.Invoke(() =>
						{
							Cards.Add(card);
						});

						SelectedCard = card;
					}
				}
				else
				{
					int status = int.Parse(response.Substring(5, 2));
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (Equals(field, value)) return;

			field = value;
			OnPropertyChanged(propertyName);
		}
	}
}
