using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using RFIDUnlocker.GUI.Infrastructure.Commands;
using RFIDUnlocker.GUI.Models;
using RFIDUnlocker.GUI.Services.Data;
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
			using ApplicationContext context = new();
			context.Cards.Load();
			context.Actions.Load();

			Cards = new(context.Cards.ToList());
			Actions = new(context.Actions.Local.ToList());

			SystemEvents.SessionSwitch += OnSessionSwitch;

			COMPortNames = new(SerialPort.GetPortNames());
		}

		private readonly JsonSerializerOptions _serializerOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};
		
		private SerialPort? _serialPort;

		private bool _isScreenLocked = false;

		public ObservableCollection<Card> Cards { get; set; }
		public ObservableCollection<ActionInfo> Actions { get; set; }
		public ObservableCollection<string> COMPortNames { get; set; }
		public string? SelectedCOMPortName { get; set; }

		private bool _isPortConnected = false;
		public bool IsPortConnected
		{
			get => _isPortConnected;
			set => Set(ref _isPortConnected, value);
		}

		private string? _password;
		public string? Password
		{
			get => _password;
			set => Set(ref _password, value);
		}
		
		private Card? _selectedCard;
		public Card? SelectedCard
		{
			get => _selectedCard;
			set => Set(ref _selectedCard, value);
		}

		#region Commands

		private RelayCommand? _changePortConnectionState;
		public RelayCommand? ChangePortConnectionState 
			=> _changePortConnectionState ??= new(_ =>
			{
				try
				{
					if (_serialPort == null)
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
							IsPortConnected = true;
						}
					}
					else
					{
						if (IsPortConnected is true)
						{
							_serialPort.Close();
							IsPortConnected = false;
						}
						else
						{
							_serialPort.Open();
							IsPortConnected = true;
						}
					}
				}
				catch { }
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
					Password = string.Empty;
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
				if (SelectedCard != null)
				{
					using ApplicationContext context = new();
					context.Cards.Update(SelectedCard);
					context.SaveChanges();
				}
			});

		private RelayCommand? _deleteCard;
		public RelayCommand? DeleteCard
			=> _deleteCard ??= new(_ =>
			{
				if (SelectedCard != null)
				{
					using ApplicationContext context = new();
					context.Cards.Remove(SelectedCard);
					context.SaveChanges();

					Cards.Remove(SelectedCard);
				}
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
			string json = string.Empty;

			if (request.Contains('|'))
			{
				int firstBracketIndex = request.IndexOf("{");
				int lastBracketIndex = request.LastIndexOf("}");
				int length = lastBracketIndex - firstBracketIndex + 1;

				json = request.Substring(firstBracketIndex, length);
			}

			if (command == Command.Access)
			{
				Card? card = JsonSerializer.Deserialize<Card>(json, _serializerOptions);

				if (_isScreenLocked == true && Cards.Any(c => c.UID == card?.UID))
				{
					SendResponse(Command.Access, status: 10);
				}
				else
				{
					SendResponse(Command.Access, status: 11);
				}
			}
			else if (command == Command.Log)
			{
				ActionInfo? actionInfo = JsonSerializer.Deserialize<ActionInfo>(json, _serializerOptions);

				if (actionInfo != null)
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						Actions.Add(actionInfo);
					});

					using ApplicationContext context = new();
					context.Actions.Add(actionInfo);
					context.SaveChanges();
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

					Card? card = JsonSerializer.Deserialize<Card>(json, _serializerOptions);

					if (card is not null && !Cards.Any(c => c.UID == card.UID))
					{
						App.Current.Dispatcher.Invoke(() =>
						{
							Cards.Add(card);
						});

						using ApplicationContext context = new();
						context.Cards.Add(card);
						context.SaveChanges();

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

		private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			if (e.Reason == SessionSwitchReason.SessionLock)
			{
				_isScreenLocked = true;
			}
			else if (e.Reason == SessionSwitchReason.SessionUnlock)
			{
				_isScreenLocked = false;
			}
		}
	}
}
