using System;
using System.Text.Json.Serialization;

namespace RFIDUnlocker.GUI.Models
{
	internal class ActionInfo
	{
		public ActionInfo() { }

		public ActionInfo(string uid, ActionType type)
		{
			UID = uid;
			Type = type;
		}

		public int Id { get; set; }
		public string UID { get; init; }
		public DateTime Date { get; init; } = DateTime.Now;

		[JsonPropertyName("action")]
		public ActionType Type { get; init; }

		public override string ToString()
		{
			string actionMessage = Type switch
			{
				ActionType.Entered => "Успешный вход",
				ActionType.NotEntered => "Попытка входа",
				_ => "Неизвестное действие",
			};
			return $"{Date:dd.MM.yy HH.mm} | [{UID}] {actionMessage}";
		}
	}

	internal enum ActionType
	{
		Entered,
		NotEntered
	}
}
