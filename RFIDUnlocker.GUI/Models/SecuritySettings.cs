using System.Text.Json.Serialization;

namespace RFIDUnlocker.GUI.Models
{
	internal readonly struct SecuritySettings
	{
		public string Password { get; }

		public SecuritySettings(string password)
		{
			Password = password;
		}
	}
}
