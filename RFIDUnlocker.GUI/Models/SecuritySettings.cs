namespace RFIDUnlocker.GUI.Models
{
	internal struct SecuritySettings
	{
		public string Password;

		public SecuritySettings(string password)
		{
			Password = password;
		}
	}
}
