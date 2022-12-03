using System.Text.Json.Serialization;

namespace RFIDUnlocker.GUI.Models
{
	internal readonly struct SecuritySettings
	{
		[JsonPropertyName("password")]
		public string Password { get; }

		public SecuritySettings(string password)
		{
			Password = password;
		}
	}
}
