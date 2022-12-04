using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RFIDUnlocker.GUI.Models
{
    internal class Card : INotifyPropertyChanged
    {
        public string UID { get; init; }

        public string? _name;
        public string? Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public Card(string uid)
        {
            UID = uid;
        }

		public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Set<T>(ref T field, T value, [CallerMemberName] string? propretyName = null)
        {
            if (Equals(field, value)) return;

            field = value;
            OnPropertyChanged(propretyName);
        }
	}
}
